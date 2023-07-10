using System;
using UnityEngine;
using UnityEngine.AI;

namespace RTS
{
    public class Unit : BaseEntity
    {
        [SerializeField] private UnitDataScriptable _unitData;

        [SerializeField] private Animator _animator;

        [SerializeField] private ParticleSystem _healParticleSystem;

        private Transform _bulletSlot;
        private float _lastActionDate;
        private BaseEntity _entityTarget;
        private BaseEntity _entityNearestTarget;
        private TargetBuilding _captureTargetOrder;
        private TargetBuilding _captureTarget;
        private NavMeshAgent _navMeshAgent;

        private Vector3 _orderPosition = Vector3.zero;
        private Vector3 _restPosition = Vector3.zero;
        private bool _attackMode = false;

        private SquadController _squadController;
        public SquadController SquadController
        {
            get => _squadController;
            set => _squadController = value;
        }

        public UnitDataScriptable GetUnitData => _unitData;
        public int Cost => _unitData.Cost;
        public int GetTypeId => _unitData.TypeId;
        public BaseEntity EntityTarget => _entityTarget;
        public BaseEntity EntityNearestTarget => _entityNearestTarget;
        public TargetBuilding CaptureTarget => _captureTarget;
        public TargetBuilding CaptureTargetOrder => _captureTargetOrder;
        public Vector3 OrderPosition => _orderPosition;
        public Vector3 RestPosition => _restPosition;
        public bool AttackMode => _attackMode;
        public bool HasReachOrder => (_orderPosition - _restPosition).sqrMagnitude <= 1;

        public override int Hp_Max => _unitData.MaxHP;

        public Action<Unit> OnUnitDeadEvent;

        public override void Init(ETeam team)
        {
            if (_isInitialized)
                return;

            base.Init(team);

            OnDeadEvent += () => OnUnitDeadEvent?.Invoke(this);

            Hp = _unitData.MaxHP;
            OnDeadEvent += Unit_OnDead;
        }
        private void Unit_OnDead()
        {
            if (IsCapturing())
                StopCapture();

            if (GetUnitData.DeathFXPrefab)
                Instantiate(GetUnitData.DeathFXPrefab, transform.position, Quaternion.identity);

            if (_navMeshAgent)
            {
                _navMeshAgent.isStopped = true;
                _navMeshAgent.enabled = false;
            }

            if (SquadController != null)
            {
                SquadController.RemoveUnit(this);
                SquadController = null;
            }

            if (_animator)
                _animator.SetTrigger(UnityEngine.Random.Range(0, 2) == 0 ? "Death_1" : "Death_2");

            SetSelected(false);
            Destroy(gameObject, 2.0f);
        }

        #region MonoBehaviour methods
        protected override void Awake()
        {
            base.Awake();

            _orderPosition = transform.position;
            _restPosition = transform.position;

            _navMeshAgent = GetComponent<NavMeshAgent>();
            _bulletSlot = transform.Find("BulletSlot");

            // fill NavMeshAgent parameters
            _navMeshAgent.speed = GetUnitData.Speed;
            _navMeshAgent.angularSpeed = GetUnitData.AngularSpeed;
            _navMeshAgent.acceleration = GetUnitData.Acceleration;
        }
        protected override void Start()
        {
            // Needed for non factory spawned units (debug)
            if (!_isInitialized)
                Init(_team);

            base.Start();
        }
        protected override void Update()
        {
            base.Update();
            
            if (_animator)
            {
                _animator.SetFloat("MoveSpeed", Mathf.Clamp01(_navMeshAgent.velocity.magnitude / _navMeshAgent.speed));
            }
        }
        private void FixedUpdate()
        {
            /*
            if (_navMeshAgent.enabled &&
                !_navMeshAgent.pathPending &&
                !(_navMeshAgent.remainingDistance < _navMeshAgent.stoppingDistance) &&
                (!_navMeshAgent.hasPath || _navMeshAgent.velocity.sqrMagnitude < 0.01f))
            {
                _navMeshAgent.isStopped = true;
                _navMeshAgent.enabled = false;
                _navMeshObstacle.enabled = true;
            }*/

            //_navMeshAgent.avoidancePriority = (100 - Mathf.Clamp(Mathf.FloorToInt((_navMeshAgent.velocity.magnitude / _navMeshAgent.speed) * 100.0f), 0, 100)) / 2;

            if ((transform.position - _orderPosition).sqrMagnitude < 1)
            {
                _restPosition = _orderPosition;
                _attackMode = false;
            }
        }
        #endregion

