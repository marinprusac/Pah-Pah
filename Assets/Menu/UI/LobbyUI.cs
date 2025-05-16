using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Netcode.UI
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
            leaveLobbyButton.onClick.AddListener(OnLeaveLobby);
            startGameButton.onClick.AddListener(OnStartGame);
            startGameButton.interactable = LobbyManager.Instance.AmHost;
            var code = LobbyManager.Instance.CodeOrId;
            lobbyCodeLabel.text = code;
            firstPlayerLabel.text = LobbyManager.Instance.HostPlayerName;
            secondPlayerLabel.text = LobbyManager.Instance.GuestPlayerName ?? "";
            LobbyManager.Instance.PlayerJoined += GuestJoined;
            LobbyManager.Instance.PlayerLeft += GuestLeft;
            LobbyManager.Instance.RemovedFromLobby += Removed;
            LobbyManager.Instance.GameStarted += Started;
        }

        private void OnDisable()
        {
            leaveLobbyButton.onClick.RemoveAllListeners();
            leaveLobbyButton.onClick.RemoveAllListeners();
            LobbyManager.Instance.PlayerJoined -= GuestJoined;
            LobbyManager.Instance.PlayerLeft -= GuestLeft;
            LobbyManager.Instance.RemovedFromLobby -= Removed;
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

        private void OnLeaveLobby()
        {
            LobbyManager.Instance.LeaveLobby();
        }

        private void OnStartGame()
        {
            LobbyManager.Instance.StartGame();
        }

        private void Started()
        {
            gameObject.SetActive(false);
        }
    }
}
