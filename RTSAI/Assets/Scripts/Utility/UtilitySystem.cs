using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RTS.AI.Tools
{
    public class UtilitySystem : MonoBehaviour
    {
        [SerializeField] private List<Utility> _utilities;

        private void Awake()
        {
            foreach (Utility utility in _utilities)
                utility.Initialize();
        }

        public UtilityAction Evaluate()
        {
            UtilityAction bestAction = null;
            float bestActionScore = -float.MaxValue;
            
            foreach (Utility utility in _utilities)
            {
                float utilityScore = utility.Evaluate();
                if (utilityScore > bestActionScore)
                {
                    bestActionScore = utilityScore;
                    bestAction = utility.Action;
                }
            }

            return bestAction;
        }

        public UtilityAction[] EvaluateOrdered()
        {
            List<KeyValuePair<int, float>> actionCosts = new List<KeyValuePair<int, float>>();
            
            for (int utilityIndex = 0; utilityIndex < _utilities.Count; utilityIndex++)
                actionCosts.Add(new KeyValuePair<int, float>(utilityIndex, _utilities[utilityIndex].Evaluate()));

            actionCosts.Sort((lhs, rhs) => rhs.Value.CompareTo(lhs.Value));

            float sumCosts = actionCosts.Sum(pair => pair.Value);
            
            UtilityAction[] actions = new UtilityAction[_utilities.Count];
            for (int costIndex = 0; costIndex < actions.Length; costIndex++)
            {
                var cost = actionCosts[costIndex];
                actions[costIndex] = _utilities[cost.Key].Action;
                actions[costIndex].Cost = cost.Value;
                actions[costIndex].Percent = sumCosts > float.MinValue ? cost.Value / sumCosts : 0;
            }

            
            return actions;
        }
    }
}