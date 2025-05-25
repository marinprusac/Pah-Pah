using System.Collections.Generic;
using Arena.Player;
using Managers;
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


        private ArenaPlayer _hostPlayer;
        private ArenaPlayer _guestPlayer;

        private ulong HostId => _hostPlayer.OwnerClientId;
        private ulong GuestId => _guestPlayer.OwnerClientId;
        

        private readonly NetworkVariable<int> _hostPoints = new();
        private readonly NetworkVariable<int> _guestPoints = new();

        private string HostName => LobbyManager.Instance.HostPlayerName;
        private string GuestName => LobbyManager.Instance.GuestPlayerName;
        
        
        private readonly List<Vector3> _spawns = new();

        private int _hostSpawnIndex;
        private int _guestSpawnIndex;
        
        public override void OnNetworkSpawn()
        {
            Instance = this;
            
            for (int i = 0; i < spawnsParent.childCount; i++)
            {
                _spawns.Add(spawnsParent.GetChild(i).position);
            }

            var rand = new Random();
            _hostSpawnIndex = rand.Next(0, _spawns.Count-1);
            _guestSpawnIndex = (_hostSpawnIndex + rand.Next(1, _spawns.Count - 1)) % _spawns.Count;
            
            SpawnPlayerRpc();

            if (IsServer)
            {
                NetworkManager.SpawnManager.InstantiateAndSpawn(musicPlayer);
            }
            
        }

        public void OnDisable()
        {
            Instance = null;
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
                _hostPlayer.AssignSpawnPositionRpc(_spawns[_hostSpawnIndex]);
            }
            else
            {
                _guestPlayer = arenaPlayer;
                _guestPlayer.AssignSpawnPositionRpc(_spawns[_guestSpawnIndex]);
            }
        }


        [Rpc(SendTo.Server)]
        public void AssignPointsRpc(int points, ulong clientId)
        {
            if (clientId == HostId) _hostPoints.Value += points;
            else if (clientId == GuestId) _guestPoints.Value += points;
            print("Host points: " + _hostPoints.Value + "; Guest points: " + _guestPoints.Value);
        }
    }
}
