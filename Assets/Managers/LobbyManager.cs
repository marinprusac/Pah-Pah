using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Managers
{
    
    public class LobbyManager
    {

        private static LobbyManager _instance;

        public static LobbyManager Instance
        {
            get => _instance ?? throw new Exception("LobbyManager not initialized. Join or create a lobby!");
            private set => _instance = value;
        }
        
        private LobbyManager(Lobby lobby, bool amHost)
        {
            IsLobbyPrivate = lobby.IsPrivate;
            LobbyCode = lobby.LobbyCode;
            LobbyId = lobby.Id;
            HostPlayer = lobby.Players[0];
            AmHost = amHost;
            if (lobby.Players.Count > 1) GuestPlayer = lobby.Players[1];

            if (AmHost)
            {
                _heartbeatTimer = new Timer(15000);
                _heartbeatTimer.Elapsed += async (_, _) => await LobbyService.Instance.SendHeartbeatPingAsync(LobbyId);
                _heartbeatTimer.AutoReset = true;
                _heartbeatTimer.Start();
            }
            

            var callbacks = new LobbyEventCallbacks();
            callbacks.PlayerJoined += joinData =>
            {
                GuestPlayer = joinData[0].Player;
                GuestJoined?.Invoke(GuestPlayerName);
            };
            callbacks.PlayerLeft += leftData =>
            {
                GuestPlayer = null;
                GuestLeft?.Invoke();
            };
            callbacks.KickedFromLobby += () =>
            {
                LeftLobby?.Invoke();
            };
            callbacks.LobbyDeleted += () =>
            {
                LeftLobby?.Invoke();
            };
            callbacks.DataAdded += values =>
            {
                foreach (var (variable, changedOrRemovedLobbyValue) in values)
                {
                    var value = changedOrRemovedLobbyValue.Value.Value;
                    _dataChanged?.Invoke(variable, value);
                }
            };
            callbacks.DataChanged += values =>
            {
                foreach (var (variable, changedOrRemovedLobbyValue) in values)
                {
                    var value = changedOrRemovedLobbyValue.Value.Value;
                    _dataChanged?.Invoke(variable, value);
                }
            };

            LobbyService.Instance.SubscribeToLobbyEventsAsync(LobbyId, callbacks);
        }

        public bool IsLobbyPrivate { get; private set; }
        public string LobbyCode { get; private set; }
        public string LobbyId { get; private set; }
        public bool AmHost { get; private set; }
        private Player HostPlayer { get; set; }
        private Player GuestPlayer { get; set; }
        
        private readonly Timer _heartbeatTimer;
        public string HostPlayerName => HostPlayer.Data["name"].Value;
        public string GuestPlayerName => GuestPlayer?.Data["name"].Value;


        public static Action JoinedLobby;
        public static Action LeftLobby;
        public static Action<string> GuestJoined;
        public static Action GuestLeft;
        private Action<string, string> _dataChanged;
        
        public static async Task CreateLobby(bool isPublic, string playerName)
        {
            try
            {
                var player = BuildPlayer(playerName);

                var lobbyOptions = new CreateLobbyOptions
                {
                    IsPrivate = !isPublic,
                    Player = player,
                };
                
                 var lobby = await LobbyService.Instance.CreateLobbyAsync(GUID.Generate().ToString(), 2, lobbyOptions);
                 
                Instance = new LobbyManager(lobby, true);
                JoinedLobby?.Invoke();

                
            }
            catch (Exception e)
            {
                Debug.LogError("Could not create a lobby. Error: " + e.Message);
            }
        }

        public static async Task JoinLobbyWithCode(string lobbyCode, string playerName)
        {
            try
            {
                var player = BuildPlayer(playerName);
                var joinOptions = new JoinLobbyByCodeOptions {
                    Player = player
                };
                var lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinOptions);
                Instance = new LobbyManager(lobby, false);
                JoinedLobby?.Invoke();

            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        } 
        
        // ReSharper disable Unity.PerformanceAnalysis
        public static async Task JoinLobbyWithId(string lobbyId, string playerName)
        {
            try
            {
                var player = BuildPlayer(playerName);
                var joinOptions = new JoinLobbyByIdOptions {
                    Player = player
                };
                var lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinOptions);
                Instance = new LobbyManager(lobby, false);
                JoinedLobby?.Invoke();

            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        private static Player BuildPlayer(string playerName)
        {
            var player = new Player
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    ["name"] = new(PlayerDataObject.VisibilityOptions.Member, playerName)
                }
            };
            return player;
        }


        public async void LeaveLobby()
        {
            try
            {
                if (AmHost)
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

        private void OnLeftLobby()
        {
            LeftLobby?.Invoke();
            Instance = null;
        }
        
        public static async Task<List<Lobby>> GetUpdatedLobbies()
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

        public void SubscribeToDataChanged(string variable, Action<string> callback)
        {
            _dataChanged += (v, val) =>
            {
                if (v == variable) callback(val);
            };
        }

        public void ChangeData(string variable, string value)
        {
            LobbyService.Instance.UpdateLobbyAsync(LobbyId, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { variable, new DataObject(DataObject.VisibilityOptions.Member, value)}
                }
            });
        }
    }
}
