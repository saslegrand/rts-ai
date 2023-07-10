using UnityEngine;

namespace RTS
{
    [CreateAssetMenu(fileName = "Unit_Data", menuName = "RTS/UnitData", order = 0)]
    public class UnitDataScriptable : EntityDataScriptable
    {
        [Header("Combat")]
        public int DPS = 10;
        public float AttackFrequency = 1f;
        public float AttackDistanceMax = 10f;
        public float CaptureDistanceMax = 10f;
        public float CaptureDistanceRange = 20f;

        [Header("Repairing")]
        public bool CanRepair;
        public int RPS = 10;
        public float RepairFrequency = 1f;
        public float RepairDistanceMax = 10f;
        public float RepairDistanceRange = 20f;

        [Header("Movement")]
        [Tooltip("Overrides NavMeshAgent steering settings")]
        public float Speed = 10f;
        public float AngularSpeed = 200f;
        public float Acceleration = 20f;
        public bool IsFlying;
        public int FormationOrder;
        public float FormationRadius = 2.0f;

        [Header("FX")]
        public GameObject BulletPrefab;
        public GameObject DeathFXPrefab;
        
        [Header("Power")] 
    	public int Influence = 5;
    	public int InfluenceRadius = 5;
    }
}
