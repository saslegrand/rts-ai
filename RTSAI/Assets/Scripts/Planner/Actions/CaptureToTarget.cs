using UnityEngine;

namespace RTS.AI.Planner
{
    using Actions;

    [System.Serializable]
    [CreateAssetMenu(fileName = "CaptureToTargetAction", menuName = "AI/Planner/CaptureToTarget")]
    public class CaptureToTarget : Action<SquadLeaderAgent, SquadLeaderState>
    {
        public override void OnActionStart()
        {
            Debug.Log("Capturing to target");
        }

        public override ActionState Execute()
        {
            return ActionState.Accomplished;
        }
    }
}