using System;
using System.Collections.Generic;
using System.Linq;
using RTS.Waypoint;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


namespace RTS
{
    public sealed class Factory : BaseEntity
    {
        [SerializeField]
        private FactoryDataScriptable _factoryData;
        public FactoryDataScriptable FactoryData => _factoryData;
        
        private GameObject[] _unitPrefabs;
        private GameObject[] _factoryPrefabs;
        private int _requestedEntityBuildIndex = -1;
        private Image _buildGaugeImage;
        private int _spawnCount;
        /* !! max available unit count in menu is set to 9, available factories count to 3 !! */
        private const int MAX_AVAILABLE_UNITS = 9;
        private const int MAX_AVAILABLE_FACTORIES = 3;

        private const float BANNER_RADIUS = 10.0f;
        private GameObject _spawnBanner;
        private GameObject _localSpawnBanner;
        private bool _isBannerOnFactory;
        private TeamController _controller;

        [SerializeField] private int _maxBuildingQueueSize = 8;

        public Queue<int> UnitQueue { get; } = new ();
        public int CurrentUnitInBuild => _requestedEntityBuildIndex;

        private float _currentBuildDuration;
        public float CurrentBuildPercent => _buildCurrentTime / _currentBuildDuration;

        public override int Hp_Max => _factoryData.MaxHP;

        private enum State
        {
            Available = 0,
            UnderConstruction,
            BuildingUnit,
        }

        private State _currentState;
        public bool IsUnderConstruction => _currentState == State.UnderConstruction;
        public int Cost => _factoryData.Cost;
        private FactoryDataScriptable _factoryDataScriptable => _factoryData;
        public int AvailableUnitsCount => Mathf.Min(MAX_AVAILABLE_UNITS, _factoryData.AvailableUnits.Length);
        public int AvailableFactoriesCount => Mathf.Min(MAX_AVAILABLE_FACTORIES, _factoryData.AvailableFactories.Length);
        public Action<Unit> OnUnitBuilt;
        public Action<Factory> OnFactoryBuilt;
        public Action OnBuildCanceled;
        private float _buildCurrentTime;
        public bool IsBuildingUnit => _currentState == State.BuildingUnit;

        #region MonoBehaviour methods
        protected override void Awake()
        {
            base.Awake();

            Hp = _factoryData.MaxHP;
            
            if (transform.Find("SpawnBanner") is { } bannerT)
                _spawnBanner = bannerT.gameObject;
            
            if (transform.Find("LocalSpawnBanner") is { } localBannerT)
                _localSpawnBanner = localBannerT.gameObject;

            _buildGaugeImage = transform.Find("Canvas/BuildProgressImage").GetComponent<Image>();
            if (_buildGaugeImage)
            {
                _buildGaugeImage.fillAmount = 0f;
                _buildGaugeImage.color = GameServices.GetTeamColor(GetTeam());
            }

            if (_factoryData == null)
            {
                Debug.LogWarning("Missing FactoryData in " + gameObject.name);
            }
            OnDeadEvent += Factory_OnDead;

            _unitPrefabs = new GameObject[_factoryData.AvailableUnits.Length];
            _factoryPrefabs = new GameObject[_factoryData.AvailableFactories.Length];

            // Load from resources actual Unit prefabs from template data
            for (int i = 0; i < _factoryData.AvailableUnits.Length; i++)
            {
                GameObject templateUnitPrefab = _factoryData.AvailableUnits[i];
                string path = "Prefabs/Units/" + templateUnitPrefab.name + "_" + _team;
                _unitPrefabs[i] = Resources.Load<GameObject>(path);
                if (_unitPrefabs[i] == null)
                    Debug.LogWarning("could not find Unit Prefab at " + path);
            }

            // Load from resources actual Factory prefabs from template data
            for (int i = 0; i < _factoryData.AvailableFactories.Length; i++)
            {
                GameObject templateFactoryPrefab = _factoryData.AvailableFactories[i];
                string path = "Prefabs/Factories/" + templateFactoryPrefab.name + "_" + _team;
                _factoryPrefabs[i] = Resources.Load<GameObject>(path);
            }

        }
        protected override void Start()
        {
            base.Start();

            GameServices.GetGameState().IncreaseTeamScore(_team);
            _controller = GameServices.GetControllerByTeam(_team);
        }
        protected override void Update()
        {
            base.Update();
            
            _buildCurrentTime += Time.deltaTime;
            float buildPercent = CurrentBuildPercent;

            switch (_currentState)
            {
                case State.Available:
                    break;

                case State.UnderConstruction:
                    if (buildPercent <= 1f)
                    {
                        if (!_buildGaugeImage) return;

                        _buildGaugeImage.fillAmount = buildPercent;
                        Hp = (int)(buildPercent * Hp_Max) - LostHp;
                        
                        return;
                    }

                    _currentState = State.Available;
                    _buildGaugeImage.fillAmount = 0f;
                    Hp = Hp_Max - LostHp;

                    Vector3 bannerPosition = WaypointGraph.Instance.GetClosestWaypointToWorldPosition(transform.position).transform.position;
                    if (Vector3.Distance(transform.position, bannerPosition) < 10f)
                        UpdateBanner(true, transform.position);
                    else
                        UpdateBanner(false, bannerPosition);
                    
                    return;

                case State.BuildingUnit:
                    if (buildPercent <= 1f)
                    {
                        if (!_buildGaugeImage) return;

                        float currentBuildDuration = CurrentBuildPercent;
                        _buildGaugeImage.fillAmount = currentBuildDuration;
                        return;
                    }

                    OnUnitBuilt?.Invoke(BuildUnit());
                    OnUnitBuilt = null; // remove registered methods
                    _currentState = State.Available;

                    // manage build queue : chain with new unit build if necessary
                    if (UnitQueue.Count == 0) return;

                    int unitIndex = UnitQueue.Dequeue();
                    StartBuildUnit(unitIndex);
                    
                    return;
            }
        }
        #endregion
        
