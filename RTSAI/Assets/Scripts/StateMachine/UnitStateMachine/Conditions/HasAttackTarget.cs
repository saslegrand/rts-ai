using UnityEngine;

namespace RTS.FSM
{
    [CreateAssetMenu(fileName = "HasAttackTarget", menuName = "AI Conditions/HasAttackTarget")]
    public class HasAttackTarget : StateCondition<Unit>
    {
        protected override bool Validate()
        {
            return _owner.EntityTarget != null && _owner.EntityTarget.IsAlive && _owner.EntityTarget.GetTeam() != _owner.GetTeam();
        }
    }
}