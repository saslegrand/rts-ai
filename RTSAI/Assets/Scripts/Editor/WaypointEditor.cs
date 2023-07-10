using UnityEditor;
using UnityEngine;


namespace RTS.Waypoint
{

	[CustomEditor(typeof(Waypoint))]
	public class WaypointEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			if (target is not Waypoint waypoint) return;

			serializedObject.Update();

			waypoint.Type = (EWaypointType)EditorGUILayout.EnumPopup(waypoint.Type);

			_showWaypointList = EditorGUILayout.Foldout(_showWaypointList, "Linked Waypoints", true, EditorStyles.foldoutHeader);
			if (_showWaypointList)
			{
				using (new EditorGUI.DisabledScope(true))
					foreach (Waypoint linkedWaypoint in waypoint.LinkedWaypoints)
						EditorGUILayout.ObjectField("", linkedWaypoint, typeof(Waypoint), true);
			}

			if (GUILayout.Button("Break Links"))
				waypoint.BreakLinks();
            
			EditorUtility.SetDirty(waypoint);
            
			serializedObject.ApplyModifiedProperties();
		}

		private bool _showWaypointList;
	}

}
