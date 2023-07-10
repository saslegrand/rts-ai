using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.AI
{
    using Planner;
    using Planner.Actions;

    public class SquadLeaderExecutioner : MonoBehaviour
    {
        [SerializeField] private List<Action<SquadLeaderAgent, SquadLeaderState>> _actions;

        private List<Action<SquadLeaderAgent, SquadLeaderState>> _actionInstances;
        protected List<Goal<SquadLeaderState>> _goals;

        private Action<SquadLeaderAgent, SquadLeaderState> _curAction;
        private SquadLeaderAgent _owner;
        private Queue<Action<SquadLeaderAgent, SquadLeaderState>> _plan;

        private List<Goal<SquadLeaderState>> Goals => _goals;

        // Start is called before the first frame update
        void Start()
        {
            _owner = GetComponent<SquadLeaderAgent>();

            // Create action instances
            _actionInstances = new List<Action<SquadLeaderAgent, SquadLeaderState>>();
            foreach (Action<SquadLeaderAgent, SquadLeaderState> actionTemplate in _actions)
            {
                Action<SquadLeaderAgent, SquadLeaderState> actionInstance = Instantiate(actionTemplate);
                actionInstance.Initialize(_owner);

                _actionInstances.Add(actionInstance);
            }

            FindNewPlan();
        }

        protected void FindNewGoal()
        {
            _goals = new List<Goal<SquadLeaderState>>()
            {
                new Goal<SquadLeaderState>()
                {
                    Target = SquadLeaderState.IsGrouped,
                    Value = true
                }
            };
        }

        private void FindNewPlan()
        {
            FindNewGoal();

            if (_goals == null)
                return;

            _plan = Goap<SquadLeaderAgent, SquadLeaderState>.FindPlan(_actionInstances, _owner.CurWorldState, Goals);

            if (_plan == null)
            {
                Debug.LogWarning("No plan found for the agent");
                return;
            }

            _curAction = _plan.Dequeue();
            _curAction.OnActionStart();
        }

        private void OnActionAccomplished()
        {
            _curAction.OnActionEnd();
            _owner.CurWorldState = _curAction.ApplyEffects(_owner.CurWorldState);

            if (_plan.Count == 0)
            {
                Debug.Log("Plan is finished");
                _plan = null;
                FindNewPlan();
                return;
            }

            _curAction = _plan.Dequeue();
            _curAction.OnActionStart();
        }

        private void OnActionCompromised()
        {
            _curAction.OnActionFail();

            _plan = null;
        }

        public void Execute()
        {
            if (_plan == null)
            {
                // No plan, or on research of a new plan
                // Stay idle
                return;
            }

            // Should not happen
            if (_curAction == null)
                return;

            // Execute the action
            ActionState actionState = _curAction.Execute();

            switch (actionState)
            {
                default:
                case ActionState.Performed:
                    break;
                case ActionState.Accomplished:
                    OnActionAccomplished();
                    break;
                case ActionState.Compromised:
                    OnActionCompromised();
                    break;
            }
        }
    }
}