using Unity.Netcode;
using UnityEngine;

namespace Arena
{
    public class RoundManager : NetworkBehaviour
    {

        public NetworkObject playerPrefab;

        public static RoundManager Instance;
        public override void OnNetworkSpawn()
        {
            Instance = this;
            SpawnPlayerRpc(NetworkManager.LocalClientId);
        }

        [Rpc(SendTo.Server)]
        private void SpawnPlayerRpc(ulong clientId)
        {
            var obj = NetworkManager.SpawnManager.InstantiateAndSpawn(playerPrefab, clientId, true, false, false, new Vector3(0, 1.337f, -36));

        }
    }
}
