using UnityEngine;

namespace RTS.FSM
{
    [CreateAssetMenu(fileName = "OrderHasChanged", menuName = "AI Conditions/OrderHasChanged")]
    public class OrderHasChanged : StateCondition<Unit>
    {
        protected override bool Validate()
        {
            return !_owner.HasReachOrder;
        }

    }
}
