using UnityEngine;

namespace RTS.AI.Planner
{
    using Actions;

    [System.Serializable]
    [CreateAssetMenu(fileName = "MoveToTargetAction", menuName = "AI/Planner/MoveToTarget")]
    public class MoveToTarget : Action<SquadLeaderAgent, SquadLeaderState>
    {
        public override void OnActionStart()
        {
            Debug.Log("Moving to target");
            _owner.MoveTo(_owner.OrderTarget);
        }

        public override ActionState Execute()
        {
            if (_owner.HasReachOrderTarget())
                return ActionState.Accomplished;

            return ActionState.Performed;
        }
    }
}