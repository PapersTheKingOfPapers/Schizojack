using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static SchizojackBackend;

[RequireComponent(typeof(SchizojackBackend))]

public class SchizojackNetworkBackend : NetworkBehaviour
{
    private SchizojackBackend _backEnd;

    [HideInInspector] public int _localUserNumber = 0;

    public GameObject startGameButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _backEnd = this.GetComponent<SchizojackBackend>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Rpc(SendTo.Everyone)]
    public void ResetDecksRpc()
    {
        _backEnd.ResetDecks();
    }
    // Called by Client
    [Rpc(SendTo.Server)]
    public void ActorHitRequestRpc(int actorIndex)
    {
        _backEnd.ActorHit(actorIndex);
        Card tempCard = _backEnd.ActorLatestCard(actorIndex);
        int value = tempCard.value;
        string suit = tempCard.suit;
        int image = tempCard.image;
        ActorHitClientRpc(actorIndex, value, suit, image);
        Debug.Log("Client Called Hit");
    }
    // Called by Server, sent to clients
    [Rpc(SendTo.NotServer)]
    public void ActorHitClientRpc(int actorIndex, int value, string suit, int image)
    {
        Card card = new Card(value, suit, image);
        _backEnd.ActorHitClient(actorIndex, card);
        Debug.Log("Server Sent Hit");
    }
    [Rpc(SendTo.Everyone)]
    public void ActorStandRpc(int actorIndex)
    {
        _backEnd.ActorStand(actorIndex);
    }

    [Rpc(SendTo.Everyone)]
    public void StartSessionRpc()
    {
        _backEnd.InitializeActors();
        startGameButton.SetActive(false);
        Debug.Log("Started Session");
    }
}
