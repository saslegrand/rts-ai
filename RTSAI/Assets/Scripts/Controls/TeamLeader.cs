using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RTS.AI.Tools;

namespace RTS.AI
{
    [System.Serializable]
    public class UnitPlanning
    {
        public int TypeID = 0;
        public int Count = 1;

        public UnitPlanning(UnitPlanning other)
        {
            TypeID = other.TypeID;
            Count = other.Count;
        }
    }

    [System.Serializable]
    public class UnitProductionPlanning
    {
        public List<UnitPlanning> Plan;

        [HideInInspector] public int Cost = 0;

        public UnitProductionPlanning(UnitProductionPlanning other)
        {
            Plan = new List<UnitPlanning>();
            foreach (UnitPlanning p in other.Plan)
            {
                Plan.Add(new UnitPlanning(p));
            }

            Cost = other.Cost;
        }
    }

    public class TeamLeader : MonoBehaviour
    {
        [SerializeField] private AIController _controller;

        [SerializeField] private UnitProductionPlanning[] _productionPlans;

        [SerializeField] private Unit[] _unitsAvailable;


        private UnitProductionPlanning _currentProductionPlan = null;


        private void Awake()
        {
            ComputePlanningCosts();
        }

        public void Produce(bool canProduceFactory, bool canProduceUnits)
        {
            if (canProduceUnits)
            {
                TryProduceUnits();
                ProduceUnits();
            }

            if (canProduceFactory)
            {
                TryProduceFactory();
            }
        }

        public void InterpretGoals(UtilityAction[] goals)
        {
            // Handle goals to define the current team production
            
            // Produce factories
            // Produce units
            // Wait
        }

        private void ComputePlanningCosts()
        {
            foreach (UnitProductionPlanning planning in _productionPlans)
            {
                planning.Cost = 0;
                foreach (UnitPlanning plan in planning.Plan)
                {
                    planning.Cost += _unitsAvailable[plan.TypeID].Cost;
                }
            }

            _productionPlans.OrderBy(p => p.Cost);
        }


        private void TryProduceFactory()
        {
            if (_controller.FactoryList.Count == 0)
                return;

            int factoryIndex = 1;

            if (_controller.FactoryList.Count >= 2)
            {
                factoryIndex = Random.Range(0, 2);
            }

            _controller.SelectFactory(_controller.FactoryList[^1]);

            Vector3 basePosition = _controller.FactoryList[0].transform.position;
            Vector2 insideUnitCircle = Random.insideUnitCircle.normalized * Random.Range(10, 50.0f);
            Vector3 wantedPosition = basePosition + new Vector3(insideUnitCircle.x, 0, insideUnitCircle.y);

            //_controller.RequestFactoryBuild(factoryIndex, wantedPosition);
            _controller.ForceRequestFactoryBuild(factoryIndex, _controller.FactoryList[^1].transform.position);

            _controller.UnselectCurrentFactory();
        }

        private void TryProduceUnits()
        {
            if (_currentProductionPlan != null)
                return;

            List<UnitProductionPlanning> plannings = _productionPlans.ToList();

            for (int i = plannings.Count - 1; i >= 0; i--)
            {
                if (_controller.TotalBuildPoints < plannings[i].Cost)
                {
                    plannings.RemoveAt(i);
                    return;
                }
            }

            int rng = Random.Range(0, plannings.Count);
            _currentProductionPlan = new UnitProductionPlanning(plannings[rng]);
        }

        private void ProduceUnits()
        {
            if (_currentProductionPlan == null)
                return;

            foreach (UnitPlanning plan in _currentProductionPlan.Plan)
            {
                int factoryID = plan.TypeID > 2 ? 1 : 0;
                int unitID = plan.TypeID > 2 ? plan.TypeID - 3 : plan.TypeID;

                foreach (Factory factory in _controller.FactoryList)
                {
                    if (factory.FactoryData.TypeId != factoryID)
                        continue;

                    _controller.SelectFactory(factory);

                    if (plan.Count > 0)
                    {
                        while (plan.Count > 0)
                        {
                            if (factory.RequestUnitBuild(unitID))
                                plan.Count--;
                            else
                                break;
                        }
                    }

                    _controller.UnselectCurrentFactory();
                }
            }

            _currentProductionPlan = null;
        }

    }
}