        public override void SetSelected(bool selected)
        {
            base.SetSelected(selected);
            
            ShowLocalBanner(selected && _isBannerOnFactory);
            ShowBanner(selected && !_isBannerOnFactory);
        }

        public void UpdateBanner(bool isOnFactory, Vector3 newBannerPos)
        {
            if (isOnFactory)
            {
                _isBannerOnFactory = true;
                ShowLocalBanner(true);
                ShowBanner(false);
            }
            else
            {
                _isBannerOnFactory = false;
                ShowLocalBanner(false);
                ShowBanner(true);
                
                float yPos = _spawnBanner.transform.position.y;
                _spawnBanner.transform.position = new Vector3(newBannerPos.x, yPos, newBannerPos.z);
            }
        }
        
        private void ShowBanner(bool visibility)
        {
            if (_spawnBanner)
                _spawnBanner.SetActive(visibility);
        }
        
        private void ShowLocalBanner(bool visibility)
        {
            if (_localSpawnBanner)
                _localSpawnBanner.SetActive(visibility);
        }
        
        void Factory_OnDead()
        {
            if (_factoryData.DeathFXPrefab)
            {
                GameObject fx = Instantiate(_factoryData.DeathFXPrefab, transform);
                fx.transform.parent = null;
            }

            GameServices.GetGameState().DecreaseTeamScore(_team);
            Destroy(gameObject);
        }
        #region IRepairable
        public override bool NeedsRepairing()
        {
            return Hp < _factoryDataScriptable.MaxHP;
        }
        public override void Repair(int amount)
        {
            Hp = Mathf.Min(Hp + amount, _factoryDataScriptable.MaxHP);
            base.Repair(amount);
        }
        public override void FullRepair()
        {
            Repair(_factoryDataScriptable.MaxHP);
        }
        #endregion

        #region Unit building methods
        private bool IsUnitIndexValid(int unitIndex)
        {
            return unitIndex >= 0 && unitIndex < _unitPrefabs.Length;
        }
        
        public UnitDataScriptable GetBuildableUnitData(int unitIndex)
        {
            return IsUnitIndexValid(unitIndex) == false ? null : _unitPrefabs[unitIndex].GetComponent<Unit>().GetUnitData;
        }

        private int GetUnitCost(int unitIndex)
        {
            UnitDataScriptable data = GetBuildableUnitData(unitIndex);
            return data ? data.Cost : 0;
        }
        public int GetQueuedCount(int unitIndex)
        {
            return UnitQueue.Count(id => id == unitIndex);
        }
        public bool RequestUnitBuild(int unitMenuIndex)
        {
            int cost = GetUnitCost(unitMenuIndex);
            if (_controller.TotalBuildPoints < cost || UnitQueue.Count >= _maxBuildingQueueSize)
                return false;

            _controller.TotalBuildPoints -= cost;

            StartBuildUnit(unitMenuIndex);

            return true;
        }

