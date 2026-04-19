using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;
using static SchizojackBackend;
using static System.Net.Mime.MediaTypeNames;

public class SchizojackActor : MonoBehaviour
{
    public Animator animator;

    public Transform cardParentObject;

    public GameObject cardPrefab;

    public GameObject animatedCard;

    public TMP_Text monitorText;

    public List<Card> tempCards;

    [HideInInspector] public SchizojackBackend _SB;
    
    public bool finishedAnimation;

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
            string tempText = "TempCards : ";
            for (int i = 0; i < tempCards.Count; i++)
            {
                float deckCount = tempCards.Count;

                Vector3 tempRotation = new Vector3(90, 0, (i - (deckCount - 1) / 2f) * 5f);
                GameObject temp = Instantiate(cardPrefab, new Vector3(0, 0, 0), Quaternion.identity, cardParentObject.transform);
                temp.transform.localPosition = new Vector3(0, 0, 0);
                temp.transform.localEulerAngles = tempRotation;
                temp.transform.Rotate(temp.transform.up, -2f);
                //temp.GetComponent<RectTransform>().SetLocalPositionAndRotation(tempPos, Quaternion.identity);

                string path = tempCards[i].cardTexturePath();
                if (tempCards[i].suit == "S" && gameObject.CompareTag($"Actor{_SB._localUserNumber}"))
                {
                    ItemDescription itemDes = temp.AddComponent<ItemDescription>();
                    switch (tempCards[i].image)
                    {
                        case 0: //Reset21
                            itemDes.itemTitle = "Reset 21";
                            itemDes.itemDescription = "Changes the target value being played to back to 21 if it was altered.";
                            break;
                        case 1: //To27
                            itemDes.itemTitle = "Play to 27";
                            itemDes.itemDescription = "Changes the target value being played to 27.";
                            break;
                        case 2: //To17
                            itemDes.itemTitle = "Play to 17";
                            itemDes.itemDescription = "Changes the target value being played to 17.";
                            break;
                        case 3: //RoundReset
                            itemDes.itemTitle = "Re-Round";
                            itemDes.itemDescription = "Restarts the current round, and does not give trump cards.";
                            break;
                        case 4: //Peek
                            itemDes.itemTitle = "Peek";
                            itemDes.itemDescription = "Peek what the next card from the deck will be.";
                            break;
                        case 5: //Survivor
                            itemDes.itemTitle = "Survivor";
                            itemDes.itemDescription = "No one will die this round if they would have.";
                            break;
                    }
                    itemDes.trumpCardType = tempCards[i].image;

                }

                temp.GetComponent<Renderer>().materials[1].enableInstancing = true;
                temp.GetComponent<Renderer>().materials[1].mainTexture = Resources.Load<Texture>(path);
                temp.name = $"{tempCards[i].value}{tempCards[i].suit}{tempCards[i].image}";
                tempText += $"{temp.name},";
                Debug.Log(path);
            }
            Debug.Log(tempText); 
        }
    }
}
