using System;
using System.Threading.Tasks;
using TMPro;
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
        
        private const float RefreshTime = 5f;
        private float _refreshTimeLeft;
        
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
            LobbyManager.Instance.JoinedLobby += OnJoinedLobby;
            _ = LobbyManager.Instance.JoinLobbyWithCode(lobbyCodeInput.text);
        }


        private void OnJoinPublicLobby(string lobbyId)
        {
            LobbyManager.Instance.JoinedLobby += OnJoinedLobby;
            _ = LobbyManager.Instance.JoinLobbyWithId(lobbyId);
        }
        
        private void OnJoinedLobby()
        {
            LobbyManager.Instance.JoinedLobby -= OnJoinedLobby;
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
            var updatedLobbies = await LobbyManager.Instance.GetUpdatedLobbies();
            
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
                instantiate.lobbyIdText.text = lobbyId;
            }
            
            
        }


    }
}
