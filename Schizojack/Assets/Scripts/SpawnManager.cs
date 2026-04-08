using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
public class SpawnManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

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
        nextSpawn++;

        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }
}
