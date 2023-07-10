using RTS;
using RTS.AI.Tools;
using UnityEngine;

public class RegisterTemp : MonoBehaviour
{
    private InfluenceMap _map;
    private Unit _unit;
    
    // Start is called before the first frame update
    void Start()
    {
        _unit = GetComponent<Unit>();

        _map = FindObjectOfType<InfluenceMap>();
        _map.AddInfluenceSource(_unit);
    }

    private void OnDestroy()
    {
        _map.RemoveInfluenceSource(_unit);
    }
}
