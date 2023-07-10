using UnityEngine;

namespace RTS
{
    public class EntityDataScriptable : ScriptableObject
    {
        [Header("Build Data")]
        public int TypeId;
        public string Caption = "Unknown Unit";
        public int Cost = 1;
        public float BuildDuration = 1f;
        public Sprite Thumbnail;
        
        [Header("Health Points")]
        public int MaxHP = 100;
    }
}