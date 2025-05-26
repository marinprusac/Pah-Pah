using Menu.Managers;
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
            startGameButton.interactable = false;
            var code = LobbyManager.Instance.IsLobbyPrivate
                ? LobbyManager.Instance.LobbyCode
                : LobbyManager.Instance.LobbyId;
            lobbyCodeLabel.text = code;
            firstPlayerLabel.text = LobbyManager.Instance.HostPlayerName;
            secondPlayerLabel.text = LobbyManager.Instance.GuestPlayerName ?? "";
            LobbyManager.GuestJoined += GuestJoined;
            LobbyManager.GuestLeft += GuestLeft;
            LobbyManager.LeftLobby += Removed;
            RelayManager.AllReady += OnGameStarted;
            
            RelayManager.Initialize();
        }

        private void OnDisable()
        {
            leaveLobbyButton.onClick.RemoveAllListeners();
            leaveLobbyButton.onClick.RemoveAllListeners();
            LobbyManager.GuestJoined -= GuestJoined;
            LobbyManager.GuestLeft -= GuestLeft;
            LobbyManager.LeftLobby -= Removed;
            RelayManager.AllReady -= OnGameStarted;
            
            RelayManager.Uninitialize();
            LobbyManager.Uninitialize();
        }

        private void GuestJoined(string playerName)
        {
            secondPlayerLabel.text = playerName;
            if (LobbyManager.Instance.AmHost) startGameButton.interactable = true;

        }

        private void GuestLeft()
        {
            secondPlayerLabel.text = "";
            if (LobbyManager.Instance.AmHost) startGameButton.interactable = false;
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
            RelayManager.StartRelay();
        }

        private void OnGameStarted()
        {
            gameObject.SetActive(false);
        }
    }
}