        #region IRepairable
        public override bool NeedsRepairing()
        {
            return Hp < Hp_Max;
        }
        public override void Repair(int amount)
        {
            if (!IsAlive)
                return;

            Hp = Mathf.Min(Hp + amount, GetUnitData.MaxHP);
            base.Repair(amount);

            _healParticleSystem.Stop();
            _healParticleSystem.Play();
        }
        public override void FullRepair()
        {
            Repair(GetUnitData.MaxHP);
        }
        #endregion

        #region Tasks methods : Moving, Capturing, Targeting, Attacking, Repairing ...

        // Moving Task
        public void SetTargetPos(Vector3 pos)
        {
            if (!IsAlive)
                return;

            _orderPosition = pos;
            _entityTarget = null;

            if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
                _orderPosition = hit.position;

            MoveTo(_orderPosition);
        }

        public void MoveTo(Vector3 pos)
        {
            if (!IsAlive)
                return;

            if ((transform.position - pos).sqrMagnitude < 1)
                return;

            if (IsCapturing())
                StopCapture();

            if (_navMeshAgent)
            {
                _navMeshAgent.SetDestination(pos);
                _navMeshAgent.isStopped = false;
            }
        }

        public void MoveToEntityTarget()
        {
            if (!IsAlive)
                return;

            if (EntityTarget == null)
                return;

            MoveToEntityTarget(EntityTarget);
        }

        public void MoveToEntityTarget(BaseEntity target)
        {
            MoveTo(target.GetPhysicalPosition(transform.position));
        }

        public void MoveToTargetBuilding(TargetBuilding target)
        {
            MoveTo(target.GetPhysicalPosition(transform.position));
        }

        public void SetAttackNearestTarget(BaseEntity target)
        {
            _entityNearestTarget = target;
        }

        public bool AttackNearestTarget()
        {
            if (_entityNearestTarget == null)
                return false;

            SetAttackTarget(_entityNearestTarget);
            return true;
        }

        // Targeting Task - attack
        public void SetAttackTarget(BaseEntity target)
        {
            if (target == null)
                return;

            if (IsCapturing())
                StopCapture();

            if (target.GetTeam() != GetTeam() && target.IsAlive)
                _entityTarget = target;
        }

        // Targeting Task - capture
        public void SetCaptureTarget(TargetBuilding target)
        {
            if (target == null)
                return;

            if (target == _captureTarget || target == _captureTargetOrder)
                return;

            if (IsCapturing())
                StopCapture();

            if (target.GetTeam() == GetTeam())
                return;

            _entityTarget = null;
            _captureTargetOrder = target;
        }

        // Targeting Task - repairing
        public void SetRepairTarget(BaseEntity entity)
        {
            if (entity == null)
                return;

            if (IsCapturing())
                StopCapture();

            if (entity.GetTeam() == GetTeam())
                StartRepairing(entity);
        }
        public bool CanAttack(BaseEntity target)
        {
            if (!IsAlive)
                return false;

            if (target == null)
                return false;

            // distance check
            Vector3 localPos = transform.position;
            Vector3 targetPos = target.GetPhysicalPosition(localPos);
            float sqrDist = (targetPos - localPos).sqrMagnitude;
            float sqrAttack = GetUnitData.AttackDistanceMax * GetUnitData.AttackDistanceMax;

            if (sqrDist > sqrAttack)
                return false;

            return true;
        }

        public void SetAttackMode(bool value)
        {
            _attackMode = value;
        }


