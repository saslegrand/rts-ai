using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace RTS.Waypoint
{
	public class WaypointGraph : Singleton<WaypointGraph>
	{
		public Waypoint[] Waypoints;

		private Waypoint _selectedWaypoint;
		public Waypoint SelectedWaypoint
		{
			get => _selectedWaypoint;

			set
			{
				if (_selectedWaypoint)
					_selectedWaypoint.color = Color.white;
				
				_selectedWaypoint = value;

				if (!_selectedWaypoint) return;

				_selectedWaypoint.color = Color.green;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			RegisterWaypoints();
		}

		public Waypoint GetClosestOfType(Vector3 position, EWaypointType type)
		{
			Waypoint closest = null;
			float distance = float.MaxValue;

			foreach (Waypoint waypoint in Waypoints)
			{
				if (waypoint.Type != type) continue;

				float newDistance = Vector3.Distance(waypoint.transform.position, position);
				
				if (newDistance >= distance) continue;

				distance = newDistance;
				closest = waypoint;
			}

			return closest;
		}
		
		public Waypoint GetFurthestOfType(Vector3 position, EWaypointType type)
		{
			Waypoint closest = null;
			float distance = -float.MaxValue;

			foreach (Waypoint waypoint in Waypoints)
			{
				if (waypoint.Type != type) continue;

				float newDistance = Vector3.Distance(waypoint.transform.position, position);
				
				if (newDistance <= distance) continue;

				distance = newDistance;
				closest = waypoint;
			}

			return closest;
		}
		
		public Waypoint GetClosestWaypointToWorldPosition(Vector3 position)
		{
			Waypoint closest = null;
			float distance = float.MaxValue;
			
			foreach (Waypoint waypoint in Waypoints)
			{
				float newDistance = Vector3.Distance(waypoint.transform.position, position);

				if (newDistance >= distance) continue;

				distance = newDistance;
				closest = waypoint;
			}

			return closest;
		}
		
		public Waypoint[] GetPath(Vector3 startPosition, Vector3 endPosition)
		{
			Waypoint startNode = GetClosestWaypointToWorldPosition(startPosition);
			Waypoint endNode = GetClosestWaypointToWorldPosition(endPosition);

			Heap<Waypoint> openSet = new (Waypoints.Length);
			//List<Waypoint> openSet = new ();
			HashSet<Waypoint> closedSet = new ();
			openSet.Add(startNode);

			while (openSet.Count > 0)
			{
				//Waypoint currentNode = openSet[0];
				//
				//foreach (Waypoint t in openSet.Where(t => t.FCost < currentNode.FCost || t.FCost == currentNode.FCost && t.HCost < currentNode.HCost))
				//{
				//	currentNode = t;
				//}
				//openSet.Remove(currentNode);

				Waypoint currentNode = openSet.RemoveFirst();
				closedSet.Add(currentNode);

				if (currentNode == endNode)
					return RetracePath(startNode, endNode);

				Waypoint[] neighbours = currentNode.LinkedWaypoints.ToArray();

				foreach (Waypoint neighbour in neighbours)
				{
					if (closedSet.Contains(neighbour)) continue;

					float newMovementCost = currentNode.GCost + GetDistance(currentNode, neighbour);

					if (!( newMovementCost < neighbour.GCost ) && openSet.Contains(neighbour)) continue;

					neighbour.GCost = newMovementCost;
					neighbour.HCost = GetDistance(neighbour, endNode);
					neighbour.Parent = currentNode;

					if (!openSet.Contains(neighbour))
						openSet.Add(neighbour);
					else
						openSet.UpdateItem(neighbour);
				}
			}

			return null;
		}

		private Waypoint[] RetracePath(Waypoint startNode, Waypoint endNode)
		{
			List<Waypoint> path = new ();
			Waypoint currentNode = endNode;

			while (currentNode != startNode)
			{
				path.Add(currentNode);
				currentNode = currentNode.Parent;
			}
			path.Add(currentNode);
			path.Reverse();

			return path.ToArray();
		}

		private float GetDistance(Waypoint currentNode, Waypoint neighbour)
		{
			return Vector3.Distance(currentNode.transform.position, neighbour.transform.position);
		}

		public void LinkWaypoints(Waypoint graphWaypoint)
		{
			if (!SelectedWaypoint) return;
			
			if (!SelectedWaypoint.LinkedWaypoints.Contains(graphWaypoint))
				SelectedWaypoint.LinkedWaypoints.Add(graphWaypoint);
			if (!graphWaypoint.LinkedWaypoints.Contains(SelectedWaypoint))
				graphWaypoint.LinkedWaypoints.Add(SelectedWaypoint);
			
#if UNITY_EDITOR 
			EditorUtility.SetDirty(SelectedWaypoint);
			EditorUtility.SetDirty(graphWaypoint);
#endif		
		}

		public void RegisterWaypoints()
		{
			Waypoints = GetComponentsInChildren<Waypoint>();
		}

		public void BreakLinks()
		{
			foreach (Waypoint waypoint in Waypoints)
			{
				waypoint.BreakLinks();
			}
		}

		public void ClearColor()
		{
			foreach (Waypoint waypoint in Waypoints)
			{
				waypoint.color = Color.white;
			}

			SelectedWaypoint = null;
		}
	}

}
