using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.UI
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text lobbyCodeLabel;
        [SerializeField] private Button leaveLobbyButton;
        [SerializeField] private Button startGameButton;

        [SerializeField] private MainMenuUI mainMenuUI;

        [SerializeField] private TMP_Text firstPlayerLabel;
        [SerializeField] private TMP_Text secondPlayerLabel;

        

        private void OnEnable()
        {
            leaveLobbyButton.onClick.AddListener(OnLeaveLobbyPressed);
            startGameButton.onClick.AddListener(OnStartGamePressed);
            startGameButton.interactable = LobbyManager.Instance.AmHost;
            var code = LobbyManager.Instance.IsLobbyPrivate
                ? LobbyManager.Instance.LobbyCode
                : LobbyManager.Instance.LobbyId;
            lobbyCodeLabel.text = code;
            firstPlayerLabel.text = LobbyManager.Instance.HostPlayerName;
            secondPlayerLabel.text = LobbyManager.Instance.GuestPlayerName ?? "";
            LobbyManager.GuestJoined += GuestJoined;
            LobbyManager.GuestLeft += GuestLeft;
            LobbyManager.LeftLobby += Removed;
            GameManager.GameStarted += OnGameStarted;
            
            GameManager.Initialize();
        }

        private void OnDisable()
        {
            leaveLobbyButton.onClick.RemoveAllListeners();
            leaveLobbyButton.onClick.RemoveAllListeners();
            LobbyManager.GuestJoined -= GuestJoined;
            LobbyManager.GuestLeft -= GuestLeft;
            LobbyManager.LeftLobby -= Removed;
            GameManager.GameStarted -= OnGameStarted;
        }

        private void GuestJoined(string playerName)
        {
            secondPlayerLabel.text = playerName;
        }

        private void GuestLeft()
        {
            secondPlayerLabel.text = "";
        }

        private void Removed()
        {
            mainMenuUI.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }

        private void OnLeaveLobbyPressed()
        {
            LobbyManager.Instance.LeaveLobby();
        }

        private void OnStartGamePressed()
        {
            GameManager.StartGame();
        }

        private void OnGameStarted()
        {
            gameObject.SetActive(false);
        }
    }
}
