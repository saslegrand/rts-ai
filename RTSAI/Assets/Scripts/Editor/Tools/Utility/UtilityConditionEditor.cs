using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RTS.AI.Tools
{
    [CustomEditor(typeof(UtilityCondition))]
    public class UtilityConditionEditor : Editor
    {
        private UtilityCondition _self;
        
        private SerializedProperty _nameProp;
        private SerializedProperty _curveProp;
        private SerializedProperty _curveMultProp;
        private SerializedProperty _blackboardProp;

        private GUIContent[] _values;

        private int _popupNameIndex;

        private GUIContent[] GetBlackboardValues(UtilityBlackboard blackboard)
        {
            List<UtilityBlackboardValue> blackboardValues =
                new List<UtilityBlackboardValue>(blackboard.Values.ToArray());
            
            blackboardValues.Sort((lhs, rhs) => String.Compare(lhs.Name, rhs.Name, StringComparison.Ordinal));

            string lastValueName = "";
            List<GUIContent> valueNames = new List<GUIContent>();
            for (int i = 0; i < blackboardValues.Count; i++)
            {
                UtilityBlackboardValue value = blackboardValues[i];
                
                if (!string.IsNullOrEmpty(value.Name) && value.Name != lastValueName)
                {
                    lastValueName = value.Name;
                    valueNames.Add(new GUIContent(value.Name));
                }
            }

            return valueNames.ToArray();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(_blackboardProp);
            serializedObject.ApplyModifiedProperties();
            
            UtilityBlackboard blackboard = _self.Blackboard;
            if (blackboard == null)
                return;

            _values = GetBlackboardValues(blackboard);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.PrefixLabel(_nameProp.stringValue);
            int newPopupIndex = EditorGUILayout.Popup(_popupNameIndex, _values);

            if (newPopupIndex != _popupNameIndex)
            {
                _popupNameIndex = newPopupIndex;
                _nameProp.stringValue = _values[_popupNameIndex].text;
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.PropertyField(_curveProp);
            EditorGUILayout.PropertyField(_curveMultProp);

            EditorGUILayout.EndVertical();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void OnEnable()
        {
            _self = (UtilityCondition)target;

            _blackboardProp = serializedObject.FindProperty("_blackboard");
            _nameProp = serializedObject.FindProperty("ValueName");
            _curveProp = serializedObject.FindProperty("Curve");
            _curveMultProp = serializedObject.FindProperty("CurveMultiplier");

            _popupNameIndex = 0;

            if (_self.Blackboard)
            {
                _values = GetBlackboardValues(_self.Blackboard);
                for (int index = 0; index < _values.Length; index++)
                {
                    if (_nameProp.stringValue == _values[index].text)
                    {
                        _popupNameIndex = index;
                        break;
                    }
                }

                _nameProp.stringValue = _values.Length != 0f ? _values[_popupNameIndex].text : "Error";
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}