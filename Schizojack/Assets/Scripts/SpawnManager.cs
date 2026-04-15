using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
public class SpawnManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private SchizojackNetworkBackend _SNB;
    [SerializeField] private SchizojackBackend _SB;
    [SerializeField] private SchizojackActorFrontend _SAF;

    private int nextSpawn = 0;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
        }
    }
    private void SpawnPlayer(ulong clientId)
    {
        Transform spawnPoint = spawnPoints[nextSpawn % spawnPoints.Count];

        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        var initializer = player.GetComponent<PlayerInitializer>();
        initializer.PlayerIndex.Value = nextSpawn;

        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

        nextSpawn++;
    }
}
