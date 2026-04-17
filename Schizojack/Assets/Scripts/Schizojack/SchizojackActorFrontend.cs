using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SchizojackBackend;

public class SchizojackActorFrontend : MonoBehaviour
{

    public List<SchizojackActor> Actors = new List<SchizojackActor>();
    public void ActorHit(int actorIndex, List<Card> cards)
    {
        Actors[actorIndex].tempCards = new List<Card>(cards);
        Actors[actorIndex].animator.SetTrigger("HitCard");
    }

    public void ActorTakeCard(int actorIndex, List<Card> cards)
    {
        
        Actors[actorIndex].animator.SetTrigger("PickupCard");
    }

    public void ActorStand(int actorIndex)
    {
        Actors[actorIndex].animator.SetTrigger("StandCard");
    }

    public void ActorThrow(int actorIndex)
    {
        Actors[actorIndex].animator.SetTrigger("ThrowCard");
    }

    public void UpdateActorHands(List<Actor> backEndActors)
    {
        for (int i = 0; i < Actors.Count; i++)
        {
            Actors[i].tempCards = new List<Card>(backEndActors[i].actorDeck);
            Actors[i].tempCards.AddRange(backEndActors[i].actorSpecialDeck);
            Actors[i].UpdateCards();
        }
    }
}
