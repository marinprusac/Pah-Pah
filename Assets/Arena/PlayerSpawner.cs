using Unity.Netcode;
using UnityEngine;

namespace Arena
{
    public class PlayerSpawner : MonoBehaviour
    {
        public NetworkObject playerPrefab;
        
        void Start()
        {
            SpawnServerRpc();
        }


        [Rpc(SendTo.Server)]
        void SpawnServerRpc()
        {
            print("oh!");
            var obj = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(playerPrefab, NetworkManager.Singleton.LocalClient.ClientId, true, false, false, new Vector3(0, 1.337f, -36));
        }

    }
}
