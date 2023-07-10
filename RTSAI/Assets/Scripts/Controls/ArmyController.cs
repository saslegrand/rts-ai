using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace RTS
{
    public class ArmyController
    {
        protected List<SquadController> _squads;
        public int SquadCount => _squads.Count;

        // Buffer temporaires des unités sélectionnés (que ce soit l'IA ou le Player)
        protected SquadController _currentSelectedSquad;
        public SquadController CurrentSelectedSquad => _currentSelectedSquad;

        public ArmyController()
        {
            _squads = new List<SquadController>();
        }


        protected bool IsIndexValid(int index)
        {
            return index >= 0 && index < SquadCount;
        }

        public virtual void SelectUnit(Unit unit)
        {
            if (unit == null || !unit.IsAlive)
                return;
            
            if (_currentSelectedSquad == null)
                _currentSelectedSquad = new SquadController();

            _currentSelectedSquad.AddUnit(unit);
        }

        public virtual void UnselectUnit(Unit unit)
        {
            if (unit == null)
                return;

            if (_currentSelectedSquad != null)
                _currentSelectedSquad.RemoveUnit(unit);
        }

        public void SwitchSelectUnit(Unit unit)
        {
            if (unit.IsSelected)
                UnselectUnit(unit);
            else
                SelectUnit(unit);
        }

        public void SelectUnits(List<Unit> units)
        {
            foreach (Unit unit in units)
                SelectUnit(unit);
        }

        public void UnselectUnits(List<Unit> units)
        {
            foreach (Unit unit in units)
                UnselectUnit(unit);
        }

        public void SelectOrUnselectSquad(int index)
        {
            if (IsIndexValid(index))
            {
                if (_currentSelectedSquad == _squads[index])
                {
                    UnselectSquad();
                    return;
                }

                SelectSquad(index);
            }
        }

        public virtual void SelectSquad(int index)
        {
            if (IsIndexValid(index))
                _currentSelectedSquad = _squads[index];
        }

        public virtual void UnselectSquad()
        {
            _currentSelectedSquad = null;
        }



        public virtual int SaveSquad(int index = -1)
        {
            if (IsIndexValid(index))
            {
                _squads[index] = _currentSelectedSquad;
                return index;
            }

            index = 0;
            while (index < SquadCount && !_squads[index].IsEmpty)
                index++;

            if (index >= SquadCount)
            {
                _squads.Add(_currentSelectedSquad);
                return SquadCount - 1;
            }

            _squads[index] = _currentSelectedSquad;
            return index;
        }



        #region Squad Orders
        public void MoveCurrentSquad(Vector3 target, bool attackMode = false)
        {
            if (_currentSelectedSquad == null)
                return;

            _currentSelectedSquad.MoveSquadToTarget(target, attackMode);
        }

        public void MoveCurrentSquadWithoutFormation(Vector3 target, bool attackMode = false)
        {
            if (_currentSelectedSquad == null)
                return;

            _currentSelectedSquad.MoveSquadToTargetWithoutFormation(target, attackMode);
        }

        public void MoveCurrentSquadWithoutFormation(BaseEntity target, bool attackMode = false)
        {
            if (_currentSelectedSquad == null)
                return;

            _currentSelectedSquad.MoveSquadToTargetWithoutFormation(target, attackMode);
        }

        public void MoveCurrentSquadWithoutFormation(TargetBuilding target, bool attackMode = false)
        {
            if (_currentSelectedSquad == null)
                return;

            _currentSelectedSquad.MoveSquadToTargetWithoutFormation(target, attackMode);
        }

        public void MoveSquad(int index, Vector3 target)
        {
            if (IsIndexValid(index))
            {
                _squads[index].MoveSquadToTarget(target, false);
            }
        }

        public void MoveAttackCurrentSquad(Vector3 target)
        {
            if (_currentSelectedSquad == null)
                return;

            _currentSelectedSquad.MoveSquadToTarget(target, true);
        }

        public void SetCaptureTarget(TargetBuilding target)
        {
            if (_currentSelectedSquad == null)
                return;

            _currentSelectedSquad.SetCaptureTarget(target);
        }
        #endregion
    }
}