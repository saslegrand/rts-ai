using System;
using System.Collections.Generic;
using System.Linq;
using SL.Tools;

namespace RTS.AI.Planner
{
    public class WorldState
    {
        public readonly BitFlag States;

        public WorldState()
        {
            States = new BitFlag();
        }

        public WorldState(WorldState worldState)
        {
            States = new BitFlag(worldState.States);
        }

        public bool IsEqual(WorldState worldState)
        {
            return States.Equal(worldState.States);
        }

        public bool TestConditions<T>(List<Goal<T>> conditions) where T : Enum
        {
            for (int conditionIndex = 0; conditionIndex < conditions.Count; conditionIndex++)
            {
                Goal<T> condition = conditions[conditionIndex];
                if (States.IsEnable(Convert.ToInt32(condition.Target)) != condition.Value)
                    return false;
            }

            return true;
        }

        public void SetConditions<T>(List<Goal<T>> conditions) where T : Enum
        {
            for (int conditionIndex = 0; conditionIndex < conditions.Count; conditionIndex++)
            {
                Goal<T> condition = conditions[conditionIndex];
                States.SetFlag(Convert.ToInt32(condition.Target), condition.Value);
            }
        }
    }
}