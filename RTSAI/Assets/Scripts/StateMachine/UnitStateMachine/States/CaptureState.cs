namespace RTS.FSM
{
    public class CaptureState : State<Unit>
    {
        public override void OnEnter()
        {
            base.OnEnter();

            _owner.SetStateColor(_color);

            if (_owner.CaptureTargetOrder != null)
            {
                _owner.MoveToTargetBuilding(_owner.CaptureTargetOrder);
            }
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (_owner.CanCapture(_owner.CaptureTargetOrder))
            {
                _owner.StartCapture(_owner.CaptureTargetOrder);
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            _owner.SetTargetPos(_owner.OrderPosition);
        }
    }
}