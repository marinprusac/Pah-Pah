using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Arena
{
    public class RoundManager : NetworkBehaviour
    {

        public static RoundManager Instance;
        
        public NetworkObject playerPrefab;
        


        private Dictionary<ulong, ClientObject> _clientObjects;
        private Dictionary<ulong, ArenaPlayer> _arenaPlayerObjects;
        
        public override void OnNetworkSpawn()
        {
            Instance = this;

            _clientObjects = new Dictionary<ulong, ClientObject>();
            _arenaPlayerObjects = new Dictionary<ulong, ArenaPlayer>();
            foreach (var connectedClientsValue in NetworkManager.ConnectedClients.Values)
            {
                _clientObjects[connectedClientsValue.ClientId] = connectedClientsValue.PlayerObject.GetComponent<ClientObject>();
            }
            
            SpawnPlayerRpc(NetworkManager.LocalClientId);
        }

        public void OnDisable()
        {
            Instance = null;
        }

        [Rpc(SendTo.Server)]
        private void SpawnPlayerRpc(ulong clientId)
        {
            var obj = NetworkManager.SpawnManager.InstantiateAndSpawn(playerPrefab, clientId, true, position: new Vector3(0, 1.337f, -36));
            _arenaPlayerObjects[clientId] = obj.GetComponent<ArenaPlayer>();
        }
    }
}
