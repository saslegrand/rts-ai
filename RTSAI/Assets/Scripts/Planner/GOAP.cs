using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RTS.AI.Planner
{
    public class PlannerNode<Tagent, Tenum> where Tenum : Enum where Tagent : MonoBehaviour
    {
        public WorldState LinkedWorldState;
        public Actions.Action<Tagent, Tenum> LinkedAction;
        public PlannerNode<Tagent, Tenum> Parent;
        public float Cost;
        
        public static PlannerNode<Tagent, Tenum> CreateNode(PlannerNode<Tagent, Tenum> parent, WorldState newWorldState, Actions.Action<Tagent, Tenum> action)
        {
            return new PlannerNode<Tagent, Tenum>
            {
                LinkedWorldState = new WorldState(newWorldState),
                LinkedAction = action,
                Cost = parent.Cost + action.GetCost(newWorldState),
                Parent = parent
            };
        }
    }
    
    public static class Goap<Tagent, Tenum> where Tenum : Enum where Tagent : MonoBehaviour
    {
        private static bool IsGoalAchieved(WorldState worldState, List<Goal<Tenum>> goals)
        {
            return worldState.TestConditions(goals);
        }

        private static List<Actions.Action<Tagent, Tenum>> CreateSubActions(List<Actions.Action<Tagent, Tenum>> actions, Actions.Action<Tagent, Tenum> actionToRemove)
        {
            List<Actions.Action<Tagent, Tenum>> subActions = new(actions);
            subActions.Remove(actionToRemove);
            
            return subActions;
        }
        
        private static void BuildGraphForward(PlannerNode<Tagent, Tenum> parentNode, List<PlannerNode<Tagent, Tenum>> leaves, 
            List<Actions.Action<Tagent, Tenum>> actions, List<Goal<Tenum>> goals, ref float heuristic)
        {
            for (int actionIndex = 0; actionIndex < actions.Count; actionIndex++)
            {
                Actions.Action<Tagent, Tenum> action = actions[actionIndex];
                
                // Check if the action can be accomplished based on the agent world state
                if (actions[actionIndex].CheckPreconditions(parentNode.LinkedWorldState))
                {
                    WorldState newWorldState = action.ApplyEffects(parentNode.LinkedWorldState);
                    PlannerNode<Tagent, Tenum> newNode = PlannerNode<Tagent, Tenum>.CreateNode(parentNode, newWorldState, action);

                    // Dont continue with this action if another one is already better
                    if (newNode.Cost > heuristic)
                        continue;

                    if (IsGoalAchieved(newWorldState, goals))
                    {
                        heuristic = newNode.Cost;
                        
                        // Add action
                        leaves.Add(newNode);
                    }
                    else
                    {
                        // Recursively find new action with updated world state
                        List<Actions.Action<Tagent, Tenum>> subActions = CreateSubActions(actions, action);
                        BuildGraphForward(newNode, leaves, subActions, goals, ref heuristic);
                    }
                }
            }
        }

        private static PlannerNode<Tagent, Tenum> FindBestNode(List<PlannerNode<Tagent, Tenum>> leaves)
        {
            // No node find, no plan can be executed
            if (leaves.Count == 0)
                return null;

            // Sort nodes
            leaves.Sort((lhs, rhs) => lhs.Cost.CompareTo(rhs.Cost));

            float lowerCost = leaves[0].Cost;
            List<PlannerNode<Tagent, Tenum>> bestNodes = new List<PlannerNode<Tagent, Tenum>>();

            // Find the best nodes in the potential nodes (lower cost)
            int nodeIndex = 0;
            while (nodeIndex <= leaves.Count - 1 && leaves[nodeIndex].Cost == lowerCost)
            {
                bestNodes.Add(leaves[nodeIndex]);
                nodeIndex++;
            }

            // Return a random node from the best nodes selection
            return bestNodes[Random.Range(0, bestNodes.Count)];
        }

        private static Queue<Actions.Action<Tagent, Tenum>> NodeToPlan(PlannerNode<Tagent, Tenum> node)
        {
            if (node == null)
                return null;
            
            List<Actions.Action<Tagent, Tenum>> actions = new List<Actions.Action<Tagent, Tenum>>();

            // Get all actions linked to the node
            PlannerNode<Tagent, Tenum> curNode = node;
            while (curNode.Parent != null)
            {
                actions.Add(curNode.LinkedAction);
                curNode = curNode.Parent;
            }

            // Reverse actions to get them from start to end
            actions.Reverse();
            
            // Create the plan queue
            return new Queue<Actions.Action<Tagent, Tenum>>(actions);
        }
        
        public static Queue<Actions.Action<Tagent, Tenum>> FindPlan(List<Actions.Action<Tagent, Tenum>> actions, WorldState initialWorldState, List<Goal<Tenum>> goals)
        {
            if (IsGoalAchieved(initialWorldState, goals))
            {
                Debug.LogWarning("The goals you are trying to achieve have already been achieved. Please consider find new goals");
                return null;
            }
            
            
            // All the potential node that we can use
            List<PlannerNode<Tagent, Tenum>> leaves = new List<PlannerNode<Tagent, Tenum>>();
            PlannerNode<Tagent, Tenum> node = new() { LinkedWorldState = initialWorldState };
            
            // Get all the nodes that can fulfill the goal
            float heuristic = float.MaxValue;
            BuildGraphForward(node, leaves, actions, goals, ref heuristic);

            // Find the best node in the leaves node and create the associated plan
            // Return null if no leaves
            PlannerNode<Tagent, Tenum> bestNode = FindBestNode(leaves);
            return NodeToPlan(bestNode);
        }
    }
}