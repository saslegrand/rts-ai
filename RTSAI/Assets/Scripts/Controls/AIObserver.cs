using RTS.AI.Tools;
using RTS.Waypoint;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RTS.AI
{
    public class AIObserver : MonoBehaviour
    {
        [SerializeField] private UtilityBlackboard _gameBlackboard;

        [SerializeField] private InfluenceMap _influenceMap;
        [SerializeField] private WaypointGraph _waypointGraph;
        [SerializeField] private AIController _selfController;
        [SerializeField] private TeamController _opponentController;

        [Header("Values")]
        [SerializeField] private int _buildingPointsThreshold;

        private void ObserveArmy()
        {
            ETeam opponentTeam = GameServices.GetOpponent(_selfController.GetTeam());
            int totalUnitCount = _selfController.UnitList.Count + _opponentController.UnitList.Count;

            // Army power count
            float allyArmyPowerCount = totalUnitCount == 0 ? 0f : _selfController.UnitList.Count / (float)totalUnitCount;
            _gameBlackboard.SetValueByName("AllyArmyPowerCount", allyArmyPowerCount);

            // Army power influence
            float allyArmyInfluence = _influenceMap.GetTeamInfluencePercentage(_selfController.GetTeam());
            _gameBlackboard.SetValueByName("AllyArmyInfluencePower", allyArmyInfluence);

            // Army field possession
            float allyArmyPossession = _influenceMap.GetTeamPossessionPercentage(_selfController.GetTeam());
            _gameBlackboard.SetValueByName("AllyArmyGlobalPossession", allyArmyPossession);
            float enemyArmyPossession = _influenceMap.GetTeamPossessionPercentage(opponentTeam);
            _gameBlackboard.SetValueByName("EnemyArmyGlobalPossession", enemyArmyPossession);
        }

        private void ObserveBuildings()
        {
            // Building power
            int buildingCount = TargetBuildingMap.Instance.Buildings.Length;
            float allyBuildingPower = buildingCount == 0 ? 0f : _selfController.CapturedTargets / (float)buildingCount;
            _gameBlackboard.SetValueByName("AllyBuildingPower", allyBuildingPower);

            float enemyBuildingPower = buildingCount == 0 ? 0f : _opponentController.CapturedTargets / (float)buildingCount;
            _gameBlackboard.SetValueByName("EnemyBuildingPower", enemyBuildingPower);

            // Building points
            float allyBuildingPointsPower = _buildingPointsThreshold == 0 ? 0f : _selfController.TotalBuildPoints / (float)_buildingPointsThreshold;
            _gameBlackboard.SetValueByName("AllyBuildingPointsPower", Mathf.Clamp(allyBuildingPointsPower, 0f, 1f));
        }

        public List<Factory> GetAttackedFactories()
        {
            return _selfController.FactoryList.Where(factory => factory.IsAttacked).ToList();
        }

        public List<Factory> GetOpponentFactories()
        {
            return _opponentController.FactoryList;
        }

        public void UpdateBlackboard()
        {
            if (!_gameBlackboard)
                return;

            // Global instances updates
            _influenceMap.ComputeMap();

            // BLACKBOARD VALUES
            // Power balance from unit influence
            // Power balance from unit amount
            // Power balance mines/resources
            // Team influence dispersion

            ObserveArmy();
            ObserveBuildings();
        }
    }
}