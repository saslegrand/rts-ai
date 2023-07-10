using UnityEngine;

namespace RTS.FSM
{
    [System.Serializable]
    public abstract class State<T> : MonoBehaviour
    {
        [SerializeField] protected Color _color;

        [SerializeField] private StateTransition<T>[] _transitions;

        protected T _owner;

        public void Awake()
        {
            _owner = GetComponent<T>();
            foreach (StateTransition<T> transition in _transitions)
            {
                transition.Initialize(_owner);
            }
        }

        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void OnUpdate() { }

        public virtual void OnFixedUpdate() { }

        public State<T> CheckTransitions()
        {
            foreach (StateTransition<T> transition in _transitions)
            {
                State<T> newState = transition.CheckConditions();

                if (newState != null && newState != this)
                {
                    OnExit();

                    newState.OnEnter();

                    return newState;
                }
            }

            return this;
        }
    }
}

