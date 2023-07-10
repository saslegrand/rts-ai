using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RTS.FSM
{
    [Serializable]
    public class Condition<T>
    {
        public StateCondition<T> StateCondition;
        public bool InverseCondition;
    }


    [Serializable]
    public class StateTransition<T>
    {
        [SerializeField] protected List<Condition<T>> _conditions;
        [SerializeField] protected State<T> _nextState;

        private List<StateCondition<T>> _conditionInstances;

        public void Initialize(T owner)
        {
            _conditionInstances = new List<StateCondition<T>>();
            foreach (var condition in _conditions)
            {
                StateCondition<T> conditionInstance = Object.Instantiate(condition.StateCondition);
                conditionInstance.Initialize(owner, condition.InverseCondition);
                _conditionInstances.Add(conditionInstance);
            }
        }

        public State<T> CheckConditions()
        {
            if (_conditionInstances == null || _conditionInstances.Count == 0)
                return null;

            foreach (var condition in _conditionInstances)
            {
                if (!condition.ValidateCondition())
                    return null;
            }
            return _nextState;
        }
    }
}
