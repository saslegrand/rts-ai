using System.Collections.Generic;
using System.Linq;
using RTS.AI.Tools;
using UnityEngine;

namespace RTS.AI
{
    using Planner;

    [System.Serializable]
    public class SquadThreshold
    {
        public float Threshold = 0.1f;
        public int MaxSquadNumber = 1;
        public Vector2Int MinMaxUnitNumberInSquad = Vector2Int.one;
    }

    [System.Serializable]
    public class GoalThreshold
    {
        public AnimationCurve _armyPercentByGoalPercent;

        public SquadThreshold[] Thresholds;

        public SquadThreshold GetThreshold(float percent)
        {
            foreach (SquadThreshold squadThreshold in Thresholds)
            {
                if (percent > squadThreshold.Threshold)
                    continue;

                return squadThreshold;
            }

            return Thresholds[^1];
        }
    }

    public class ArmyLeader : MonoBehaviour
    {
        [SerializeField] private float _focusOnlyOnMainGoalThreshold = 0.85f;
        [SerializeField] private GoalThreshold _captureThresholds;
        [SerializeField] private GoalThreshold _defendThresholds;
        [SerializeField] private GoalThreshold _attackThresholds;

        [SerializeField] private AIController _controller;

        [SerializeField] private SquadLeaderAgent _squadLeaderPrefab;

        private List<SquadLeaderAgent> _squadLeaders;

        private List<Unit> _availableUnits;
        private List<Unit> _busyUnits;

        private List<Factory> _defendTargets;
        private List<Factory> _attackTargets;
        private List<TargetBuilding> _captureTargets;

        private UtilityAction[] _orderedGoals;
        public UtilityAction[] OrderedGoals => _orderedGoals;

        private ArmyLeaderGoal _mainGoal = ArmyLeaderGoal.None;

        private void Awake()
        {
            _squadLeaders = new List<SquadLeaderAgent>();
        }

        public void InterpretGoals(UtilityAction[] orderedGoals, List<Factory> defendTargets, List<Factory> attackTargets, List<TargetBuilding> captureTargets)
        {
            _orderedGoals = orderedGoals;

            if (orderedGoals.Length == 0)
                return;

            _mainGoal = orderedGoals[0].Goal;

            _defendTargets = new List<Factory>(defendTargets);
            _attackTargets = new List<Factory>(attackTargets);
            _captureTargets = new List<TargetBuilding>(captureTargets);

            
            UpdateUnitsCount();
            
            foreach (UtilityAction order in orderedGoals)
            {
                ArmyLeaderGoal goal = order.Goal;
                float percent = order.Percent;

                if (percent > _focusOnlyOnMainGoalThreshold)
                {
                    DispatchSquadsForOrder(goal, 1.0f);
                    return;
                }

                DispatchSquadsForOrder(goal, Mathf.Clamp01(percent * 1.5f));
            }

            SquadController otherUnits = new (_availableUnits);
            otherUnits.MoveSquadToTarget(otherUnits.GetSquadCenter() + new Vector3(Random.Range(-20.0f, 20.0f), 0, Random.Range(-20.0f, 20.0f)), true);
            otherUnits.RemoveUnits(otherUnits.GetAllUnits());
        }

        private void UpdateUnitsCount()
        {
            // Dismantle all unused squads
            for (int i = 0; i < _squadLeaders.Count; i++)
            {
                if (_squadLeaders[i] == null)
                {
                    _squadLeaders.RemoveAt(i);
                    UpdateUnitsCount();
                    return;
                }

                if (_squadLeaders[i].Goal == ArmyLeaderGoal.None ||
                   (_defendTargets.Count > 0 && _squadLeaders[i].Goal != ArmyLeaderGoal.Defend && _mainGoal == ArmyLeaderGoal.Defend))
                {
                    _squadLeaders[i].DismantleSquad();
                }
            }

            // Then count up available & busy units
            List<Unit> allUnits = _controller.UnitList;
            _availableUnits = new List<Unit>();
            _busyUnits = new List<Unit>();

            foreach (Unit unit in allUnits)
            {
                if (unit.SquadController == null)
                    _availableUnits.Add(unit);
                else
                    _busyUnits.Add(unit);
            }
        }


        private void DispatchSquadsForOrder(ArmyLeaderGoal goal, float percent)
        {
            SquadThreshold threshold;

            switch (goal)
            {
                case ArmyLeaderGoal.CaptureTarget:
                    threshold = _captureThresholds.GetThreshold(percent);
                    //percent = _captureThresholds._armyPercentByGoalPercent.Evaluate(percent);
                    break;
                case ArmyLeaderGoal.Defend:
                    threshold = _defendThresholds.GetThreshold(percent);
                    //percent = _defendThresholds._armyPercentByGoalPercent.Evaluate(percent);
                    break;
                case ArmyLeaderGoal.Attack:
                    threshold = _attackThresholds.GetThreshold(percent);
                    //percent = _defendThresholds._armyPercentByGoalPercent.Evaluate(percent);
                    break;
                default:
                    return;
            }

            int nbOnGoal = IsGoalBeingAchieved(goal);
            int nb = threshold.MaxSquadNumber;
            Vector2Int minMax = threshold.MinMaxUnitNumberInSquad;
            nb -= nbOnGoal;

            if (nb <= 0)
            {
                FillSquadForGoal(goal, percent, minMax);
                return;
            }

            float dispatchPercent = percent / nb;

            switch (goal)
            {
                case ArmyLeaderGoal.CaptureTarget:
                    for (int i = 0; i < nb; i++)
                        DispatchSquadsForCapture(dispatchPercent, minMax);
                    break;
                case ArmyLeaderGoal.Defend:
                    for (int i = 0; i < nb; i++)
                        DispatchSquadsForDefend(dispatchPercent, minMax);
                    break;
                case ArmyLeaderGoal.Attack:
                    for (int i = 0; i < nb; i++)
                        DispatchSquadsForAttack(dispatchPercent, minMax);
                    break;
            }
        }

