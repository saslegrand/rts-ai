using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RTS.AI.Tools
{
    [CustomEditor(typeof(UtilityBlackboard))]

    public class UtilityBlackboardEditor : Editor
    {
        private UtilityBlackboard _blackboard;
        private SerializedProperty _valuesProp;
        private List<UtilityBlackboardValue> _values;
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.LabelField("Values");
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            List<string> valueNames = new List<string>();
            for (int i = 0; i < _valuesProp.arraySize; i++)
            {
                SerializedProperty blackboardValProp = _valuesProp.GetArrayElementAtIndex(i);
                SerializedProperty nameProp = blackboardValProp.FindPropertyRelative("Name");
                
                valueNames.Add(nameProp.stringValue);
                nameProp.stringValue = EditorGUILayout.TextField(nameProp.stringValue);
            }
            
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add value"))
            {
                int valueIndex = 1;
                string newValueName = "Value";
                while (valueNames.Contains(newValueName))
                {
                    newValueName = "Value" + valueIndex;
                    valueIndex++;
                }
                
                _blackboard.AddValue(newValueName);
            }

            if (GUILayout.Button("Remove last value") && valueNames.Count > 0)
            {
                _blackboard.RemoveValue(valueNames[^1]);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
            
            int unnamedValuesCount = 0;
            List<string> equalNames = new List<string>();
            for (int i = 0; i < valueNames.Count; i++)
            {
                bool isEmpty = string.IsNullOrEmpty(valueNames[i]);
                if (isEmpty)
                {
                    unnamedValuesCount++;
                    continue;
                }

                if (i == valueNames.Count - 1)
                    break;

                for (int j = i + 1; j < valueNames.Count; j++)
                {
                    if (valueNames[i] == valueNames[j])
                    {
                        if (!equalNames.Contains(valueNames[i]))
                            equalNames.Add(valueNames[i]);
                    }
                }
            }
            
            // Draw help boxes
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (unnamedValuesCount == 1)
                EditorGUILayout.HelpBox(new GUIContent($"{unnamedValuesCount} value is not named, utility behavior can be compromised"));
            if (unnamedValuesCount > 1)
                EditorGUILayout.HelpBox(new GUIContent($"{unnamedValuesCount} values are not named, utility behavior can be compromised"));


            if (equalNames.Count > 0)
            {
                string equalNamesStr = "{";
                for (int valIndex = 0; valIndex < equalNames.Count; valIndex++)
                {
                    equalNamesStr += equalNames[valIndex];
                    equalNamesStr += (valIndex == equalNames.Count - 1) ? "}" : ", ";
                }
                EditorGUILayout.HelpBox(new GUIContent($"{equalNamesStr} are defined multiple times, utility behavior can be compromised"));
            }
            
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _blackboard = (UtilityBlackboard)target;
            _valuesProp = serializedObject.FindProperty("_values");
        }
    }
}