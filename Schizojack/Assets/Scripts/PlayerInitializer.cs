using System;
using System.Globalization;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.ProBuilder;

public class PlayerInitializer : NetworkBehaviour
{
    public NetworkVariable<int> PlayerIndex = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<FixedString64Bytes> PlayerName = new NetworkVariable<FixedString64Bytes>(
        "Temporary",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool _initialized = false;
    public override void OnNetworkSpawn()
    {
        if (!IsClient) return;

        PlayerIndex.OnValueChanged += OnIndexChanged;
        PlayerName.OnValueChanged += OnPlayerNameChanged;

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
        actor.actorName = PlayerName.Value.ToString();

        Debug.Log($"Registering actor {gameObject.tag} with index {index}");

        if (!SAF.Actors.Contains(actor))
        {
            SAF.Actors.Add(actor);
        }

        if (IsOwner)
        {
            var SNB = FindAnyObjectByType<SchizojackNetworkBackend>();
            var SB = FindAnyObjectByType<SchizojackBackend>();
            var CAM = actor.GetComponentInChildren<Camera>();

            SNB._localUserNumber = index;
            SB._localUserNumber = index;
            SB.localUserCamera = CAM;

            Debug.Log($"[CLIENT] Local user number: {index}");

            SNB.ClientReadyServerRpc();
        }
    }

    private void OnPlayerNameChanged(FixedString64Bytes oldVal, FixedString64Bytes newVal)
    {
        var actor = GetComponentInChildren<SchizojackActor>();
        actor.actorName = PlayerName.Value.ToString();
    }
}