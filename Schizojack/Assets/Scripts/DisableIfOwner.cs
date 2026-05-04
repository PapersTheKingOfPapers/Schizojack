using Unity.Netcode;
using UnityEngine;

public class DisableIfOwner : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            gameObject.SetActive(false);
        }
    }
}