        public void ComputeAttack()
        {
            if (!IsAlive)
                return;

            if (!CanAttack(_entityTarget))
                return;

            if (!_entityTarget.IsAlive)
            {
                _entityTarget = null;
                return;
            }

            if (_navMeshAgent)
                _navMeshAgent.isStopped = true;

            transform.LookAt(_entityTarget.transform);
            // only keep Y axis
            Vector3 eulerRotation = transform.eulerAngles;
            eulerRotation.x = 0f;
            eulerRotation.z = 0f;
            transform.eulerAngles = eulerRotation;

            if ((Time.time - _lastActionDate) > _unitData.AttackFrequency)
            {
                _lastActionDate = Time.time;
                int damages = Mathf.FloorToInt(_unitData.DPS * _unitData.AttackFrequency);

                // visual only ?
                if (_unitData.BulletPrefab)
                {
                    GameObject newBullet = Instantiate(_unitData.BulletPrefab, _bulletSlot);
                    newBullet.transform.parent = null;
                    newBullet.GetComponent<Bullet>().ShootToward(_entityTarget, this, damages);
                }
                else
                {
                    // apply damages
                    _entityTarget.AddDamage(damages);
                }


                if (_animator)
                {
                    _animator.SetTrigger("Attack_1");
                }
            }
        }

        public bool SeeCapture(TargetBuilding target)
        {
            if (target == null)
                return false;

            // distance check
            if ((target.transform.position - transform.position).sqrMagnitude > GetUnitData.CaptureDistanceRange * GetUnitData.CaptureDistanceRange)
                return false;

            return true;
        }

        public bool CanCapture(TargetBuilding target)
        {
            if (target == null)
                return false;

            // distance check
            if ((target.transform.position - transform.position).sqrMagnitude > GetUnitData.CaptureDistanceMax * GetUnitData.CaptureDistanceMax)
                return false;

            return true;
        }

        // Capture Task
        public void StartCapture(TargetBuilding target)
        {
            if (!CanCapture(target))
                return;

            _captureTarget = target;
        }
        public void StopCapture()
        {
            if (_captureTarget == null)
                return;

            _captureTarget = null;
            _captureTargetOrder = null;
        }

        public bool IsCapturing()
        {
            return _captureTarget != null;
        }

        // Repairing Task
        public bool CanRepair(BaseEntity target)
        {
            if (GetUnitData.CanRepair == false || !target)
                return false;

            // distance check
            if ((target.transform.position - transform.position).sqrMagnitude > GetUnitData.RepairDistanceMax * GetUnitData.RepairDistanceMax)
                return false;

            return true;
        }
        public void StartRepairing(BaseEntity entity)
        {
            if (GetUnitData.CanRepair)
            {
                _entityTarget = entity;
            }
        }

        public void ComputeRepairing()
        {
            if (!IsAlive)
                return;

            if (CanRepair(_entityTarget) == false)
                return;

            if (_navMeshAgent)
                _navMeshAgent.isStopped = true;

            transform.LookAt(_entityTarget.transform);
            // only keep Y axis
            Vector3 eulerRotation = transform.eulerAngles;
            eulerRotation.x = 0f;
            eulerRotation.z = 0f;
            transform.eulerAngles = eulerRotation;

            if ((Time.time - _lastActionDate) > _unitData.RepairFrequency)
            {
                _lastActionDate = Time.time;

                _animator.SetTrigger("Attack_1");

                // Apply repairing
                int amount = Mathf.FloorToInt(_unitData.RPS * _unitData.RepairFrequency);

                _entityTarget.Repair(amount);

                if (!_entityTarget.NeedsRepairing())
                    _entityTarget = null;
            }
        }
        #endregion


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            /*
            if (GetUnitData != null)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(transform.position, GetUnitData.AttackDistanceMax);
            }*/

            if (_navMeshAgent)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(_orderPosition, 0.5f);
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(_restPosition, 0.4f);
            }
        }
#endif

    }

}
