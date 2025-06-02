using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Arena.Management
{
    public class SpawnPointManager : MonoBehaviour
    {


        [SerializeField] private Transform playerSpawnPointParent;
        [SerializeField] private Transform coinSpawnPointParent;

        private readonly List<Vector3> _playerSpawnPoints = new();
        private readonly List<Vector3> _coinSpawnPoints = new();

        public static SpawnPointManager Instance { get; private set; }

        private void Awake()
        {
            foreach (Transform spawnPoint in playerSpawnPointParent)
            {
                _playerSpawnPoints.Add(spawnPoint.position);
            }
        
            foreach (Transform spawnPoint in coinSpawnPointParent)
            {
                _coinSpawnPoints.Add(spawnPoint.position);
            }
        }
        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            Instance = null;
        }
    
    
        public void GetPlayerSpawnPoints(out Vector3 player1, out Vector3 player2)
        {
            var spawnPointIndex1 = Random.Range(0, _playerSpawnPoints.Count);
            var spawnPointIndex2 = (spawnPointIndex1 + Random.Range(1, _playerSpawnPoints.Count)) % _playerSpawnPoints.Count;
            player1 = _playerSpawnPoints[spawnPointIndex1];
            player2 = _playerSpawnPoints[spawnPointIndex2];
        }

        public List<Vector3> GetCoinSpawnPoints()
        {
            return new List<Vector3>(_coinSpawnPoints);
        }
    }
}
