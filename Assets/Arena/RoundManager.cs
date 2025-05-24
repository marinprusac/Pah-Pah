using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

namespace Arena
{
    public class RoundManager : NetworkBehaviour
    {

        public static RoundManager Instance;
        
        public NetworkObject playerPrefab;
        public Transform spawnsParent;
        
        
        
        private readonly Dictionary<ulong, ClientObject> _clientObjects = new();
        private readonly Dictionary<ulong, ArenaPlayer> _arenaPlayerObjects = new();
        private readonly List<Vector3> _spawns = new();

        private int _playerOneSpawnIndex;
        private int _playerTwoSpawnIndex;
        
        public override void OnNetworkSpawn()
        {
            Instance = this;



            
            for (int i = 0; i < spawnsParent.childCount; i++)
            {
                _spawns.Add(spawnsParent.GetChild(i).position);
            }

            var rand = new Random();
            _playerOneSpawnIndex = rand.Next(0, _spawns.Count-1);
            _playerTwoSpawnIndex = (_playerOneSpawnIndex + rand.Next(1, _spawns.Count - 1)) % _spawns.Count;
            
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

            var spawn = _spawns[_arenaPlayerObjects.Count == 0 ? _playerOneSpawnIndex : _playerTwoSpawnIndex];
            
            var obj = NetworkManager.SpawnManager.InstantiateAndSpawn(playerPrefab, clientId, true, position: spawn);
            _arenaPlayerObjects[clientId] = obj.GetComponent<ArenaPlayer>();
        }
    }
}
