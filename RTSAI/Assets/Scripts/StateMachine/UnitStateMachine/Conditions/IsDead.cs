using UnityEngine;

namespace RTS.FSM
{
    [CreateAssetMenu(fileName = "IsDead", menuName = "AI Conditions/IsDead")]
    public class IsDead : StateCondition<Unit>
    {
        protected override bool Validate()
        {
            return !_owner.IsAlive;
        }
    }
}