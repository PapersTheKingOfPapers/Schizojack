using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using static SchizojackBackend;

[RequireComponent(typeof(SchizojackBackend))]

public class SchizojackNetworkBackend : NetworkBehaviour
{
    private SchizojackBackend _backEnd;

    [HideInInspector] public int _localUserNumber = 0;

    //public GameObject startGameButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _backEnd = this.GetComponent<SchizojackBackend>();
        /*if (!IsHost)
        {
            //startGameButton.SetActive(false);
        }*/
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        NetworkManager.Singleton.OnClientDisconnectCallback += KillClientFunction;
    }

    public void KillClientFunction(ulong clientId)
    {
        foreach(Actor actor in _backEnd._actors)
        {
            if(actor.clientId == clientId)
            {
                actor.actorDead = true;
            }
        }
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
        //startGameButton.SetActive(false);
        Debug.Log("Started Session");
    }
    [Rpc(SendTo.Everyone)]
    public void FinishCurrentRoundRpc()
    {
        _backEnd.FinishCurrentRound();
        Debug.Log("Finishing Round");
    }
    [Rpc(SendTo.Everyone)]
    public void StartNewRoundRpc()
    {
        _backEnd.StartNewRound();
        _backEnd.baseDamageThisRound++;
        Debug.Log("Starting New Round");
    }

    [Rpc(SendTo.Everyone)]
    public void SessionOverRpc(int winnerIndex)
    {
        _backEnd.SessionOver(winnerIndex);
        Debug.Log("Session Over RPC");
    }

    [Rpc(SendTo.Server)]
    public void ActorTrumpCardUseRequestRpc(int actorIndex, int type, int targetIndex)
    {
        _backEnd.TrumpCardUsage(actorIndex, type, targetIndex);
    }
    [Rpc(SendTo.Everyone)]
    public void ActorTrumpCardUseEveryoneRpc(int actorIndex, int type, int targetIndex, string text)
    {
        _backEnd.TrumpCardUsageClient(actorIndex, type, targetIndex, text);
    }
    [Rpc(SendTo.Everyone)]
    public void GiveTrumpCardRpc(int actorIndex, int type)
    {
        _backEnd.GiveTrumpCard(actorIndex, type);
    }
}
