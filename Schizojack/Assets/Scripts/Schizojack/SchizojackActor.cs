using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;
using static SchizojackBackend;

public class SchizojackActor : MonoBehaviour
{
    public Animator animator;

    public Transform cardParentObject;

    public GameObject cardPrefab;

    public GameObject animatedCard;

    [HideInInspector] public bool finishedAnimation;

    [HideInInspector] public List<Card> tempCards;

    public void FinishedAnimation()
    {
        finishedAnimation = true;
    }

    public void ToggleAnimatedCard(int value)
    {
        animatedCard.SetActive(Convert.ToBoolean(value));
    }

    public void UpdateAnimatedCard(Card card)
    {
        string path = card.cardTexturePath();

        animatedCard.GetComponent<Renderer>().materials[0].enableInstancing = true;
        animatedCard.GetComponent<Renderer>().materials[0].mainTexture = Resources.Load<Texture>(path);
    }

    public void UpdateCards()
    {
        if (cardParentObject.childCount != 0)
        {
            foreach (Transform child in cardParentObject)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        if (tempCards.Count > 0)
        {
            for (int i = 0; i < tempCards.Count; i++)
            {
                float deckCount = tempCards.Count;

                Vector3 tempRotation = new Vector3(90, 0, (i - (deckCount - 1) / 2f) * 10f);
                GameObject temp = Instantiate(cardPrefab, new Vector3(0, 0, 0), Quaternion.identity, cardParentObject.transform);
                temp.transform.localPosition = new Vector3(0, 0, 0);
                temp.transform.localEulerAngles = tempRotation;
                temp.transform.Rotate(temp.transform.up, -2f);
                //temp.GetComponent<RectTransform>().SetLocalPositionAndRotation(tempPos, Quaternion.identity);

                string path = tempCards[i].cardTexturePath();

                temp.GetComponent<Renderer>().materials[1].enableInstancing = true;
                temp.GetComponent<Renderer>().materials[1].mainTexture = Resources.Load<Texture>(path);
                temp.name = $"{tempCards[i].value}{tempCards[i].suit}{tempCards[i].image}";
                Debug.Log(path);
            }
        }
    }
}
