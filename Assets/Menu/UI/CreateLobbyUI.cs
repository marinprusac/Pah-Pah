using Managers;
using Menu.Managers;
using Netcode.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.UI
{
    public class CreateLobbyUI : MonoBehaviour
    {


        [SerializeField] private Toggle toggleVisibility;
        [SerializeField] private Button createLobbyButton;
        [SerializeField] private Button goBackButton;
        [SerializeField] private TMP_InputField nameInput;

        [SerializeField] private MainMenuUI mainMenuUI;
        [SerializeField] private LobbyUI lobbyUI;

        private void OnEnable()
        {
            createLobbyButton.onClick.AddListener(OnCreateLobby);
            goBackButton.onClick.AddListener(OnGoBack);
            LobbyManager.JoinedLobby += OnLobbyCreated;
        }

        private void OnDisable()
        {
            createLobbyButton.onClick.RemoveAllListeners();
            goBackButton.onClick.RemoveAllListeners();
            LobbyManager.JoinedLobby -= OnLobbyCreated;
        }

        private void OnCreateLobby()
        {
            _ = LobbyManager.CreateLobby(toggleVisibility.isOn, nameInput.text);
        }

        private void OnLobbyCreated()
        {
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
