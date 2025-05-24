using System;
using Unity.Netcode;
using UnityEngine;

namespace Arena
{
    public class ArenaPlayer : NetworkBehaviour
    {
        [SerializeField]
        private GameObject visionPrefab;

        public void Start()
        {
            if (IsOwner)
            {
                Instantiate(visionPrefab, transform);
            }
            else
            {
                gameObject.layer = 6;
            }
        }
    }
}
