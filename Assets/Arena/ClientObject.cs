using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Arena
{
    public class ClientObject : NetworkBehaviour
    {
        [SerializeField] private NetworkObject playerPrefab;


        private void Awake()
        {
            NetworkManager.SceneManager.OnLoadEventCompleted += (_, _, _, _) => Spawn();
        }


        private void Spawn()
        {
            if (IsLocalPlayer)
            {
                SpawnRpc(NetworkManager.LocalClientId);
            }
        }


        [Rpc(SendTo.Server)]
        private void SpawnRpc(ulong clientId)
        {
            var obj = NetworkManager.SpawnManager.InstantiateAndSpawn(playerPrefab, clientId, true, false, false, new Vector3(0, 1.337f, -36));
        }
        
    }
}
