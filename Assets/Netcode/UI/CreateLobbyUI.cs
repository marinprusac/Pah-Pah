using UnityEngine;
using UnityEngine.UI;

namespace Netcode.UI
{
    public class CreateLobbyUI : MonoBehaviour
    {


        [SerializeField] private Toggle toggleVisibility;
        [SerializeField] private Button createLobbyButton;
        [SerializeField] private Button goBackButton;

        [SerializeField] private MainMenuUI mainMenuUI;
        [SerializeField] private LobbyUI lobbyUI;

        private void OnEnable()
        {
            createLobbyButton.onClick.AddListener(OnCreateLobby);
            goBackButton.onClick.AddListener(OnGoBack);
        }

        private void OnDisable()
        {
            createLobbyButton.onClick.RemoveAllListeners();
            goBackButton.onClick.RemoveAllListeners();
        }

        private void OnCreateLobby()
        {
            LobbyManager.Instance.JoinedLobby += OnLobbyCreated;
            _ = LobbyManager.Instance.CreateLobby(toggleVisibility.isOn);
        }

        private void OnLobbyCreated()
        {
            LobbyManager.Instance.JoinedLobby -= OnLobbyCreated;
            lobbyUI.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }

        private void OnGoBack()
        {
            mainMenuUI.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
