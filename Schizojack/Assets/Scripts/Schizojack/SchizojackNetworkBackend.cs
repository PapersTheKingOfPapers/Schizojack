using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static SchizojackBackend;

[RequireComponent(typeof(SchizojackBackend))]

public class SchizojackNetworkBackend : NetworkBehaviour
{
    private SchizojackBackend _backEnd;

    [HideInInspector] public int _localUserNumber = 0;

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
        ActorHitClientRpc(actorIndex, tempCard);
    }
    // Called by Server, sent to clients
    [Rpc(SendTo.NotServer)]
    public void ActorHitClientRpc(int actorIndex, Card card)
    {
        _backEnd.ActorHitClient(actorIndex, card);
    }
    [Rpc(SendTo.Everyone)]
    public void ActorStandRpc(int actorIndex)
    {
        _backEnd.ActorStand(actorIndex);
    }
}
