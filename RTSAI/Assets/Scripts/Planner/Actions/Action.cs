using System.Collections.Generic;
using UnityEngine;

namespace RTS.AI.Planner.Actions
{
    public enum ActionState
    {
        Performed,
        Accomplished,
        Compromised
    }
    
    public abstract class Action<Tagent, Tenum> : ScriptableObject where Tenum : System.Enum
    {
        [SerializeField] private List<Goal<Tenum>> _preconditions;
        [SerializeField] protected List<Goal<Tenum>> _effects;

        [SerializeField] private float _baseCost = 2;

        protected Tagent _owner;

        public virtual float GetCost(WorldState worldState)
        {
            return _baseCost;
        }

        public virtual void Initialize(Tagent owner)
        {
            _owner = owner;
        }

        public virtual void OnActionStart() { }

        public virtual void OnActionEnd() { }
        public virtual void OnActionFail() { }

        // Execute the action
        public abstract ActionState Execute();

        // Check if all the preconditions in the world state are valid to execute the action
        public virtual bool CheckPreconditions(WorldState worldState)
        {
            if (_preconditions == null)
                return false;

            return worldState.TestConditions(_preconditions);
        }

        // Update the world state given the needed specificities of the action
        public virtual WorldState ApplyEffects(WorldState worldState)
        {
            WorldState newWorldState = new WorldState(worldState);

            if (_effects == null)
                return newWorldState;

            newWorldState.SetConditions(_effects);

            return newWorldState;
        }
    }
}