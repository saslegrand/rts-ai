using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.FSM
{
    public class HealState : State<Unit>
    {
        public override void OnEnter()
        {
            base.OnEnter();

            _owner.SetStateColor(_color);

            
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();


            if (!_owner.CanRepair(_owner.EntityTarget) && _owner.EntityTarget != null)
                _owner.MoveToEntityTarget();
            else
                _owner.ComputeRepairing();
        }
    }
}