        private void StartBuildUnit(int unitMenuIndex)
        {
            if (IsUnitIndexValid(unitMenuIndex) == false)
                return;

            // Factory is being constructed
            if (_currentState == State.UnderConstruction)
                return;

            // Build queue
            if (_currentState == State.BuildingUnit)
            {
                if (UnitQueue.Count < _maxBuildingQueueSize)
                    UnitQueue.Enqueue(unitMenuIndex);
                return;
            }

            _buildCurrentTime = 0f;
            _currentBuildDuration = GetBuildableUnitData(unitMenuIndex).BuildDuration;
            //Debug.Log("currentBuildDuration " + CurrentBuildDuration);

            _currentState = State.BuildingUnit;
            
            _requestedEntityBuildIndex = unitMenuIndex;

            OnUnitBuilt += unit =>
            {
                if (!unit) return;

                _controller.AddUnit(unit);
                _requestedEntityBuildIndex = -1;
            };

            OnUnitBuilt += _ =>
            {
                if (UnitQueue.Count <= 0)
                    _spawnCount = 0;
            };
        }

        // Finally spawn requested unit
        private Unit BuildUnit()
        {
            if (IsUnitIndexValid(_requestedEntityBuildIndex) == false)
                return null;

            _currentState = State.Available;

            GameObject unitPrefab = _unitPrefabs[_requestedEntityBuildIndex];

            if (_buildGaugeImage)
                _buildGaugeImage.fillAmount = 0f;

            int slotIndex = _spawnCount % _factoryData.NbSpawnSlots;
            // compute simple spawn position around the factory
            
            float angle = 2f * Mathf.PI * 0.75f;

            Vector3 spawnPos = transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            
            // !! Flying units require a specific layer to be spawned on !!
            bool isFlyingUnit = unitPrefab.GetComponent<Unit>().GetUnitData.IsFlying;
            int layer = isFlyingUnit ? LayerMask.NameToLayer("FlyingZone") : LayerMask.NameToLayer("Floor");

            // cast position on ground
            Ray ray = new (spawnPos, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit raycastInfo, 10f, 1 << layer))
                spawnPos = raycastInfo.point;

            Transform teamRoot = GameServices.GetControllerByTeam(GetTeam())?.GetTeamRoot();
            GameObject unitInst = Instantiate(unitPrefab, spawnPos, Quaternion.identity, teamRoot);
            unitInst.name = unitInst.name.Replace("(Clone)", $"_{_spawnCount}");
            Unit newUnit = unitInst.GetComponent<Unit>();
            newUnit.Init(GetTeam());

            Vector3 position;
            if (!_isBannerOnFactory)
            {
                Vector2 rng = Random.insideUnitCircle;
                position = _spawnBanner.transform.position + new Vector3(rng.x, 0, rng.y) * BANNER_RADIUS;
            }
            else
            {
                float angleTarget = 2f * Mathf.PI / _factoryData.NbSpawnSlots * slotIndex;
                int offsetIndex = Mathf.FloorToInt((float)_spawnCount / _factoryData.NbSpawnSlots);
                float radius = _factoryData.SpawnRadius + offsetIndex * _factoryData.RadiusOffset;
                position = transform.position + new Vector3(Mathf.Cos(angleTarget), 0f, Mathf.Sin(angleTarget)) * radius;
            }
            
            newUnit.SetAttackMode(true);
            newUnit.SetTargetPos(position);

            _spawnCount++;

            // disable build cancelling callback
            OnBuildCanceled = null;

            return newUnit;
        }
        public void CancelCurrentBuild()
        {
            if (_currentState is State.UnderConstruction or State.Available)
                return;

            _currentState = State.Available;

            // refund build points
            _controller.TotalBuildPoints += GetUnitCost(_requestedEntityBuildIndex);
            foreach (int unitIndex in UnitQueue)
            {
                _controller.TotalBuildPoints += GetUnitCost(unitIndex);
            }
            UnitQueue.Clear();

            _buildGaugeImage.fillAmount = 0f;
            _currentBuildDuration = 0f;
            _requestedEntityBuildIndex = -1;

            OnBuildCanceled?.Invoke();
            OnBuildCanceled = null;
        }
        #endregion

