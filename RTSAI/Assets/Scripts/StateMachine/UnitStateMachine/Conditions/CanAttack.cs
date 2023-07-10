using UnityEngine;

namespace RTS.FSM
{
    [CreateAssetMenu(fileName = "CanAttack", menuName = "AI Conditions/CanAttack")]
    public class CanAttack : StateCondition<Unit>
    {
        protected override bool Validate()
        {
            if (!_owner.HasReachOrder && !_owner.AttackMode)
                return false;

            int unitMask = LayerMask.GetMask("Unit") | LayerMask.GetMask("Factory");
            Collider[] units = Physics.OverlapSphere(_owner.transform.position, _owner.Visibility.AggroRange, unitMask);

            if (units.Length == 0)
                return false;


            float minDist = float.MaxValue;
            BaseEntity nearUnit = null;

            foreach (Collider c in units)
            {
                if (c.TryGetComponent(out BaseEntity unit) && unit.IsAlive && unit.GetTeam() == GameServices.GetOpponent(_owner.GetTeam()))
                {
                    float dist = (unit.transform.position - _owner.transform.position).sqrMagnitude;
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearUnit = unit;
                    }
                }
            }

            if (nearUnit == null)
                return false;

            _owner.SetAttackNearestTarget(nearUnit);
            return true;
        }

    }
}
