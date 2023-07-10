using System.Collections.Generic;
using UnityEngine;

namespace RTS.FSM
{
    [CreateAssetMenu(fileName = "CanHeal", menuName = "AI Conditions/CanHeal")]
    public class CanHeal : StateCondition<Unit>
    {
        protected override bool Validate()
        {
            if (!_owner.GetUnitData.CanRepair)
                return false;

            List<Unit> units = GameServices.GetControllerByTeam(_owner.GetTeam()).UnitList;

            Unit healUnit = null;
            float minDist = float.MaxValue;
            foreach (Unit unit in units)
            {
                if (unit == _owner)
                    continue;

                if (!unit.NeedsRepairing())
                    continue;

                float dist = (unit.transform.position - _owner.transform.position).sqrMagnitude;

                if (dist < minDist && dist <= _owner.GetUnitData.RepairDistanceMax)
                {
                    minDist = dist;
                    healUnit = unit;
                }
            }

            if (healUnit == null)
                return false;

            _owner.SetRepairTarget(healUnit);
            return true;
        }
    }
}