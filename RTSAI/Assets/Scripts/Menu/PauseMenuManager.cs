using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RTS.Menu
{
    public class PauseMenuManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _buttonPanel;
        [SerializeField] private GameObject _controlsPanel;
        
        [Header("Option buttons")]
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _leaveButton;

        [Header("Controls")]
        [SerializeField] private Button _controlsButton;
        [SerializeField] private Button _exitControlsButton;

        private void Start()
        {
            _pauseButton.onClick.AddListener(OnPauseButtonClicked);
            _continueButton.onClick.AddListener(OnContinueButtonClicked);
            _retryButton.onClick.AddListener(OnRetryButtonClicked);
            _leaveButton.onClick.AddListener(OnLeaveButtonClicked);
            
            _controlsButton.onClick.AddListener(OnControlsClicked);
            _exitControlsButton.onClick.AddListener(OnExitControlsClicked);
        }

        void OnControlsClicked()
        {
            _buttonPanel.SetActive(false);
            _controlsPanel.SetActive(true);   
        }

        void OnExitControlsClicked()
        {
            _buttonPanel.SetActive(true);
            _controlsPanel.SetActive(false);    
        }
    
        void OnPauseButtonClicked()
        {
            if (GameServices.GetGameState().IsGameOver)
                return;
            
            _pauseButton.gameObject.SetActive(false);
            _buttonPanel.SetActive(true);
        }
    
        void OnContinueButtonClicked()
        {
            _pauseButton.gameObject.SetActive(true);
            _buttonPanel.SetActive(false);
        }
        
        void OnRetryButtonClicked()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
        void OnLeaveButtonClicked()
        {
            SceneManager.LoadScene("Scene_MainMenu");
        }
    }
}