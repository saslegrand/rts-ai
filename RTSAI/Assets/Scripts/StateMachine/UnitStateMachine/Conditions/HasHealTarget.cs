using UnityEngine;

namespace RTS.FSM
{
    [CreateAssetMenu(fileName = "HasHealTarget", menuName = "AI Conditions/HasHealTarget")]
    public class HasHealTarget : StateCondition<Unit>
    {
        protected override bool Validate()
        {
            return _owner.EntityTarget != null && _owner.EntityTarget.IsAlive && _owner.EntityTarget.GetTeam() == _owner.GetTeam();
        }
    }
}