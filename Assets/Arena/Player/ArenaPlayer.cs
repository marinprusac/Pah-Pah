using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;

namespace Arena.Player
{
    public class ArenaPlayer : NetworkBehaviour
    {
        [SerializeField]
        private GameObject visionPrefab;

        private GameObject _visionInstance;

        [SerializeField] private Transform aim;
        
        
        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj is null) return;

            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                if (child is null) continue;
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
        
        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                _visionInstance = Instantiate(visionPrefab, transform);
                SetLayerRecursively(gameObject, 3);
            }
            
        }

        private void Update()
        {
            if (IsOwner)
            {
                _visionInstance.transform.rotation = aim.rotation;
            }
        }

        [Rpc(SendTo.Owner)]
        public void AssignSpawnPositionRpc(Vector3 spawnPosition)
        {
            transform.position = spawnPosition;
        }
        
    }
}
