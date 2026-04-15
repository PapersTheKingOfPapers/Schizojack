using System.Globalization;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class PlayerInitializer : NetworkBehaviour
{
    public NetworkVariable<int> PlayerIndex = new NetworkVariable<int>();

    private bool _initialized = false;
    public override void OnNetworkSpawn()
    {
        if (!IsClient) return;

        PlayerIndex.OnValueChanged += OnIndexChanged;

        if (PlayerIndex.Value != -1)
            Initialize(PlayerIndex.Value);
    }
    private void OnIndexChanged(int oldVal, int newVal)
    {
        if (newVal < 0) return;
        Initialize(newVal);
    }

    private void Initialize(int index)
    {
        if (_initialized) return;
        _initialized = true;

        gameObject.tag = $"Actor{index}";

        var SAF = FindAnyObjectByType<SchizojackActorFrontend>();
        var actor = GetComponentInChildren<SchizojackActor>();

        Debug.Log($"Registering actor {actor.name} with index {index}");

        if (!SAF.Actors.Contains(actor))
        {
            SAF.Actors.Add(actor);
        }

        if (IsOwner)
        {
            var SNB = FindAnyObjectByType<SchizojackNetworkBackend>();
            var SB = FindAnyObjectByType<SchizojackBackend>();

            SNB._localUserNumber = index;
            SB._localUserNumber = index;

            Debug.Log($"[CLIENT] Local user number: {index}");
        }
    }
}