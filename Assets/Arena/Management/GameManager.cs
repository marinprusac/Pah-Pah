using System.Collections;
using System.Collections.Generic;
using Arena.Coin;
using Arena.Player;
using Arena.Sound;
using Arena.UI;
using Menu.Managers;
using Unity.Netcode;
using UnityEngine;

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

        public bool AmHost => NetworkManager.LocalClientId == HostId;
        
        private ArenaPlayer MyPlayer => AmHost ? _hostPlayer : _guestPlayer;

        private ulong HostId => _hostPlayer.OwnerClientId;
        private ulong GuestId => _guestPlayer.OwnerClientId;

        private Vector3 _hostSpawnPoint;
        private Vector3 _guestSpawnPoint;
        

        private int _hostPoints;
        private int _guestPoints;

        private string HostName => LobbyManager.Instance.HostPlayerName;
        private string GuestName => LobbyManager.Instance.GuestPlayerName;

        private List<NetworkObject> _spawnedCoins = new();

        
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
                SpawnCoins();
                RandomMusicPlayer.Instance.SyncNextSong();
            }
            SpawnPlayerRpc();
        }


        private void EndRound(bool won)
        {
            if (!won)
            {
                UIManager.Instance.LoserAnimation();
            }
            StartCoroutine(DelayedShowResultsCoroutine());
        }
        
        private IEnumerator DelayedShowResultsCoroutine()
        {
            RandomMusicPlayer.Instance.FadeOutVolume();
            yield return new WaitForSeconds(3);
            UIManager.Instance.FadeOut(UIManager.Instance.ShowPoints);
            yield return new WaitForSeconds(4);
            if (IsServer)
            {
                _hostPlayer.NetworkObject.Despawn();
                _guestPlayer.NetworkObject.Despawn();
                DespawnCoins();
            }
            UIManager.Instance.FadeIn(StartRound);
        }
        
        

        private void SpawnCoins()
        {
            var coinSpawnPoints = SpawnPointManager.Instance.GetCoinSpawnPoints();
            foreach (var spawnPoint in coinSpawnPoints)
            {
                var instance = NetworkManager.SpawnManager.InstantiateAndSpawn(coinPrefab, position: spawnPoint, destroyWithScene: true);
                _spawnedCoins.Add(instance);
            }
        }

        private void DespawnCoins()
        {
            foreach (var coin in _spawnedCoins)
            {
                if(coin && coin.IsSpawned)
                    coin.Despawn();
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
                StartCoroutine(DelayMoveStart());
            }
        }

        private IEnumerator DelayMoveStart()
        {
            
            yield return new WaitForSeconds(1);
            _hostPlayer.StartMovingRpc();
            _guestPlayer.StartMovingRpc();
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
            _hostPlayer.StopMovingRpc();
            _guestPlayer.StopMovingRpc();
            AssignPointsRpc(5, clientId);
            RoundEndRpc(clientId, _hostPoints, _guestPoints);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void RoundEndRpc(ulong clientWinner, int hostPoints, int guestPoints)
        {
            _hostPoints = hostPoints;
            _guestPoints = guestPoints;
            var myPoints = AmHost ? hostPoints : guestPoints;
            var opponentsPoints = AmHost ? guestPoints : hostPoints;
            var myName = AmHost ? HostName : GuestName;
            var opponentsName = AmHost ? GuestName : HostName;
            UIManager.Instance.SetPoints(myName, myPoints, opponentsName, opponentsPoints);
            EndRound(clientWinner == NetworkManager.LocalClientId);
        }

    }
}
