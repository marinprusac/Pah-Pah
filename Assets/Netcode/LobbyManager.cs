using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEngine;

namespace Netcode
{
    
    public class LobbyManager
    {

        public static LobbyManager Instance;
        public static async void Initialize()
        {
            try
            {
                await UnityServices.InitializeAsync();
                Instance = new LobbyManager();
            }
            catch (Exception e)
            {
                Debug.LogError("Unable initialize Unity services. Error: " + e.Message);
            }
        }

        private LobbyManager()
        {
        }
        
        private string _playerName;

        private bool _isLobbyPrivate;
        private string LobbyCode { get; set; }
        private string LobbyId { get; set; }
        public bool AmIHost { get; private set; }
        private Player HostPlayer { get; set; }
        private Player GuestPlayer { get; set; }
        private Timer _heartbeatTimer;

        public string HostPlayerName => HostPlayer.Data["name"].Value;
        public string GuestPlayerName => GuestPlayer?.Data["name"].Value;
        
        public string CodeOrId => _isLobbyPrivate ? LobbyCode : LobbyId;

        
        public event Action JoinedLobby;
        public event Action<string> PlayerJoined;
        public event Action PlayerLeft;
        public event Action RemovedFromLobby;
        public event Action GameStarted;

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

        private void ResetData()
        {
            HostPlayer = null;
            GuestPlayer = null;
            AmIHost = false;
        }

        private void GetLobbyData(Lobby lobby)
        {
            _isLobbyPrivate = lobby.IsPrivate;
            LobbyCode = lobby.LobbyCode;
            LobbyId = lobby.Id;
            HostPlayer = lobby.Players[0];
            if (lobby.Players.Count > 1) GuestPlayer = lobby.Players[1];
            var callbacks = new LobbyEventCallbacks();
            callbacks.PlayerJoined += (joins) =>
            {
                var player = joins[0].Player;
                PlayerJoined?.Invoke(player.Data["name"].Value);
                GuestPlayer = player;
            };
            callbacks.PlayerLeft += (_) =>
            {
                PlayerLeft?.Invoke();
                GuestPlayer = null;
            };
            callbacks.LobbyDeleted += () =>
            {
                RemovedFromLobby?.Invoke();
                ResetData();
            };
            callbacks.KickedFromLobby += () =>
            {
                RemovedFromLobby?.Invoke();
                ResetData();
            };
            callbacks.DataChanged += (dataChange) =>
            {
                var startedData = dataChange["started"];
                if (startedData.Changed && startedData.Value.Value == "true")
                {
                    OnStarted();
                }
            };
            LobbyService.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);
        }
        
        public async Task CreateLobby(bool isPublic)
        {
            try
            {
                var player = new Player
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
                
                 var lobby = await LobbyService.Instance.CreateLobbyAsync(GUID.Generate().ToString(), 2, lobbyOptions);
                
                AmIHost = true;
                GetLobbyData(lobby);
                JoinedLobby?.Invoke();
                _heartbeatTimer = new Timer(15000);
                _heartbeatTimer.Elapsed += async (_, _) => await LobbyService.Instance.SendHeartbeatPingAsync(LobbyId);
                _heartbeatTimer.AutoReset = true;
                _heartbeatTimer.Start();
            }
            catch (Exception e)
            {
                Debug.LogError("Could not create a lobby. Error: " + e.Message);
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

                var lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinOptions);
                GetLobbyData(lobby);
                JoinedLobby?.Invoke();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        } 
        
        // ReSharper disable Unity.PerformanceAnalysis
        public async Task JoinLobbyWithId(string lobbyId)
        {
            try
            {
                var player = BuildPlayer();
                
                var joinOptions = new JoinLobbyByIdOptions {
                    Player = player
                };

                var lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinOptions);
                GetLobbyData(lobby);
                JoinedLobby?.Invoke();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        private Player BuildPlayer()
        {
            var player = new Player
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
                if (AmIHost)
                {
                    await LobbyService.Instance.DeleteLobbyAsync(LobbyId);
                    _heartbeatTimer?.Stop();
                    _heartbeatTimer?.Dispose();
                }
                else
                {
                    var playerId = AuthenticationService.Instance.PlayerId;
                    await LobbyService.Instance.RemovePlayerAsync(LobbyId, playerId);
                    
                }
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

                await LobbyService.Instance.UpdateLobbyAsync(LobbyId, lobbyUpdate);
            }
            catch (Exception e)
            {
                Debug.LogError("Unable to start a game. Error: " + e.Message);
            }
        }

        private void OnStarted()
        {
            GameStarted?.Invoke();
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
