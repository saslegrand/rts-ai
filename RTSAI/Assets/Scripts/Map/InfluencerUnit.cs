using RTS.AI.Tools;
using UnityEngine;

namespace RTS.AI
{
    [RequireComponent(typeof(Unit))]
    public class InfluencerUnit : MonoBehaviour
    {
        private Unit _selfUnit;

        private void Awake()
        {
            _selfUnit = GetComponent<Unit>();
        }

        private void Start()
        {
            InfluenceMap.Instance.AddInfluenceSource(_selfUnit);
        }

        private void OnDestroy()
        {
            InfluenceMap.Instance.RemoveInfluenceSource(_selfUnit);
        }
    }   
}