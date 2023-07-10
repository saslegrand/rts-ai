using RTS;
using UnityEditor;

[CustomEditor(typeof(GameServices))]
public class GameServicesEditor : Editor
{
    private SerializedProperty _timeScaleProp;
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (CustomEditorExtension.DrawFloatFieldInfo(_timeScaleProp, "Time scale"))
            GameServices.SetTimeScale(_timeScaleProp.floatValue);
        
        serializedObject.ApplyModifiedProperties();
    }

    private void OnEnable()
    {
        _timeScaleProp = serializedObject.FindProperty("_timeScale");
    }
}
