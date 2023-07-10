using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public static class CustomEditorExtension
{
    public static void DrawFloatField(SerializedProperty prop, string label, GUILayoutOption[] options = null)
    {
        prop.floatValue = EditorGUILayout.FloatField(label, prop.floatValue, options);
    }
    
    public static bool DrawFloatFieldInfo(SerializedProperty prop, string label, GUILayoutOption[] options = null)
    {
        float value = EditorGUILayout.FloatField(label, prop.floatValue, options);

        bool isUpdated = value != prop.floatValue;
        prop.floatValue = value;

        return isUpdated;
    }
}
#endif