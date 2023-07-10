using System.Collections.Generic;
using UnityEngine;

namespace RTS.AI.Tools
{
    [System.Serializable]
    public class UtilityBlackboardValue
    {
        public string Name;
        public float Value;
    }
    
    [CreateAssetMenu(fileName = "Utility Blackboard", menuName = "RTS/Tools/Utility/Utility Blackboard")]
    public class UtilityBlackboard : ScriptableObject
    {
        [SerializeField] private List<UtilityBlackboardValue> _values = new List<UtilityBlackboardValue>();

        public List<UtilityBlackboardValue> Values => _values;

        public void AddValue(string valueName, float defaultValue = 0f)
        {
            UtilityBlackboardValue val = _values.Find(value => value.Name == valueName);
            
            if (val == null)
            {
                _values.Add(new UtilityBlackboardValue()
                {
                    Name = valueName,
                    Value = defaultValue
                });
                
                return;
            }
            
            Debug.LogWarning($"Blackboard - The key \"{valueName}\" you tried to add is already registered");
        }

        public void RemoveValue(string valueName)
        {
            UtilityBlackboardValue val = _values.Find(value => value.Name == valueName);
            
            if (val != null)
            {
                _values.Remove(val);
                return;
            }
            
            Debug.LogWarning($"Blackboard - The key \"{valueName}\" you tried to remove is not registered");
        }
        
        public UtilityBlackboardValue GetBlackboardValueByName(string blackboardValueName)
        {
            UtilityBlackboardValue val = _values.Find(value => value.Name == blackboardValueName);

            if (val == null)
                Debug.LogWarning($"Blackboard - Impossible to get the blackboard value \"{blackboardValueName}\", not registered");

            return val;
        }

        public float GetValueByName(string valueName)
        {
            UtilityBlackboardValue val = _values.Find(value => value.Name == valueName);

            if (val == null)
            {
                Debug.LogWarning($"Blackboard - Impossible to get the key \"{valueName}\", not registered");
                return 0f;
            }

            return val.Value;
        }
        
        public void SetValueByName(string valueName, float value)
        {
            UtilityBlackboardValue val = _values.Find(v => v.Name == valueName);
            
            if (val == null)
            {
                Debug.LogWarning($"Blackboard - Impossible to set the key \"{valueName}\", not registered");
                return;
            }
            
            val.Value = value;
        }
    }
}