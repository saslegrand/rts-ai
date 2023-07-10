using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace RTS
{
    // points system for units creation (Ex : light units = 1 pt, medium = 2pts, heavy = 3 pts)
    // max points can be increased by capturing TargetBuilding entities
    public class TeamController : MonoBehaviour
    {
        [SerializeField]
        protected ETeam _team;

        [SerializeField]
        protected int _startingBuildPoints = 15;

        protected int _totalBuildPoints;

        protected int _capturedTargets;

        protected Transform _teamRoot;

        public List<Factory> FactoryList { get; protected set; }
        public List<Unit> UnitList { get; protected set; }
        
        protected Factory _selectedFactory;

        protected ArmyController _armyController;

        // events
        protected Action _onBuildPointsUpdated;
        protected Action _onCaptureTarget;
        
        public ETeam GetTeam() { return _team; }
        public Transform GetTeamRoot() { return _teamRoot; }

        public int TotalBuildPoints
        {
            get => _totalBuildPoints;
            set
            {
                //Debug.Log("TotalBuildPoints updated");
                _totalBuildPoints = value;
                _onBuildPointsUpdated?.Invoke();
            }
        }
        
        public int CapturedTargets
        {
            get => _capturedTargets;
            set
            {
                _capturedTargets = value;
                _onCaptureTarget?.Invoke();
            }
        }

        #region Unit methods
        protected virtual void SelectAllUnits()
        {
            _armyController.SelectUnits(UnitList);
        }

        protected virtual void UnselectAllUnits()
        {
            _armyController.UnselectSquad();
        }

        protected virtual void SelectAllUnitsByTypeId(int typeId)
        {
            UnselectCurrentFactory();
            UnselectAllUnits();
            List<Unit> units = UnitList.FindAll(unit => unit.GetTypeId == typeId);

            _armyController.SelectUnits(units);
        }



        public virtual void AddUnit(Unit unit)
        {
            unit.OnDeadEvent += () =>
            {
                if (unit.IsSelected)
                    _armyController.UnselectUnit(unit);
                UnitList.Remove(unit);
            };
            UnitList.Add(unit);
        }
        public void CaptureTarget()
        {
            CapturedTargets++;
        }
        public void LoseTarget()
        {
            CapturedTargets--;
        }

        public void GainPoints(int add)
        {
            TotalBuildPoints += add;
        }
        #endregion

        #region Factory methods
        private void AddFactory(Factory factory)
        {
            if (!factory)
                return;

            factory.OnDeadEvent += () =>
            {
                TotalBuildPoints += factory.Cost;
                if (factory.IsSelected)
                    _selectedFactory = null;
                FactoryList.Remove(factory);
            };
            
            FactoryList.Add(factory);
        }
        public virtual void SelectFactory(Factory factory)
        {
            if (!factory|| factory.IsUnderConstruction)
                return;

            _selectedFactory = factory;
            _selectedFactory.SetSelected(true);
            UnselectAllUnits();
        }
        public virtual void UnselectCurrentFactory()
        {
            if (_selectedFactory)
                _selectedFactory.SetSelected(false);
            _selectedFactory = null;
        }
        public bool RequestUnitBuild(int unitMenuIndex)
        {
            if (_selectedFactory == null)
                return false;

            return _selectedFactory.RequestUnitBuild(unitMenuIndex);
        }
        
        public bool RequestFactoryBuild(int factoryIndex, Vector3 buildPos)
        {
            if (!_selectedFactory)
                return false;

            int cost = _selectedFactory.GetFactoryCost(factoryIndex);
            if (TotalBuildPoints < cost)
                return false;

            // Check if position is valid
            if (!_selectedFactory.CanPositionFactory(factoryIndex, buildPos))
                return false;

            Factory newFactory = _selectedFactory.StartBuildFactory(factoryIndex, buildPos);

            if (!newFactory) return false;

            AddFactory(newFactory);
            TotalBuildPoints -= cost;

            return true;
        }

        public bool ForceRequestFactoryBuild(int factoryIndex, Vector3 center)
        {
            if (!_selectedFactory)
                return false;

            int cost = _selectedFactory.GetFactoryCost(factoryIndex);
            if (TotalBuildPoints < cost)
                return false;
            
            Vector3 buildPos = Vector3.zero;
            if (!_selectedFactory.GetValidPosition(factoryIndex, center, ref buildPos))
                return false;

            Factory newFactory = _selectedFactory.StartBuildFactory(factoryIndex, buildPos);

            if (!newFactory) return false;

            AddFactory(newFactory);
            TotalBuildPoints -= cost;

            return true;
        }
        #endregion

        #region MonoBehaviour methods
        protected virtual void Awake()
        {
            FactoryList = new List<Factory>();
            UnitList = new List<Unit>();
            string rootName = _team + "Team";
            _teamRoot = GameObject.Find(rootName)?.transform;
            //if (_teamRoot)
            //    Debug.LogFormat("TeamRoot {0} found !", rootName);

            _armyController = new ArmyController();

            CapturedTargets = 0;
            TotalBuildPoints = _startingBuildPoints;

            // get all team factory already in scene
            Factory[] allFactories = FindObjectsOfType<Factory>();
            foreach (Factory factory in allFactories)
            {
                if (factory.GetTeam() == GetTeam())
                {
                    AddFactory(factory);
                }
            }
            //Debug.Log("found " + FactoryList.Count + " factory for team " + GetTeam());
        }

        protected virtual void Start()
        {

        }

        protected virtual void Update()
        {

        }
        #endregion
    }
}