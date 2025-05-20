using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Managers
{
    public class RelayManager
    {
        private static RelayManager _instance;
        private string _relayCode;

        public static RelayManager Instance
        {
            get => _instance ?? throw new Exception("RelayManager not initialized.");
            private set => _instance = value;
        }

        private RelayManager()
        {
            LobbyManager.Instance.SubscribeToDataChanged("relayCode", code =>
            {
                if(!LobbyManager.Instance.AmHost)
                    JoinRelay(LobbyManager.Instance.LobbyId, code);
            });
        }

        public static void Initialize()
        {
            Instance = new RelayManager();
        }
        
        public static async void CreateRelay(string lobbyId)
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
                var relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                
                await LobbyService.Instance.UpdatePlayerAsync(lobbyId, playerId, playerOptionsUpdate);
                // var lobbyOptionsUpdate = new UpdateLobbyOptions
                // {
                //     Data = new Dictionary<string, DataObject>
                //     {
                //         ["relayCode"] = new(DataObject.VisibilityOptions.Member, relayCode)
                //     }
                // };
                // await LobbyService.Instance.UpdateLobbyAsync(lobbyId, lobbyOptionsUpdate);
                
                LobbyManager.Instance.ChangeData("relayCode", relayCode);
            }
            catch (Exception e)
            {
                Debug.LogError("Could not create a relay. Error: " + e.Message);
            }

        }

        public static async void JoinRelay(string lobbyId, string relayCode)
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

                await LobbyService.Instance.UpdatePlayerAsync(lobbyId, playerId, updatedPlayerOptions);
                GameManager.StartGame(false);
            }
            catch (Exception e)
            {
                Debug.LogError("Could not join a relay. Error: " + e.Message);
            }
        }
    }
}