using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RTS
{
    public class GameFlowUI : MonoBehaviour
    {
        [SerializeField] private GameObject _gameOverContainer;
        [SerializeField] private TMP_Text _winnerText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _leaveButton;
        
        void Start()
        {
            _gameOverContainer.SetActive(false);
            
            _retryButton.onClick.AddListener(OnRetryButtonClicked);
            _leaveButton.onClick.AddListener(OnLeaveButtonClicked);

            GameServices.GetGameState().OnGameOver += ShowGameResults;
        }
        
        void ShowGameResults(ETeam winner)
        {
            _gameOverContainer.SetActive(true);
            _winnerText.color = GameServices.GetTeamColor(winner);
            _winnerText.text = "Winner is " + winner + " team";
        }

        private void OnRetryButtonClicked()
        {
            if (!GameServices.GetGameState().IsGameOver)
                return;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnLeaveButtonClicked()
        {
            SceneManager.LoadScene("Scene_MainMenu");
        }
    }
}