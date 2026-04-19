using System.Globalization;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.ProBuilder;

public class PlayerInitializer : NetworkBehaviour
{
    public NetworkVariable<int> PlayerIndex = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool _initialized = false;
    public override void OnNetworkSpawn()
    {
        if (!IsClient) return;

        PlayerIndex.OnValueChanged += OnIndexChanged;

        // Handle already-set value (host case)
        if (PlayerIndex.Value >= 0)
        {
            Initialize(PlayerIndex.Value);
        }
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

        actor.gameObject.tag = $"Actor{index}";

        Debug.Log($"Registering actor {gameObject.tag} with index {index}");

        if (!SAF.Actors.Contains(actor))
        {
            SAF.Actors.Add(actor);
        }

        actor._SB._actors[index].OwnerClientId = OwnerClientId;

        if (IsOwner)
        {
            var SNB = FindAnyObjectByType<SchizojackNetworkBackend>();
            var SB = FindAnyObjectByType<SchizojackBackend>();
            var CAM = actor.GetComponentInChildren<Camera>();

            SNB._localUserNumber = index;
            SB._localUserNumber = index;
            SB.localUserCamera = CAM;

            Debug.Log($"[CLIENT] Local user number: {index}");
        }
    }
}