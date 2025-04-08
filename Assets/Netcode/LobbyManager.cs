using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Events;

namespace Netcode
{
    
    public class LobbyManager : MonoBehaviour
    {
        
        public UnityEvent onJoinedLobby;
        public UnityEvent onLeftLobby;
        private string _playerName;
        private Lobby _lobby;

        private async void OnEnable()
        {
            try
            {
                await UnityServices.InitializeAsync();
            }
            catch (Exception e)
            {
                Debug.LogError("Unable initialize Unity services. Error: " + e.Message);
            }
        }


        public async void Authenticate(string playerName)
        {
            try
            {
                AuthenticationService.Instance.SignedIn += () =>
                {
                    Debug.Log("Signed in as " + AuthenticationService.Instance.PlayerId);
                };
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                _playerName = playerName;
            }
            catch (Exception e)
            {
                Debug.LogError("Unable to sign in. Error: " + e.StackTrace);
            }
        }

        public void LogOut()
        {
            AuthenticationService.Instance.SignOut();
        }

        public async Task CreateLobby(string lobbyName, bool isPublic)
        {
            try
            {
                var player = new Unity.Services.Lobbies.Models.Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        ["name"] = new(PlayerDataObject.VisibilityOptions.Member, _playerName)
                    }
                };

                 var lobbyOptions = new CreateLobbyOptions
                 {
                     IsPrivate = !isPublic,
                     Player = player,
                     Data = new Dictionary<string, DataObject>
                     {
                         ["started"] = new(DataObject.VisibilityOptions.Public, "false")
                     }
                 };

                _lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 4, lobbyOptions);
                
                Debug.Log(_lobby.LobbyCode);
                
                CreateRelay();
                onJoinedLobby.Invoke();
                StartCoroutine(HeartbeatLobbyCoroutine(15));
            }
            catch (Exception e)
            {
                Debug.LogError("Could not create a lobby. Error: " + e.Message);
            }
        }

        private IEnumerator HeartbeatLobbyCoroutine(float waitTimeSeconds)
        {
            var delay = new WaitForSecondsRealtime(waitTimeSeconds);

            while (true)
            {
                LobbyService.Instance.SendHeartbeatPingAsync(_lobby.Id);
                yield return delay;
            }
        }

        public async Task JoinLobbyWithCode(string lobbyCode)
        {
            try
            {
                var player = BuildPlayer();
                
                var joinOptions = new JoinLobbyByCodeOptions {
                    Player = player
                };

                _lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinOptions);
                JoinRelay();
                onJoinedLobby.Invoke();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        } 
        
        public async Task JoinLobbyWithId(string lobbyId)
        {
            try
            {
                var player = BuildPlayer();
                
                var joinOptions = new JoinLobbyByIdOptions {
                    Player = player
                };

                _lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinOptions);
                JoinRelay();
                onJoinedLobby.Invoke();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        private Unity.Services.Lobbies.Models.Player BuildPlayer()
        {
            var player = new Unity.Services.Lobbies.Models.Player
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    ["name"] = new(PlayerDataObject.VisibilityOptions.Member, _playerName)
                }
            };
            return player;
        }


        public async void LeaveLobby()
        {
            try
            {
                var playerId = AuthenticationService.Instance.PlayerId;
                await LobbyService.Instance.RemovePlayerAsync(_lobby.Id, playerId);
            }
            catch (Exception e)
            {
                Debug.LogError("Error when leaving lobby: " + e.Message);
            }
        }

        public async void StartGame()
        {
            try
            {
                var lobbyUpdate = new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        ["started"] = new(DataObject.VisibilityOptions.Public, "true")
                    },
                    IsLocked = true
                };

                await LobbyService.Instance.UpdateLobbyAsync(_lobby.Id, lobbyUpdate);
            }
            catch (Exception e)
            {
                Debug.LogError("Unable to start a game. Error: " + e.Message);
            }
        }
        
        private async void CreateRelay()
        {
            try
            {
                var allocation = await RelayService.Instance.CreateAllocationAsync(19);
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(allocation.ToRelayServerData(connectionType: "udp"));
                
                if (!NetworkManager.Singleton.StartHost())
                {
                    throw new Exception("Could not start host");
                }

                var playerId = AuthenticationService.Instance.PlayerId;
                var playerOptionsUpdate = new UpdatePlayerOptions
                {
                    AllocationId = allocation.AllocationId.ToString()
                };
                await LobbyService.Instance.UpdatePlayerAsync(_lobby.Id, playerId, playerOptionsUpdate);

                var relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                var lobbyOptionsUpdate = new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        ["relayCode"] = new(DataObject.VisibilityOptions.Member, relayCode)
                    }
                };
                await LobbyService.Instance.UpdateLobbyAsync(_lobby.Id, lobbyOptionsUpdate);


            }
            catch (Exception e)
            {
                Debug.LogError("Could not create a relay. Error: " + e.Message);
            }
        }

        private async void JoinRelay()
        {
            try
            {
                var playerId = AuthenticationService.Instance.PlayerId;
                var joinCode = _lobby.Data["relayCode"].Value;
                var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(allocation.ToRelayServerData(connectionType: "udp"));
                
                if (!NetworkManager.Singleton.StartClient())
                    throw new Exception("Could not start a client.");

                var updatedPlayerOptions = new UpdatePlayerOptions
                {
                    AllocationId = allocation.AllocationId.ToString()
                };

                await LobbyService.Instance.UpdatePlayerAsync(_lobby.Id, playerId, updatedPlayerOptions);

            }
            catch (Exception e)
            {
                Debug.LogError("Could not join a relay. Error: " + e.Message);
            }
        }
        
        public async Task<List<Lobby>> GetUpdatedLobbies()
        {
            var options = new QueryLobbiesOptions
            {
                Count = 5,
                Filters = new List<QueryFilter>
                {
                    new(QueryFilter.FieldOptions.IsLocked, "false", QueryFilter.OpOptions.EQ)
                }
            };
            var response = await LobbyService.Instance.QueryLobbiesAsync(options);
            var lobbies = response.Results;
            return lobbies;
        }
    }
}
