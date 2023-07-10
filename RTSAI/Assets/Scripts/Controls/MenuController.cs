using System;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace RTS.UI
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private Transform _factoryMenuCanvas;
        [SerializeField] private SelectedFactoryMenu _selectedFactoryMenu;
        [HideInInspector] public TeamController Controller;

        [SerializeField] private TMP_Text _buildPointsText;
        [SerializeField] private TMP_Text _capturedTargetsText;
        //private Button _cancelBuildButton = null;
        //private Text[] _buildQueueTexts = null;
        
        public GraphicRaycaster BuildMenuRaycaster { get; private set; }

        public void HideFactoryMenu()
        {
            if (_selectedFactoryMenu)
                _selectedFactoryMenu.gameObject.SetActive(false);
        }

        private void ShowFactoryMenu()
        {
            if (_selectedFactoryMenu)
                _selectedFactoryMenu.gameObject.SetActive(true);
        }
        
        public void UpdateBuildPointsUI()
        {
            if (_buildPointsText)
                _buildPointsText.text = Controller.TotalBuildPoints.ToString();
        }
        public void UpdateCapturedTargetsUI()
        {
            if (_capturedTargetsText)
                _capturedTargetsText.text = Controller.CapturedTargets.ToString();
        }
        public void UnregisterBuildButtons(int availableUnitsCount, int availableFactoriesCount)
        {
            _selectedFactoryMenu.UnregisterBuildButtons(availableUnitsCount, availableFactoriesCount);
        }

        public void UpdateFactoryMenu(Factory selectedFactory, Func<int, bool> requestUnitBuildMethod, Action<int> enterFactoryBuildModeMethod)
        {
            ShowFactoryMenu();
            _selectedFactoryMenu.UpdateFactoryMenu(selectedFactory, requestUnitBuildMethod, enterFactoryBuildModeMethod);
        }

        void Awake()
        {
            _factoryMenuCanvas = transform;
            
            BuildMenuRaycaster = _factoryMenuCanvas.GetComponent<GraphicRaycaster>();
        }

        private void Start()
        {
            _selectedFactoryMenu.gameObject.SetActive(false);
            UpdateBuildPointsUI();
            UpdateCapturedTargetsUI();
        }
    }
}