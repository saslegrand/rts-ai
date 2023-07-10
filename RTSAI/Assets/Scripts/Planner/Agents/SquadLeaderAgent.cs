using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;

namespace RTS.AI.Planner
{
    using Waypoint;

    public class SquadLeaderAgent : MonoBehaviour
    {
        public WorldState CurWorldState = new WorldState();

        protected SquadController _squad;
        public SquadController Squad => _squad;

        protected Waypoint[] _path;

        protected ETeam _team;

        protected ArmyLeaderGoal _goal;
        public ArmyLeaderGoal Goal => _goal;

        protected Vector3 _orderTarget;
        public Vector3 OrderTarget => _orderTarget;

        protected BaseEntity _orderTargetEntity;
        public BaseEntity OrderTargetEntity => _orderTargetEntity;

        protected TargetBuilding _orderTargetResource;
        public TargetBuilding OrderTargetResource => _orderTargetResource;

        public UnityEvent OnSquadEmpty = new UnityEvent();
        public UnityEvent<ArmyLeaderGoal> OnSquadGoalAccomplished = new UnityEvent<ArmyLeaderGoal>();

        private int _pathLength = 0;
        private int _currentPathNode;

        private const float _radiusToWaypoint = 10.0f;

        public void StartGoal()
        {
            /*
            Regroup();

            await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(2.0f)),
                UniTask.WaitUntil(() => _squad.HasReachOrderTarget() == true));
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            */

            if (_orderTargetEntity)
            {
                FindPathToTarget(_orderTargetEntity.transform.position);
                _orderTargetEntity.OnDeadEvent += () => _goal = ArmyLeaderGoal.None;
            }
            else if (_orderTargetResource)
            {
                FindPathToTarget(_orderTargetResource.transform.position);
                _orderTargetResource.OnCaptured += () => _goal = ArmyLeaderGoal.None;
            }

            _currentPathNode = 0;
            _pathLength = _path.Length - 1;
            MoveTo(_path[_currentPathNode].transform.position, _goal != ArmyLeaderGoal.Defend);
        }

        private void Update()
        {
            if (_path == null)
            {
                DismantleSquad();
                return;
            }

            if (_currentPathNode < _pathLength)
            {
                if (!HasReachOrderTarget()) return;

                _currentPathNode++;

                MoveTo(_path[_currentPathNode].transform.position,
                       _goal != ArmyLeaderGoal.Defend);
                return;
            }

            bool isLastNode = _currentPathNode == _pathLength;
            if (isLastNode && !HasReachOrderTarget()) return;

            switch (_goal)
            {
                case ArmyLeaderGoal.CaptureTarget:
                    if (isLastNode)
                    {
                        _squad.MoveSquadToTarget(_orderTargetResource.transform.position, true, false);
                        _squad.SetCaptureTarget(_orderTargetResource);
                    }
                    break;

                case ArmyLeaderGoal.Defend:
                    if (!isLastNode && HasReachOrderTarget())
                    {
                        _goal = ArmyLeaderGoal.None;
                        return;
                    }
                    _squad.MoveSquadToTarget(_orderTargetEntity.transform.position, true);

                    break;

                case ArmyLeaderGoal.Attack:
                    if (!isLastNode && HasReachOrderTarget())
                    {
                        _goal = ArmyLeaderGoal.None;
                        return;
                    }

                    _squad.MoveSquadToTarget(_orderTargetEntity.transform.position, true);
                    break;
            }

            _currentPathNode++;
        }

        public void Initialize(List<Unit> units, ETeam team)
        {
            _team = team;
            _squad = new SquadController(units);
            _squad.OnSquadEmpty.AddListener(() =>
            {
                DismantleSquad();
            });
        }

        public void AddUnit(Unit unit)
        {
            _squad.AddUnit(unit);
        }

        public void DismantleSquad()
        {
            foreach (Unit unit in _squad.GetAllUnits())
                unit.SquadController = null;

            OnSquadEmpty?.Invoke();
            Destroy(gameObject);
        }


        private void OnDrawGizmos()
        {
            if (_path == null)
                return;

            foreach (Waypoint waypoint in _path)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(waypoint.transform.position, 1f);
            }
        }

        public void SetSquadGoal(ArmyLeaderGoal goal)
        {
            _goal = goal;
        }

        public void SetOrderTarget(Vector3 target)
        {
            _orderTarget = target;
        }

        public void SetOrderTargetResource(TargetBuilding resource)
        {
            _orderTargetResource = resource;
        }

        public void SetOrderTargetEntity(BaseEntity target)
        {
            _orderTargetEntity = target;
        }

        public bool HasReachOrderTarget()
        {
            return _squad.HasReachOrderTarget();
        }

        public void Regroup()
        {
            Vector3 center = WaypointGraph.Instance.GetClosestWaypointToWorldPosition(_squad.GetSquadCenter()).transform.position;
            MoveTo(center, true, false);
        }

        public void MoveTo(Vector3 target, bool attackMode = false, bool captureMode = false)
        {
            _squad.MoveSquadToTarget(target, attackMode, !captureMode);
        }

        public void FindPathToTarget(Vector3 target)
        {
            Vector3 center = _squad.GetSquadCenter();
            _path = WaypointGraph.Instance.GetPath(center, target);
        }
    }
}