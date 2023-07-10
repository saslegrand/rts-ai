using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RTS.AI.Tools;
using UnityEngine;

namespace RTS.AI
{
    public class AIPlayer : MonoBehaviour
    {
        [SerializeField] private AIController _controller;
        [SerializeField] private ArmyLeader _armyLeader;
        [SerializeField] private TeamLeader _teamLeader;

        [Header("Team Leader")]
        [SerializeField] private AnimationCurve _produceFactoryChance;
        [SerializeField] private AnimationCurve _produceFactoryChanceFromBuildPoints;

        [Header("Observations")]
        [SerializeField] private AIObserver _observer;
        [SerializeField] private UtilitySystem _utilitySystemArmy;
        [SerializeField] private UtilitySystem _utilitySystemTeam;
        [SerializeField] private float _armyUpdateFrequency;
        [SerializeField] private float _teamUpdateFrequency;

        private bool _isDestroyed;

        private void FindStrategicPlan()
        {
            // Strategic behavior
            // Create a strategy and give orders/directions to the Army Leader

            // Army utility
            UtilityAction[] orderedArmyGoals = _utilitySystemArmy.EvaluateOrdered();
            
            List<Factory> defendTargets = _observer.GetAttackedFactories();
            List<Factory> attackTargets = _observer.GetOpponentFactories();
            List<TargetBuilding> captureTargets = TargetBuildingMap.Instance.GetAvailableTargetBuildings(_controller.GetTeam());
            _armyLeader.InterpretGoals(orderedArmyGoals, defendTargets, attackTargets, captureTargets);
            
            // Team utility
            UtilityAction[] orderedTeamGoals = _utilitySystemTeam.EvaluateOrdered();
            _teamLeader.InterpretGoals(orderedTeamGoals);
        }

        private void FindProductionPlan()
        {
            int count = _observer.GetAttackedFactories().Count;
            bool canProduceFactory = count == 0;

            if (canProduceFactory)
            {
                canProduceFactory = false;

                int factoryNb = _controller.FactoryList.Count;
                float factoryChance = _produceFactoryChance.Evaluate(factoryNb);
                float factoryChanceCost = _produceFactoryChanceFromBuildPoints.Evaluate(_controller.TotalBuildPoints);

                float rng = UnityEngine.Random.Range(0.0f, 1.0f);

                if (factoryChance + factoryChanceCost > rng)
                    canProduceFactory = true;
            }

            bool canProduceUnits = true;
            _teamLeader.Produce(canProduceFactory, canProduceUnits);
        }

        private async UniTask ObservationRoutine()
        {
            while (!_isDestroyed)
            {
                _observer.UpdateBlackboard();

                FindStrategicPlan();

                await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(_armyUpdateFrequency)),
                    UniTask.WaitUntil(() => _isDestroyed));
            }
        }

        private async UniTask ProductionRoutine()
        {
            while (!_isDestroyed)
            {
                FindProductionPlan();

                await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(_teamUpdateFrequency)),
                    UniTask.WaitUntil(() => _isDestroyed));
            }
        }

        private async void Start()
        {
            await UniTask.Yield();

            ObservationRoutine().Forget();
            ProductionRoutine().Forget();
        }

        private void OnDestroy()
        {
            _isDestroyed = true;
        }
    }
}