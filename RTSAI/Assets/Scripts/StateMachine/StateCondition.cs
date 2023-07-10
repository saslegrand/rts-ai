using UnityEngine;

namespace RTS.FSM
{
    public abstract class StateCondition<T> : ScriptableObject
    {
        protected T _owner;
        protected bool _callbackValidated;
        protected bool _inverseCondition;


        public void Initialize(T ownerT, bool inverseCondition)
        {
            _owner = ownerT;
            _inverseCondition = inverseCondition;
        }

        public void ValidateCallBack()
        {
            _callbackValidated = true;
        }

        public bool ValidateCondition()
        {
            if (_inverseCondition)
                return ValidateInverse();

            return Validate();
        }

        protected abstract bool Validate();

        protected virtual bool ValidateInverse()
        {
            return !Validate();
        }
    }
}
