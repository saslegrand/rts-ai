using UnityEngine;

namespace RTS.FSM
{
    public class StateMachine<T> : MonoBehaviour
    {
        private T _owner;

        [SerializeField] private State<T> _currentState;

        [SerializeField] private StateTransition<T>[] _globalTransitions;

        public void Awake()
        {
            _owner = GetComponent<T>();
            foreach (StateTransition<T> transition in _globalTransitions)
            {
                transition.Initialize(_owner);
            }
        }

        private void Update()
        {
            _currentState = CheckTransitions();
            _currentState = _currentState.CheckTransitions();
            if (_currentState)
                _currentState.OnUpdate();
        }

        private void FixedUpdate()
        {
            if (_currentState)
                _currentState.OnFixedUpdate();
        }

        public State<T> CheckTransitions()
        {
            foreach (StateTransition<T> transition in _globalTransitions)
            {
                State<T> newState = transition.CheckConditions();

                if (newState != null && newState != _currentState)
                {
                    _currentState.OnExit();

                    newState.OnEnter();

                    return newState;
                }
            }

            return _currentState;
        }
    }
}
