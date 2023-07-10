using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RTS.Menu
{
    public class MenuManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private GameObject _controlsPanel;
        [Header("Battlefields")]
        [SerializeField] private Button _playBattlefieldButton;
        [SerializeField] private Button _playLargeBattlefieldButton;
        [Header("Controls")]
        [SerializeField] private Button _controlsButton;
        [SerializeField] private Button _backControlsButton;
        [Header("Quit")]
        [SerializeField] private Button _quitButton;

        private void Start()
        {
            _playBattlefieldButton.onClick.AddListener(OnPlayBattlefieldClicked);
            _playLargeBattlefieldButton.onClick.AddListener(OnPlayLargeBattlefieldClicked);
            _quitButton.onClick.AddListener(OnQuitClicked);
            
            _controlsButton.onClick.AddListener(OnControlsClicked);
            _backControlsButton.onClick.AddListener(OnExitControlsClicked);
        }

        private void OnControlsClicked()
        {
            _mainMenuPanel.SetActive(false);
            _controlsPanel.SetActive(true);
        }

        private void OnExitControlsClicked()
        {
            _mainMenuPanel.SetActive(true);
            _controlsPanel.SetActive(false);
        }

        void OnQuitClicked()
        {
            Application.Quit();
        }
    
        void OnPlayBattlefieldClicked()
        {
            SceneManager.LoadScene("New BattleField");
        }
    
        void OnPlayLargeBattlefieldClicked()
        {
            SceneManager.LoadScene("New LargeBattleField");
        }
    }
}