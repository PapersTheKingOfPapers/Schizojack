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
        Actors[actorIndex].StartBackupCoroutine();
    }
    public void ActorTakeCard(int actorIndex, List<Card> cards)
    {
        
        Actors[actorIndex].animator.SetTrigger("PickupCard");
        Actors[actorIndex].StartBackupCoroutine();
    }

    public void ActorStand(int actorIndex)
    {
        Actors[actorIndex].animator.SetTrigger("StandCard");
        Actors[actorIndex].StartBackupCoroutine();
    }

    public void ActorThrow(int actorIndex)
    {
        Actors[actorIndex].animator.SetTrigger("ThrowCard");
        Actors[actorIndex].StartBackupCoroutine();
    }

    public void ActorTakeDamage(int actorIndex)
    {
        //Actors[actorIndex].animator.SetTrigger("TakeDamage");
    }

    public void ActorDieAnimation(int actorIndex)
    {
        //Actors[actorIndex].animator.SetBool("ActorDead", true);
    }

    public void UpdateActorHands(List<Actor> backEndActors)
    {
        for (int i = 0; i < Actors.Count; i++)
        {
            Actors[i].tempCards = new List<Card>(backEndActors[i].actorDeck);
            Actors[i].tempSpecialCards = new List<Card>(backEndActors[i].actorSpecialDeck);
            Actors[i].UpdateCards();
        }
    }
    public void UpdateActorDeck(List<Actor> backEndActors)
    {
        for (int i = 0; i < Actors.Count; i++)
        {
            Actors[i].tempCards = new List<Card>(backEndActors[i].actorDeck);
            Actors[i].tempSpecialCards = new List<Card>(backEndActors[i].actorSpecialDeck);
        }
    }
}
