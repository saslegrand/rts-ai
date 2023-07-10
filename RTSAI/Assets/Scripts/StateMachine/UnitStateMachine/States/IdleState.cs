namespace RTS.FSM
{
    public class IdleState : State<Unit>
    {
        public override void OnEnter()
        {
            base.OnEnter();

            _owner.SetStateColor(_color);

            if ((_owner.OrderPosition - _owner.transform.position).sqrMagnitude > 1)
                _owner.MoveTo(_owner.OrderPosition);

        }
    }
}