namespace RTS.FSM
{
    public class DeathState : State<Unit>
    {
        public override void OnEnter()
        {
            base.OnEnter();

            _owner.SetStateColor(_color);
        }
    }
}