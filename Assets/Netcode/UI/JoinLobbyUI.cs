using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Netcode.UI
{
    public class JoinLobbyUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField lobbyCodeInput;
        [SerializeField] private Button joinPrivateLobbyButton;
        [SerializeField] private Button goBackButton;

        [SerializeField] private Button refreshButton;
        [SerializeField] private Transform publicLobbiesSection;

        [SerializeField] private GameObject publicLobbyUIPrefab;

        [SerializeField] private MainMenuUI mainMenuUI;
        [SerializeField] private LobbyUI lobbyUI;

        [SerializeField] private LobbyManager lobbyManager;

        private const float RefreshTime = 5f;
        private float _refreshTimeLeft;

        private readonly List<Lobby> _publicLobbies = new();

        private void OnEnable()
        {
            try
            {
                joinPrivateLobbyButton.onClick.AddListener(OnJoinPrivateLobby);
                goBackButton.onClick.AddListener(OnGoBack);
                refreshButton.onClick.AddListener(OnRefresh);
                _refreshTimeLeft = RefreshTime;
            }
            catch (Exception e)
            {
                Debug.LogError("Unable to fetch lobbies. Error: " + e.Message);
            }
        }

        private void OnDisable()
        {
            joinPrivateLobbyButton.onClick.RemoveAllListeners();
            goBackButton.onClick.RemoveAllListeners();
            refreshButton.onClick.RemoveAllListeners();
        }

        private void Start()
        {
            _ = RefreshPublicLobbies();
        }

        private void Update()
        {
            try
            {
                if (_refreshTimeLeft <= 0)
                {
                    _refreshTimeLeft = RefreshTime;
                    _ = RefreshPublicLobbies();
                }

                _refreshTimeLeft -= Time.deltaTime;
            }
            catch (Exception e)
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                Debug.LogError(e.Message);
            }
        }

        private void OnJoinPrivateLobby()
        {
            lobbyManager.onJoinedLobby.AddListener(OnJoinedLobby);
            _ = lobbyManager.JoinLobbyWithCode(lobbyCodeInput.text);
        }


        private void OnJoinPublicLobby(string lobbyId)
        {
            lobbyManager.onJoinedLobby.AddListener(OnJoinedLobby);
            _ = lobbyManager.JoinLobbyWithId(lobbyId);
        }
        
        private void OnJoinedLobby()
        {
            lobbyManager.onJoinedLobby.RemoveListener(OnJoinedLobby);
            lobbyUI.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }

        private void OnGoBack()
        {
            mainMenuUI.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }

        private void OnRefresh()
        {
            _ = RefreshPublicLobbies();
        }

        private async Task RefreshPublicLobbies()
        {
            var updatedLobbies = await lobbyManager.GetUpdatedLobbies();
            
            for (var i = 0; i < publicLobbiesSection.childCount; i++)
            {
                var child = publicLobbiesSection.GetChild(i);
                Destroy(child.gameObject);
            }

            for (var i = 0; i < updatedLobbies.Count; i++)
            {
                var lobby = updatedLobbies[i];
                var instantiate = Instantiate(publicLobbyUIPrefab, publicLobbiesSection).GetComponent<PublicLobbyUI>();
                instantiate.transform.localPosition = i * 35 * Vector3.down;
                var lobbyId = lobby.Id;
                instantiate.joinLobbyButton.onClick.AddListener(() => OnJoinPublicLobby(lobbyId));
                instantiate.lobbyNameText.text = lobby.Name;
            }
            
            
        }


    }
}
