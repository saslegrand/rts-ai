using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace RTS
{
    public class SubSquad
    {
        public float UnitSize { get; private set; } = 2.0f;
        public int Order { get; private set; } = 0;
        public int TypeID { get; private set; } = 0;

        public readonly List<Unit> Units;

        public bool IsEmpty => Units.Count == 0;

        public SubSquad(int typeID, int order, float unitSize)
        {
            TypeID = typeID;
            Order = order;
            UnitSize = unitSize;
            Units = new List<Unit>();
        }

        public SubSquad(SubSquad other)
        {
            TypeID = other.TypeID;
            Order = other.Order;
            UnitSize = other.UnitSize;
            Units = new List<Unit>();
        }

        public void AddUnit(Unit unit)
        {
            if (Units.Contains(unit))
                return;

            Units.Add(unit);
        }

        public void RemoveUnit(Unit unit)
        {
            if (!Units.Contains(unit))
                return;

            Units.Remove(unit);
        }

        public void SetSelected(bool value)
        {
            foreach (Unit unit in Units)
                unit.SetSelected(value);
        }

        #region SubSquad Orders

        public bool HasReachOrderTarget()
        {
            return Units.All(unit => unit.HasReachOrder);
        }
        public void MoveSquadToTarget(Vector3 target, Vector3 facingDir, Vector3 offsetX, ref Vector3 offsetY)
        {
            int numUnits = Units.Count;

            if (numUnits == 1)
            {
                Units[0].SetTargetPos(target + offsetY);
                offsetY -= facingDir * UnitSize;
                return;
            }

            Vector3 leftDir = Quaternion.Euler(0, 90, 0) * facingDir;

            int resolution = 2;
            while (numUnits > resolution * (resolution - 1))
                resolution++;

            float half = (resolution - 1) * UnitSize * 0.5f;
            Vector3 row = target;
            int count = 0;
            Vector3 localOffset = Vector3.zero;
            for (int i = 0; i < resolution - 1; i++)
            {
                int rowCount = Mathf.Clamp(numUnits - count, 0, resolution);
                for (int j = 0; j < resolution; j++)
                {
                    if (count == numUnits)
                        break;

                    Vector3 pos = row;

                    if (rowCount > 1)
                        pos += Vector3.Lerp(leftDir * half, -leftDir * half, j / (float)(rowCount - 1));

                    Units[count].SetTargetPos(pos + offsetY + offsetX);

                    count++;
                }

                row -= facingDir * UnitSize;
                localOffset += facingDir * UnitSize;

                if (count == numUnits)
                    break;
            }

            offsetY -= localOffset;
        }

        public void SetAttackMode(bool value)
        {
            foreach (Unit unit in Units)
            {
                unit.SetAttackMode(value);
            }
        }

        public void SetCaptureTarget(TargetBuilding target)
        {
            foreach (Unit unit in Units)
            {
                unit.SetCaptureTarget(target);
            }
        }

        #endregion
    }

    public class SquadController
    {
        public Dictionary<int, SubSquad> SubSquads;

        public UnityEvent OnSquadEmpty = new UnityEvent();

        public bool IsEmpty => SubSquads.Count == 0;
        public int UnitCount
        {
            get
            {
                return SubSquads.Sum(s => s.Value.Units.Count);
            }
        }

        public List<Unit> GetAllUnits()
        {
            List<Unit> units = new List<Unit>();
            foreach (SubSquad subSquad in SubSquads.Values)
            {
                foreach (Unit unit in subSquad.Units)
                    units.Add(unit);
            }
            return units;
        }

        public SquadController()
        {
            SubSquads = new Dictionary<int, SubSquad>();
        }

        public SquadController(List<Unit> units)
            : this()
        {
            AddUnits(units);
        }

        public void AddUnit(Unit unit)
        {
            int key = unit.GetTypeId;

            if (!SubSquads.ContainsKey(key))
            {
                SubSquads.Add(key, new SubSquad(key, unit.GetUnitData.FormationOrder, unit.GetUnitData.FormationRadius));
            }

            SubSquads[key].AddUnit(unit);
            unit.SquadController = this;
        }

        public void RemoveUnit(Unit unit)
        {
            int key = unit.GetTypeId;

            if (!SubSquads.ContainsKey(key))
                return;

            SubSquads[key].RemoveUnit(unit);
            if (SubSquads[key].IsEmpty)
                SubSquads.Remove(key);

            unit.SquadController = null;

            if (IsEmpty)
                OnSquadEmpty?.Invoke();
        }

        public void AddUnits(List<Unit> units)
        {
            foreach (Unit unit in units)
                AddUnit(unit);
        }

        public void RemoveUnits(List<Unit> units)
        {
            foreach (Unit unit in units)
                RemoveUnit(unit);
        }

        public void SetSelected(bool value)
        {
            foreach (SubSquad subSquad in SubSquads.Values)
                subSquad.SetSelected(value);
        }

        #region Squad Orders

        public bool HasReachOrderTarget()
        {
            foreach (SubSquad subSquad in SubSquads.Values)
            {
                if (subSquad.HasReachOrderTarget())
                    return true;
            }
            return false;
        }

        public void MoveSquadToTarget(Vector3 target, bool attackMode, bool withFormation = true)
        {
            if (withFormation)
                MoveSquad(target, attackMode);
            else
                MoveSquadToTargetWithoutFormation(target, attackMode);
        }

        public void MoveSquadToTargetWithoutFormation(Vector3 target, bool attackMode)
        {
            List<Unit> units = GetAllUnits();
            foreach (Unit unit in units)
                unit.SetTargetPos(target);

            SetAttackMode(attackMode);
        }

        public void MoveSquadToTargetWithoutFormation(TargetBuilding target, bool attackMode)
        {
            List<Unit> units = GetAllUnits();
            foreach (Unit unit in units)
                unit.MoveToTargetBuilding(target);

            SetAttackMode(attackMode);
        }

        public void MoveSquadToTargetWithoutFormation(BaseEntity target, bool attackMode)
        {
            List<Unit> units = GetAllUnits();
            foreach (Unit unit in units)
                unit.MoveToEntityTarget(target);

            SetAttackMode(attackMode);
        }

        private void SetAttackMode(bool attackMode)
        {
            foreach (SubSquad subSquad in SubSquads.Values)
                subSquad.SetAttackMode(attackMode);
        }

        private void MoveSquad(Vector3 target, bool attackMode)
        {
            MoveSquad(target);
            SetAttackMode(attackMode);
        }

        private void MoveSquad(Vector3 target)
        {
            Vector3 facingDir = (target - GetSquadCenter()).normalized;
            if (facingDir == Vector3.zero)
            {
                Vector2 rng = Random.insideUnitCircle.normalized;
                facingDir = new Vector3(rng.x, 0, rng.y);
            }

            SubSquad[] squads = SubSquads.Values.OrderBy(squad => squad.Order).ToArray();

            List<List<SubSquad>> orderSquadsList = new List<List<SubSquad>>();

            int order = -1;
            for (int i = 0; i < squads.Length; i++)
            {
                SubSquad subSquad = squads[i];
                if (subSquad.Order != order)
                {
                    order = subSquad.Order;
                    orderSquadsList.Add(new List<SubSquad>());
                }
                orderSquadsList[^1].Add(subSquad);
            }

            /*
            List<List<SubSquad>> divOrderSquadsList = new List<List<SubSquad>>();
            for (int i = 0; i < orderSquadsList.Count; i++)
            {
                divOrderSquadsList.Add(new List<SubSquad>());
                divOrderSquadsList[^1].Add(orderSquadsList[i][0]);

                for (int j = 1; j < orderSquadsList[i].Count; j++)
                {
                    SubSquad sq1 = new SubSquad(orderSquadsList[i][j]);
                    SubSquad sq2 = new SubSquad(orderSquadsList[i][j]);
                    for (int k = 0; k < orderSquadsList[i][j].Units.Count; k++)
                    {
                        if (k % 2 == 0)
                            sq1.AddUnit(orderSquadsList[i][j].Units[k]);
                        else
                            sq2.AddUnit(orderSquadsList[i][j].Units[k]);
                    }
                    divOrderSquadsList[^1].Add(sq1);
                    divOrderSquadsList[^1].Add(sq2);
                }
            }
            */


            for (int i = 0; i < orderSquadsList.Count; i++)
            {
                orderSquadsList[i] = orderSquadsList[i].OrderBy(squad => squad.TypeID).ToList();
            }


            Vector3 offsetY = Vector3.zero;

            Vector3 leftDir = Quaternion.Euler(0, 90, 0) * facingDir;

            for (int y = 0; y < orderSquadsList.Count; y++)
            {
                Vector3 offsetYrow = offsetY;

                List<SubSquad> orderSquad = orderSquadsList[y];
                List<float> orderSquadWidths = new List<float>();
                float width = 0.0f;

                foreach (SubSquad subSquad in orderSquad)
                {
                    int numUnits = subSquad.Units.Count;
                    int resolution = 2;
                    while (numUnits > resolution * (resolution - 1))
                        resolution++;
                    float size = (resolution - 1) * subSquad.UnitSize;
                    width += size;
                    orderSquadWidths.Add(size);
                }

                float mid = width * 0.5f;
                float widthCount = 0.0f;
                for (int x = 0; x < orderSquad.Count; x++)
                {
                    float localMid = orderSquadWidths[x] * 0.5f;
                    float globalMid = mid - (widthCount + localMid);
                    widthCount += orderSquadWidths[x];
                    Vector3 offsetX = leftDir * globalMid;

                    Vector3 offsetYRowTemp = offsetYrow;
                    orderSquad[x].MoveSquadToTarget(target, facingDir, offsetX, ref offsetYRowTemp);
                    offsetY = offsetYRowTemp;
                }
            }
        }

        public Vector3 GetSquadCenter()
        {
            Vector3 center = Vector3.zero;
            int count = 0;
            foreach (var squad in SubSquads)
            {
                foreach (var unit in squad.Value.Units)
                {
                    center += unit.transform.position;
                    count++;
                }
            }
            center /= count;
            return center;
        }

        public void SetCaptureTarget(TargetBuilding target)
        {
            foreach (SubSquad subSquad in SubSquads.Values)
            {
                subSquad.SetCaptureTarget(target);
            }
        }
        #endregion
    }
}