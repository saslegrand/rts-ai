using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace RTS.Waypoint
{
    public enum EWaypointType
    {
        Road,
        PassEntry,
        ResourceMine,
        Factory,
    }

    public class Waypoint : MonoBehaviour, IHeapItem<Waypoint>
    {
        public EWaypointType Type;
        private ETeam _mostInfluentialTeam;
        
        public List<Waypoint> LinkedWaypoints = new ();

        public float GCost { get; set; }
        public float HCost { get; set; }
        public float FCost => GCost + HCost;

        [HideInInspector] public Waypoint Parent;

        [HideInInspector] public Color color = Color.white;

        public int HeapIndex { get; set; }

        public int CompareTo(Waypoint other)
        {
            int compare = FCost.CompareTo(other.FCost);

            if (compare == 0)
                compare = HCost.CompareTo(other.HCost);

            return -compare;
        }
        
#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (Application.isPlaying) return;

            Vector3 position = transform.position;
            Handles.color = color;
            Handles.SphereHandleCap(0, position, Quaternion.identity, 10f, EventType.Repaint);
            foreach (Waypoint waypoint in LinkedWaypoints)
            {
                Gizmos.DrawLine(position, waypoint.transform.position);
            }
        }
#endif
        public void BreakLinks()
        {
            foreach (Waypoint waypoint in LinkedWaypoints)
            {
                waypoint.LinkedWaypoints.Remove(this);
            }
            
            LinkedWaypoints.Clear();
        }
    }

}
