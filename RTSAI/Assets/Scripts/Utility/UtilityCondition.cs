using UnityEngine;

namespace RTS.AI.Tools
{
    [CreateAssetMenu(fileName = "Utility Condition", menuName = "RTS/Tools/Utility/Utility Condition")]
    public class UtilityCondition : ScriptableObject
    {
        [SerializeField] private UtilityBlackboard _blackboard;
        
        [SerializeField] private string ValueName = "";
        [SerializeField] private AnimationCurve Curve = new AnimationCurve();
        [SerializeField] private float CurveMultiplier = 1f;

        private UtilityBlackboardValue _blackboardValue;

        public UtilityBlackboard Blackboard => _blackboard;

        public void Initialize()
        {
            _blackboardValue = _blackboard.GetBlackboardValueByName(ValueName);
        }

        public float Evaluate()
        {
            if (!_blackboard)
                return 0f;

            return Curve.Evaluate(_blackboardValue.Value) * CurveMultiplier;
        }
    }
}