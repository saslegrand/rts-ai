using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


namespace RTS.Waypoint
{

	[CustomEditor(typeof(WaypointGraph))]
	public class WaypointGraphEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			serializedObject.Update();
			
			EditorGUI.BeginChangeCheck();

			if (target is not WaypointGraph graph) return;

			if (GUILayout.Button("Register Waypoints"))
				graph.RegisterWaypoints();
			
			if (GUILayout.Button("Break All Links"))
				graph.BreakLinks();
			
			if (GUILayout.Button("Clean White"))
				graph.ClearColor();

			if (EditorGUI.EndChangeCheck())
				EditorUtility.SetDirty(graph);

			serializedObject.ApplyModifiedProperties();
		}

		private void OnSceneGUI()
		{
			if (Application.isPlaying) return;
			
			if (target is not WaypointGraph graph) return;
			Event gui_Event = Event.current;
			EditorGUI.BeginChangeCheck();

			foreach (Waypoint waypoint in graph.Waypoints)
			{
				int controlID = GUIUtility.GetControlID(FocusType.Passive);

				Handles.color = waypoint.color;
				Handles.FreeMoveHandle(controlID, waypoint.transform.position, Quaternion.identity, 10f, Vector3.zero, Handles.SphereHandleCap);

				if (controlID != GUIUtility.hotControl) continue;

				bool isCurrentSelected = waypoint == graph.SelectedWaypoint;
				if (gui_Event.control)
				{
					if (!isCurrentSelected)
						graph.SelectedWaypoint = waypoint;
				}
				else if (gui_Event.shift)
				{
					if (isCurrentSelected)
						graph.SelectedWaypoint = null;
				}
				else
					graph.LinkWaypoints(waypoint);

				if (EditorGUI.EndChangeCheck())
					EditorUtility.SetDirty(graph);

				EditorSceneManager.MarkSceneDirty(graph.gameObject.scene);
			}
		}
	}

}
