using System.Collections;
using Arena.Management;
using Arena.Player;
using Unity.Netcode;
using UnityEngine;

namespace Arena.Coin
{
    public class CollectibleCoin : NetworkBehaviour
    {
    
        [SerializeField]
        private float spinningSpeed;

        [SerializeField] private float collectDuration;
        [SerializeField] private float collectSpinFactor;

        private bool _pickupable = true;
        
        void Update()
        {
            if (IsServer && _pickupable)
            {
                transform.Rotate(Vector3.up, Time.deltaTime * spinningSpeed);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;
            if (!_pickupable) return;

            if (other.TryGetComponent<ArenaPlayer>(out var player))
            {
                CoinCollectedRpc(player.OwnerClientId);
            }
            
        }

        [Rpc(SendTo.Server)]
        private void CoinCollectedRpc(ulong clientId, RpcParams rpcParams = default)
        {
            _pickupable = false;
            StartCoroutine(CollectTween());
            GameManager.Instance.AssignPointsRpc(1, clientId);
            
        }
        
        private IEnumerator CollectTween()
        {

            for (var timeLeft = collectDuration; timeLeft > 0; timeLeft -= Time.deltaTime)
            {
                transform.localScale = Vector3.one * timeLeft / collectDuration;
                transform.Rotate(Vector3.up, Time.deltaTime * spinningSpeed * collectSpinFactor);
                yield return null;
                
                
            } 
            NetworkObject.Despawn();
        }
    }
}