        #region Factory building methods
        public GameObject GetFactoryPrefab(int factoryIndex)
        {
            return IsFactoryIndexValid(factoryIndex) ? _factoryPrefabs[factoryIndex] : null;
        }
        bool IsFactoryIndexValid(int factoryIndex)
        {
            if (factoryIndex < 0 || factoryIndex >= _factoryPrefabs.Length)
            {
                Debug.LogWarning("Wrong factoryIndex " + factoryIndex);
                return false;
            }
            return true;
        }
        public FactoryDataScriptable GetBuildableFactoryData(int factoryIndex)
        {
            if (IsFactoryIndexValid(factoryIndex) == false)
                return null;

            return _factoryPrefabs[factoryIndex].GetComponent<Factory>()._factoryDataScriptable;
        }
        public int GetFactoryCost(int factoryIndex)
        {
            FactoryDataScriptable data = GetBuildableFactoryData(factoryIndex);
            if (data)
                return data.Cost;

            return 0;
        }
        public bool CanPositionFactory(int factoryIndex, Vector3 buildPos)
        {
            if (IsFactoryIndexValid(factoryIndex) == false)
                return false;

            if (GameServices.IsPosInPlayableBounds(buildPos) == false)
                return false;

            GameObject factoryPrefab = _factoryPrefabs[factoryIndex];

            Vector3 extent = factoryPrefab.GetComponent<BoxCollider>().size / 2f;

            float overlapYOffset = 0.1f;
            buildPos += Vector3.up * (extent.y + overlapYOffset);

            return !Physics.CheckBox(buildPos, extent);
        }

        public bool GetValidPosition(int factoryIndex, Vector3 center, ref Vector3 position)
        {
            if (IsFactoryIndexValid(factoryIndex) == false)
                return false;
            
            Debug.Log("Searching for position");

            Vector3 insideUnitCircle = Random.insideUnitCircle.normalized * Random.Range(10, 50.0f);
            Vector3 wantedPosition = center + new Vector3(insideUnitCircle.x, 0f, insideUnitCircle.y);

            GameObject factoryPrefab = _factoryPrefabs[factoryIndex];

            Vector3 extent = factoryPrefab.GetComponent<BoxCollider>().size;
            extent.y *= 0.5f;
            
            
            const float OverlapYOffset = 0.1f;
            Vector3 toTestPos = wantedPosition + Vector3.up * (extent.y + OverlapYOffset);

            int i = 0;
            while (i < 20 && (Physics.CheckBox(toTestPos, extent) || !GameServices.IsPosInPlayableBounds(toTestPos)))
            {
                insideUnitCircle = Random.insideUnitCircle.normalized * Random.Range(10, 50.0f);
                wantedPosition = center + new Vector3(insideUnitCircle.x, 0f, insideUnitCircle.y);
                toTestPos = wantedPosition + Vector3.up * (extent.y + OverlapYOffset);
                
                i++;
            }

            if (i == 20)
                return false;

            Debug.Log($"Found position: {wantedPosition}");

            position = wantedPosition;
            return true;
        }
        
        public Factory StartBuildFactory(int factoryIndex, Vector3 buildPos)
        {
            if (IsFactoryIndexValid(factoryIndex) == false)
                return null;

            //if (_currentState == State.BuildingUnit)
            //    return null;

            GameObject factoryPrefab = _factoryPrefabs[factoryIndex];
            Transform teamRoot = GameServices.GetControllerByTeam(GetTeam())?.GetTeamRoot();
            GameObject factoryInst = Instantiate(factoryPrefab, buildPos, Quaternion.identity, teamRoot);
            factoryInst.name = factoryInst.name.Replace("(Clone)", "_" + _spawnCount);
            Factory newFactory = factoryInst.GetComponent<Factory>();
            newFactory.Init(GetTeam());
            newFactory.StartSelfConstruction();

            return newFactory;
        }
        void StartSelfConstruction()
        {
            _currentState = State.UnderConstruction;

            _buildCurrentTime = 0f;
            _currentBuildDuration = _factoryData.BuildDuration;
        }

        #endregion
    }
}
