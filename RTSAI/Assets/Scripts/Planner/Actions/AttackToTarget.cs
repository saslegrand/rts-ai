using UnityEngine;

namespace RTS.AI.Planner
{
    using Actions;

    [System.Serializable]
    [CreateAssetMenu(fileName = "AttackToTargetAction", menuName = "AI/Planner/AttackToTarget")]
    public class AttackToTarget : Action<SquadLeaderAgent, SquadLeaderState>
    {
        public override void OnActionStart()
        {
            Debug.Log("Attacking to target");
            _owner.MoveTo(_owner.OrderTarget, true);
        }

        public override ActionState Execute()
        {
            if (_owner.HasReachOrderTarget())
                return ActionState.Accomplished;

            return ActionState.Performed;
        }
    }
}