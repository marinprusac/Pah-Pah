using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

namespace Arena.Management
{
    public class GameManager : NetworkBehaviour
    {

        public static GameManager Instance;
        
        public NetworkObject playerPrefab;
        public Transform spawnsParent;
        public NetworkObject musicPlayer;
        
        
        
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
            print("Spawn indices: " + _playerOneSpawnIndex + ", " + _playerTwoSpawnIndex);
            
            foreach (var connectedClientsValue in NetworkManager.ConnectedClients.Values)
            {
                _clientObjects[connectedClientsValue.ClientId] = connectedClientsValue.PlayerObject.GetComponent<ClientObject>();
            }
            
            SpawnPlayerRpc(NetworkManager.LocalClientId);

            if (IsServer)
            {
                NetworkManager.SpawnManager.InstantiateAndSpawn(musicPlayer);
            }
            
        }

        public void OnDisable()
        {
            Instance = null;
        }

        private bool _spawnedOne;

        [Rpc(SendTo.Server)]
        private void SpawnPlayerRpc(ulong clientId)
        {
            
            print("Already spawned: " + _spawnedOne);
            Vector3 spawn;
            if (!_spawnedOne)
            {
                _spawnedOne = true;
                spawn = _spawns[_playerOneSpawnIndex];
            }
            else
            {
                spawn = _spawns[_playerTwoSpawnIndex];
            }
            print("Spawning on: " + spawn);
            
            var obj = NetworkManager.SpawnManager.InstantiateAndSpawn(playerPrefab, clientId, true, position: spawn);
            _arenaPlayerObjects[clientId] = obj.GetComponent<ArenaPlayer>();
        }
    }
}
