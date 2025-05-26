using System.Collections.Generic;
using Arena.Player;
using Managers;
using Menu.Managers;
using NUnit.Framework;
using Unity.Netcode;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using Random = System.Random;

namespace Arena.Management
{
    public class GameManager : NetworkBehaviour
    {

        public static GameManager Instance;
        
        public NetworkObject playerPrefab;
        public NetworkObject coinPrefab;
        public NetworkObject musicPlayer;


        private ArenaPlayer _hostPlayer;
        private ArenaPlayer _guestPlayer;

        private ulong HostId => _hostPlayer.OwnerClientId;
        private ulong GuestId => _guestPlayer.OwnerClientId;

        private Vector3 _hostSpawnPoint;
        private Vector3 _guestSpawnPoint;
        

        private int _hostPoints;
        private int _guestPoints;

        private string HostName => LobbyManager.Instance.HostPlayerName;
        private string GuestName => LobbyManager.Instance.GuestPlayerName;
        
        public void OnDisable()
        {
            Instance = null;
        }
        
        public override void OnNetworkSpawn()
        {
            Instance = this;
            StartRound();
        }

        private void StartRound()
        {
            if (IsServer)
            {
                SpawnPointManager.Instance.GetPlayerSpawnPoints(out _hostSpawnPoint, out _guestSpawnPoint);
                NetworkManager.SpawnManager.InstantiateAndSpawn(musicPlayer);
            }
            SpawnCoins();
            SpawnPlayerRpc();
        }


        private void EndRound()
        {
            RenderSettings.fogColor = Color.white;
        }

        private void SpawnCoins()
        {
            var coinSpawnPoints = SpawnPointManager.Instance.GetCoinSpawnPoints();
            foreach (var spawnPoint in coinSpawnPoints)
            {
                NetworkManager.SpawnManager.InstantiateAndSpawn(playerPrefab, position: spawnPoint, destroyWithScene: true);
            }
        }
        
        [Rpc(SendTo.Server)]
        private void SpawnPlayerRpc(RpcParams rpcParams = default)
        {

            var clientId = rpcParams.Receive.SenderClientId;
            var isHost = NetworkManager.LocalClientId == clientId;
            
            var obj = NetworkManager.SpawnManager.InstantiateAndSpawn(playerPrefab, destroyWithScene: true, ownerClientId: clientId);
            var arenaPlayer = obj.GetComponent<ArenaPlayer>();
            
            
            if (isHost)
            {
                _hostPlayer = arenaPlayer;
                _hostPlayer.AssignSpawnPositionRpc(_hostSpawnPoint);
            }
            else
            {
                _guestPlayer = arenaPlayer;
                _guestPlayer.AssignSpawnPositionRpc(_guestSpawnPoint);
            }
        }


        [Rpc(SendTo.Server)]
        public void AssignPointsRpc(int points, ulong clientId)
        {
            if (clientId == HostId) _hostPoints += points;
            else if (clientId == GuestId) _guestPoints += points;
            print("Host points: " + _hostPoints + "; Guest points: " + _guestPoints);
        }

        [Rpc(SendTo.Server)]
        public void PahPahRpc(ulong clientId)
        {
            AssignPointsRpc(5, clientId);
            RoundEndRpc(clientId, _hostPoints, _guestPoints);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void RoundEndRpc(ulong clientWinner, int hostPoints, int guestPoints)
        {
            _hostPoints = hostPoints;
            _guestPoints = guestPoints;
            EndRound();
        }
    }
}
