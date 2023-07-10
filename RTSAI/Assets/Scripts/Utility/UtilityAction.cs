using UnityEngine;

namespace RTS.AI.Tools
{
    using Planner;

    [System.Serializable]
    public class UtilityAction
    {
        public ArmyLeaderGoal Goal;
        
        [HideInInspector]
        public float Percent;

        [HideInInspector] 
        public float Cost;
    }
}