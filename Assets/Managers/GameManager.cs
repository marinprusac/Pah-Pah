using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public static class GameManager
    {

        private static bool _initialized;
        public static void Initialize()
        {

            if (_initialized) throw new Exception("GameManager already initialized.");
            
            LobbyManager.Instance.SubscribeToLobbyDataChanged("relayCode", async (code) =>
            {
                if (!LobbyManager.Instance.AmHost)
                {
                    await JoinGame(code);
                    await LobbyManager.Instance.ChangePlayerData(AuthenticationService.Instance.PlayerId, "ready", "");
                }

            });
            
            LobbyManager.Instance.SubscribeToPlayerDataChanged("ready", (playerId, value) =>
            {
                if (LobbyManager.Instance.AmHost)
                {
                    NetworkManager.Singleton.SceneManager.LoadScene("Arena", LoadSceneMode.Single);
                }
                GameStarted?.Invoke();
            });

            _initialized = true;
        }

        public static Action GameStarted;
        
        public static async void StartGame()
        {
            try
            {
                var allocation = await RelayService.Instance.CreateAllocationAsync(19);
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(allocation.ToRelayServerData(connectionType: "udp"));
                
                if (!NetworkManager.Singleton.StartHost())
                {
                    throw new Exception("Could not start host");
                }
                
                var relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                await LobbyService.Instance.UpdatePlayerAsync(LobbyManager.Instance.LobbyId, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
                {
                    AllocationId = allocation.AllocationId.ToString()
                });
                await LobbyManager.Instance.ChangeLobbyData("relayCode", relayCode);
            }
            catch (Exception e)
            {
                Debug.LogError("Could not create a relay. Error: " + e.Message);
            }

        }

        public static async Task JoinGame(string relayCode)
        {
            try
            {
                var playerId = AuthenticationService.Instance.PlayerId;
                var allocation = await RelayService.Instance.JoinAllocationAsync(relayCode);
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(allocation.ToRelayServerData(connectionType: "udp"));
                
                if (!NetworkManager.Singleton.StartClient())
                    throw new Exception("Could not start a client.");

                var updatedPlayerOptions = new UpdatePlayerOptions
                {
                    AllocationId = allocation.AllocationId.ToString()
                };

                await LobbyService.Instance.UpdatePlayerAsync(LobbyManager.Instance.LobbyId, playerId, updatedPlayerOptions);
            }
            catch (Exception e)
            {
                Debug.LogError("Could not join a relay. Error: " + e.Message);
            }
        }
    }
}