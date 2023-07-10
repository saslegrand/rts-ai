using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.AI.Planner
{
    [System.Flags]
    public enum SquadLeaderState
    {
        IsGrouped = 1 << 0,
        IsMoving = 1 << 1,
        IsAttacking = 1 << 2,
        HasReachedTarget = 1 << 3,
        IsHoldingPosition = 1 << 4,
        HasEnemyNearby = 1 << 5
    }
}
