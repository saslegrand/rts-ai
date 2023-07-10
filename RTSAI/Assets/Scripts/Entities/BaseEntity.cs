using System;
using UnityEngine;

namespace RTS
{
    using FogOfWar;
    using UI;

    public abstract class BaseEntity : MonoBehaviour, ISelectable, IDamageable, IRepairable
    {
        [SerializeField] protected ETeam _team;

        protected EntityVisibility _visibility;
        public EntityVisibility Visibility
        {
            get
            {
                _visibility ??= GetComponent<EntityVisibility>();

                return _visibility;
            }
        }
        
        protected GameObject _selectedSprite;
        protected SpriteRenderer _stateSprite;
        protected Lifebar _lifeBar;
        protected bool _isInitialized;
        protected UnityEngine.UI.Image _minimapImage;

        public Action OnHpUpdated;
        public Action OnDamage;
        public Action OnDeadEvent;

        private int _currentHp;
        private int _lostHp;
        
        public int Hp
        {
            get => _currentHp;

            protected set
            {
                _currentHp = Math.Clamp(value, 0, Hp_Max);
                OnHpUpdated?.Invoke();
            }
            
        }
        public abstract int Hp_Max { get; }

        protected int LostHp
        {
            get => _lostHp;

            private set => _lostHp = Math.Clamp(value, 0, Hp_Max);
        }

        public bool IsSelected { get; protected set; }
        public bool IsAlive { get; protected set; }
        public float PhysicalRadius { get; protected set; }

        private const float IS_ATTACKED_RESET_TIME = 1f;
        private float _isAttackedTimer;

        private bool _isAttacked = false;

        public bool IsAttacked
        {
            get => _isAttacked;

            private set
            {
                _isAttacked = value;

                if (_isAttacked)
                    _isAttackedTimer = 0;
            }
        }
        
        public virtual void Init(ETeam team)
        {
            if (_isInitialized)
                return;

            _team = team;

            if (Visibility) { Visibility.Team = team; }

            Transform minimapTransform = transform.Find("MinimapCanvas");
            if (minimapTransform != null)
            {
                _minimapImage = minimapTransform.GetComponentInChildren<UnityEngine.UI.Image>();
                _minimapImage.color = GameServices.GetTeamColor(_team);
            }

            if (TryGetComponent(out Collider col))
            {
                CapsuleCollider caps = col as CapsuleCollider;
                SphereCollider sphere = col as SphereCollider;
                BoxCollider box = col as BoxCollider;
                if (caps != null)
                    PhysicalRadius = caps.radius;
                else if (sphere != null)
                    PhysicalRadius = sphere.radius;
                else if (box != null)
                {
                    Vector3 boxSize = box.size;
                    PhysicalRadius = Mathf.Max(boxSize.x, boxSize.z) * 0.5f;
                }
            }

            _isInitialized = true;
        }

        public Vector3 GetPhysicalPosition(Vector3 other)
        {
            Vector3 pos = transform.position;
            Vector3 dir = (other - pos).normalized;
            return pos + dir * (PhysicalRadius * 0.9f);
        }

        public Color GetColor()
        {
            return GameServices.GetTeamColor(GetTeam());
        }

        private void UpdateHpUI()
        {
            if (_lifeBar)
                _lifeBar.SetPercent(Hp, Hp_Max);                
        }

        #region ISelectable
        public virtual void SetSelected(bool selected)
        {
            if (IsAlive == false)
                return;

            IsSelected = selected;
            
            if (_selectedSprite)
                _selectedSprite.SetActive(IsSelected);
        }

        public void SetStateColor(Color stateColor)
        {
            if (_stateSprite) _stateSprite.color = stateColor;
        }

        public ETeam GetTeam()
        {
            return _team;
        }
        #endregion

        #region IDamageable
        public void AddDamage(int damageAmount)
        {
            if (IsAlive == false)
                return;

            Hp -= damageAmount;
            LostHp += damageAmount;
            OnDamage?.Invoke();

            if (Hp > 0) return;

            IsAlive = false;
            OnDeadEvent?.Invoke();
        }
        
        public void Destroy()
        {
            AddDamage(Hp);
        }
        #endregion

        #region IRepairable
        public virtual bool NeedsRepairing()
        {
            return true;
        }
        public virtual void Repair(int amount)
        {
            OnHpUpdated?.Invoke();
        }
        public virtual void FullRepair()
        {
        }
        #endregion

        #region MonoBehaviour methods
        protected virtual void Awake()
        {
            IsAlive = true;

            _selectedSprite = transform.Find("SelectedSprite")?.gameObject;
            if (_selectedSprite)
                _selectedSprite.SetActive(false);
            
            _stateSprite = transform.Find("StateSprite")?.GetComponent<SpriteRenderer>();

            Transform hpTransform = transform.Find("Canvas_Lifebar");
            if (hpTransform)
            {
                _lifeBar = hpTransform.GetComponent<Lifebar>();
            }

            OnHpUpdated += UpdateHpUI;
            OnDamage += () => IsAttacked = true;
        }
        protected virtual void Start()
        {
            Init(GetTeam());
            UpdateHpUI();
        }
        
        protected virtual void Update()
        {
            _isAttackedTimer += Time.deltaTime;
            if (_isAttackedTimer > IS_ATTACKED_RESET_TIME)
                IsAttacked = false;
        }
        #endregion
    }
}
