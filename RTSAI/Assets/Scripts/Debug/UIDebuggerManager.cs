using RTS.AI.Debugger;
using UnityEngine;
using UnityEngine.UI;

public class UIDebuggerManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private Button _toggleButton;

    [Header("Panels")] 
    [SerializeField] private UIDebugger[] _debuggers;

    private bool _showDebug;
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (UIDebugger debugger in _debuggers)
            debugger.gameObject.SetActive(false);
        
        _toggleButton.onClick.AddListener(SwitchPanel);
    }

    private void Update()
    {
        if (!_showDebug)
            return;
        
        foreach (UIDebugger debugger in _debuggers)
            debugger.UpdateUI();
    }

    private void SwitchPanel()
    {
        _showDebug = !_showDebug;
        
        foreach (UIDebugger debugger in _debuggers)
            debugger.gameObject.SetActive(_showDebug);
    }
}
