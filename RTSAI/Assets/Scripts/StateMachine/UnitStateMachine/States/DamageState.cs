namespace RTS.FSM
{
    public class DamageState : State<Unit>
    {
        public override void OnEnter()
        {
            base.OnEnter();

            _owner.SetStateColor(_color);
        }
    }
}