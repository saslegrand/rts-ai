using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RTS.AI.Tools
{
    [System.Serializable]
    public class Utility
    {
        [SerializeField] private UtilityAction _action;
        [SerializeField] private List<UtilityCondition> _conditions;

        public UtilityAction Action => _action;

        public void Initialize()
        {
            foreach (UtilityCondition condition in _conditions)
                condition.Initialize();
        }
        
        public virtual float Evaluate()
        {
            return _conditions.Sum(condition => condition.Evaluate());
        }
    }
}