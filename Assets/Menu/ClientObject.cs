using System;
using Unity.Netcode;
using UnityEngine;

namespace Menu
{
    public class ClientObject : NetworkBehaviour
    {
        private void Start()
        {
        }

        private void Update()
        {
            if (IsLocalPlayer)
            {
            }
        }
    }
}
