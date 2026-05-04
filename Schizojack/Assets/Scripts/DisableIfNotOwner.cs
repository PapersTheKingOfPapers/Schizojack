using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class DisableIfNotOwner : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            gameObject.SetActive(false);
        }
    }
}
