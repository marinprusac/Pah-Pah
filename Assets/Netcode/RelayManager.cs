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

namespace Netcode
{
    public class RelayManager
    {
        
        public static RelayManager Instance;
        public static void Initialize()
        {
            try
            {
                Instance = new RelayManager();
            }
            catch (Exception e)
            {
                Debug.LogError("Unable initialize Unity services. Error: " + e.Message);
            }
        }

        private RelayManager()
        {
        }
        
        
        
        public async void CreateRelay(string lobbyId)
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
                var lobbyOptionsUpdate = new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        ["relayCode"] = new(DataObject.VisibilityOptions.Member, relayCode)
                    }
                };
                await LobbyService.Instance.UpdateLobbyAsync(lobbyId, lobbyOptionsUpdate);

            }
            catch (Exception e)
            {
                Debug.LogError("Could not create a relay. Error: " + e.Message);
            }

        }

        public async void JoinRelay(string lobbyId, string relayCode)
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

            }
            catch (Exception e)
            {
                Debug.LogError("Could not join a relay. Error: " + e.Message);
            }
        }
    }
}