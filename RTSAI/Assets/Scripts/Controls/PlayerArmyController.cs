using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS
{
    public class PlayerArmyController : ArmyController
    {
        private const int MAX_SQUADS = 10;

        public PlayerArmyController()
            : base()
        {
            for (int i = 0; i < MAX_SQUADS; i++)
            {
                _squads.Add(new SquadController());
            }
        }

        public override int SaveSquad(int index = -1)
        {
            if (IsIndexValid(index))
            {
                _squads[index] = _currentSelectedSquad;
                return index;
            }

            index = 0;
            while (index < SquadCount && !_squads[index].IsEmpty)
                index++;

            if (index >= MAX_SQUADS)
                return -1;

            _squads[index] = _currentSelectedSquad;
            return index;
        }

        public override void SelectUnit(Unit unit)
        {
            base.SelectUnit(unit);

            if (unit != null)
                unit.SetSelected(true);
        }

        public override void SelectSquad(int index)
        {
            base.SelectSquad(index);

            if (_currentSelectedSquad != null)
                _currentSelectedSquad.SetSelected(true);
        }

        public override void UnselectSquad()
        {
            if (_currentSelectedSquad != null)
                _currentSelectedSquad.SetSelected(false);

            base.UnselectSquad();
        }
    }
}