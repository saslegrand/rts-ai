using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.AI.Tools
{
    public class AIGoalsVisualizer : MonoBehaviour
    {
        [SerializeField] private RectTransform _captureBar;
        [SerializeField] private RectTransform _defendBar;
        [SerializeField] private RectTransform _attackBar;

        [SerializeField] private ArmyLeader _armyLeader;

        private void FixedUpdate()
        {
            if (_armyLeader == null)
                return;

            UtilityAction[] goals = _armyLeader.OrderedGoals;

            if (goals == null)
                return;

            foreach (UtilityAction goal in goals)
            {
                switch (goal.Goal)
                {
                    case Planner.ArmyLeaderGoal.CaptureTarget:
                        ScaleBar(_captureBar, goal.Percent);
                        break;
                    case Planner.ArmyLeaderGoal.Defend:
                        ScaleBar(_defendBar, goal.Percent);
                        break;
                    case Planner.ArmyLeaderGoal.Attack:
                        ScaleBar(_attackBar, goal.Percent);
                        break;
                }
            }
        }

        public void ScaleBar(RectTransform bar, float percent)
        {
            bar.localScale = new Vector3(1, Mathf.Clamp01(percent), 1);
        }
    }
}