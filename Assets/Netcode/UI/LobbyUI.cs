using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Netcode.UI
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text lobbyNameLabel;
        [SerializeField] private TMP_Text lobbyCodeLabel;
        [SerializeField] private Button leaveLobbyButton;
        [SerializeField] private Button startGameButton;

        [SerializeField] private Transform playersSection;
        [SerializeField] private GameObject playerUIPrefab;

        [SerializeField] private MainMenuUI mainMenuUI;
        [SerializeField] private LobbyManager lobbyManager;


        private void OnEnable()
        {
            leaveLobbyButton.onClick.AddListener(OnLeaveLobby);
            startGameButton.onClick.AddListener(OnStartGame);
            
        }

        private void OnDisable()
        {
            leaveLobbyButton.onClick.RemoveAllListeners();
            leaveLobbyButton.onClick.RemoveAllListeners();
        }

        private void OnLeaveLobby()
        {
            lobbyManager.LeaveLobby();
        }

        private void OnStartGame()
        {
            
        }
    }
}
