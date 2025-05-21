using Unity.Netcode;
using UnityEngine;

namespace Arena
{
    public class RoundManagerSpawner : MonoBehaviour
    {

        public NetworkObject roundManagerPrefab;

        private void Start()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(roundManagerPrefab, destroyWithScene: true);
            }
        }

    }
}
