namespace RTS.FSM
{
    public class AttackState : State<Unit>
    {
        public override void OnEnter()
        {
            base.OnEnter();

            _owner.SetStateColor(_color);

            _owner.AttackNearestTarget();
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

            if (_owner.EntityTarget == null)
                _owner.AttackNearestTarget();

            if (!_owner.CanAttack(_owner.EntityTarget) && _owner.EntityTarget != null)
                _owner.MoveToEntityTarget();
            else
                _owner.ComputeAttack();
        }
    }
}