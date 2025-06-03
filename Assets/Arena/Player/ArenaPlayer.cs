using Arena.Management;
using Arena.UI;
using Unity.Netcode;
using UnityEngine;

namespace Arena.Player
{
    public class ArenaPlayer : NetworkBehaviour
    {
        [SerializeField]
        private GameObject visionPrefab;
        [SerializeField] private Transform aim;
        
        
        
        private SightCheck _visionInstance;
        public PlayerControls PlayerControls { get; private set; }


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
                _visionInstance = Instantiate(visionPrefab, transform).GetComponent<SightCheck>();
                SetLayerRecursively(gameObject, 3);
               PlayerControls = GetComponent<PlayerControls>();
            }
            
        }

        public float cooldownDuration = 5;
        private float cooldown;

        private void Update()
        {
            if (IsOwner && _visionInstance.TurnedOn)
            {
                _visionInstance.transform.rotation = aim.rotation;
                cooldown -= Mathf.Max(0, Time.deltaTime);
                if (Input.GetMouseButtonDown(0) && cooldown <= 0)
                {
                    cooldown = cooldownDuration;
                    var result = _visionInstance.Check();
                    if (result)
                    {
                        GameManager.Instance.PahPahRpc(NetworkManager.LocalClientId);
                        UIManager.Instance.FlashAnimation(new Color(1,1,1,0.5f), 0.75f);
                    }
                    else
                    {
                        UIManager.Instance.ReloadAnimation(cooldownDuration);
                        UIManager.Instance.FlashAnimation(new Color(0,0,0,0.5f), 0.75f);
                    }
                }
            }
        }

        [Rpc(SendTo.Owner)]
        public void AssignSpawnPositionRpc(Vector3 spawnPosition)
        {
            transform.position = spawnPosition;
        }

        [Rpc(SendTo.Owner)]
        public void StartMovingRpc()
        {
            PlayerControls.TurnedOn = true;
            _visionInstance.TurnedOn = true;
        }
        
        [Rpc(SendTo.Owner)]
        public void StopMovingRpc()
        {
            PlayerControls.TurnedOn = false;
            _visionInstance.TurnedOn = false;
        }
    }
}
