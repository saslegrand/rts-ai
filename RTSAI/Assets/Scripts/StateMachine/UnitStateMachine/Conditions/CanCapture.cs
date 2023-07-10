using UnityEngine;

namespace RTS.FSM
{
    [CreateAssetMenu(fileName = "CanCapture", menuName = "AI Conditions/CanCapture")]
    public class CanCapture : StateCondition<Unit>
    {
        protected override bool Validate()
        {
            return _owner.SeeCapture(_owner.CaptureTargetOrder);
        }
    }
}
