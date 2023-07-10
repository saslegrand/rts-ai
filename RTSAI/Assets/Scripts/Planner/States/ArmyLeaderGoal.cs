using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.AI.Planner
{
    public enum ArmyLeaderGoal
    {
        CaptureTarget,

        Defend,

        Attack,

        ProduceFactory,

        ProduceUnits,

        None
    }
}