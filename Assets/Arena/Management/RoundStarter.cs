using Unity.Netcode;
using UnityEngine;

namespace Arena.Management
{
    public class RoundStarter : MonoBehaviour
    {
        
        public NetworkObject gameManagerPrefab;

        public void Start()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(gameManagerPrefab, destroyWithScene: true);
            }
        }

    }
}