        private void DispatchSquadsForCapture(float percent, Vector2Int minMaxUnit)
        {
            SquadLeaderAgent leader = TryCreateNewSquad(percent, minMaxUnit);

            if (leader == null)
                return;

            Vector3 center = (_controller.GetTeamRoot().position + leader.Squad.GetSquadCenter()) * 0.5f;
            float minDist = float.MaxValue;
            TargetBuilding nearestTarget = null;

            List<TargetBuilding> availableCaptureTargets = ( from target in _captureTargets let found = _squadLeaders.Any(agent => agent.OrderTargetResource == target) where !found select target ).ToList();

            TargetBuilding targetNeutral = null;
            TargetBuilding targetOpponent = null;

            foreach (TargetBuilding target in availableCaptureTargets)
            {
                float dist = (target.transform.position - center).sqrMagnitude;
                if (dist <= minDist)
                {
                    minDist = dist;
                    nearestTarget = target;
                    if (target.GetTeam() == ETeam.Neutral)
                        targetNeutral = target;
                    else
                        targetOpponent = target;
                }
            }

            if (nearestTarget == null)
            {
                leader.DismantleSquad();
                return;
            }

            if (targetNeutral != null && targetOpponent != null)
                nearestTarget = targetNeutral;

            leader.SetSquadGoal(ArmyLeaderGoal.CaptureTarget);
            leader.SetOrderTargetResource(nearestTarget);
            leader.StartGoal();
            _captureTargets.Remove(nearestTarget);
        }

        private void DispatchSquadsForDefend(float percent, Vector2Int minMaxUnit)
        {
            SquadLeaderAgent leader = TryCreateNewSquad(percent, minMaxUnit);

            if (leader == null)
                return;

            Vector3 center = leader.Squad.GetSquadCenter();
            float minDist = float.MaxValue;
            Factory nearestTarget = null;

            foreach (Factory target in _defendTargets)
            {
                float dist = (target.transform.position - center).sqrMagnitude;

                if (dist > minDist) continue;

                minDist = dist;
                nearestTarget = target;
            }

            if (nearestTarget == null)
            {
                leader.DismantleSquad();
                return;
            }

            leader.SetSquadGoal(ArmyLeaderGoal.Defend);
            leader.SetOrderTargetEntity(nearestTarget);
            leader.StartGoal();
        }

        private void DispatchSquadsForAttack(float percent, Vector2Int minMaxUnit)
        {
            SquadLeaderAgent leader = TryCreateNewSquad(percent, minMaxUnit);

            if (leader == null)
                return;

            Vector3 center = leader.Squad.GetSquadCenter();
            float minDist = float.MaxValue;
            Factory nearestTarget = null;

            foreach (Factory target in _attackTargets)
            {
                float dist = (target.transform.position - center).sqrMagnitude;

                if (dist > minDist) continue;

                minDist = dist;
                nearestTarget = target;
            }

            if (nearestTarget == null)
            {
                leader.DismantleSquad();
                return;
            }

            leader.SetSquadGoal(ArmyLeaderGoal.Attack);
            leader.SetOrderTargetEntity(nearestTarget);
            leader.StartGoal();
        }

        private int IsGoalBeingAchieved(ArmyLeaderGoal goal)
        {
            return _squadLeaders.Count(agent => agent.Goal == goal);
        }


        private void FillSquadForGoal(ArmyLeaderGoal goal, float percent, Vector2Int minMaxUnit)
        {
            foreach (SquadLeaderAgent agent in _squadLeaders)
            {
                if (agent.Goal != goal)
                    continue;

                int count = Mathf.RoundToInt(_availableUnits.Count * percent);
                if (count < 1) count = _availableUnits.Count;
                int max = minMaxUnit.y - agent.Squad.UnitCount;
                if (count > max && max >= 1)
                    count = max;

                int i = 0;
                while (i < count && _availableUnits.Count > 0)
                {
                    agent.AddUnit(_availableUnits[0]);
                    _availableUnits.RemoveAt(0);
                }
            }
        }

        private SquadLeaderAgent TryCreateNewSquad(float percent, Vector2Int minMaxUnit)
        {
            if (_availableUnits.Count < minMaxUnit.x)
                return null;

            // Take the percentage of available units
            int count = Mathf.RoundToInt(_availableUnits.Count * percent);
            if (count < minMaxUnit.x) count = minMaxUnit.x;

            List<Unit> squadUnits = new ();

            int i = 0;
            for (; i < count; i++)
            {
                squadUnits.Add(_availableUnits[0]);
                _availableUnits.RemoveAt(0);
            }

            while (i < minMaxUnit.y && _availableUnits.Count > 0)
            {
                squadUnits.Add(_availableUnits[0]);
                _availableUnits.RemoveAt(0);
            }

            return CreateNewSquad(squadUnits);
        }

        private SquadLeaderAgent CreateNewSquad(List<Unit> units)
        {
            SquadLeaderAgent squadLeader = Instantiate(_squadLeaderPrefab, transform);
            _squadLeaders.Add(squadLeader);

            squadLeader.Initialize(units, _controller.GetTeam());
            squadLeader.OnSquadEmpty.AddListener(() =>
            {
                _squadLeaders.Remove(squadLeader);
            });

            return squadLeader;
        }
    }
}