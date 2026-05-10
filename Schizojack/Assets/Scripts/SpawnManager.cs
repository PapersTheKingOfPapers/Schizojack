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
    private PlayerNameTransferer _PlayerNameTransferer;

    private int nextSpawn = 0;

    private string playerName = "";

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        _PlayerNameTransferer = FindAnyObjectByType<PlayerNameTransferer>();

        NetworkManager.SceneManager.OnLoadComplete += OnClientLoadedScene;

        /*foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            SpawnPlayer(client.ClientId);
        }

        NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;*/
    }
    private void OnClientLoadedScene(ulong clientId, string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (sceneName != "MainScene") return;

        SpawnPlayer(clientId);
    }
    private void SpawnPlayer(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
            return;

        Transform spawnPoint = spawnPoints[nextSpawn % spawnPoints.Count];
        
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

        var initializer = player.GetComponent<PlayerInitializer>(); 
        switch (nextSpawn)
        {
            case 0:
                playerName = _PlayerNameTransferer.player1.Value.ToString();
                break;
            case 1:
                playerName = _PlayerNameTransferer.player2.Value.ToString();
                break;
            case 2:
                playerName = _PlayerNameTransferer.player3.Value.ToString();
                break;
            case 3:
                playerName = _PlayerNameTransferer.player4.Value.ToString();
                break;
        }
        initializer.PlayerName.Value = playerName;
        initializer.PlayerIndex.Value = nextSpawn;

        nextSpawn++;
    }
}
