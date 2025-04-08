using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Netcode.UI
{
    public class CreateLobbyUI : MonoBehaviour
    {


        [SerializeField] private TMP_InputField lobbyName;
        [SerializeField] private Toggle toggleVisibility;
        [SerializeField] private Button createLobbyButton;
        [SerializeField] private Button goBackButton;

        [SerializeField] private MainMenuUI mainMenuUI;
        [SerializeField] private LobbyUI lobbyUI;

        [SerializeField] private LobbyManager lobbyManager;


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
            lobbyManager.onJoinedLobby.AddListener(OnLobbyCreated);
            _ = lobbyManager.CreateLobby(lobbyName.text, toggleVisibility.isOn);
        }

        private void OnLobbyCreated()
        {
            lobbyManager.onJoinedLobby.RemoveListener(OnLobbyCreated);
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
