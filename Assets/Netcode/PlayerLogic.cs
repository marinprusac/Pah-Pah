using Unity.Netcode;
using UnityEngine;

namespace Netcode
{
    public class PlayerLogic : NetworkBehaviour
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
