using Unity.Netcode;
using UnityEngine;

namespace Netcode
{
    public class Player : NetworkBehaviour
    {

        // Update is called once per frame
        void Update()
        {
            if (IsLocalPlayer)
            {
                transform.position += Vector3.right * Time.deltaTime;
            }
        }
    }
}
