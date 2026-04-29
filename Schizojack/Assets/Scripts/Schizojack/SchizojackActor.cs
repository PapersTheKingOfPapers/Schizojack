using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SchizojackBackend;
using static System.Net.Mime.MediaTypeNames;

public class SchizojackActor : MonoBehaviour
{
    public Animator animator;

    public Transform cardTableParentObject;

    public Transform cardParentObject;

    public GameObject cardPrefab;

    public GameObject animatedCard;

    public TMP_Text monitorText;

    public List<Card> tempCards;
    public List<Card> tempSpecialCards;

    [HideInInspector] public SchizojackBackend _SB;

    public bool finishedAnimation;

    public void FinishedAnimation()
    {
        finishedAnimation = true;
    }

    public void StartBackupCoroutine()
    {
        StartCoroutine(BackupAnimationFinish());
    }

    public void StopBackupCoroutine()
    {
        StopCoroutine(BackupAnimationFinish());
        StopAllCoroutines();
    }

    public IEnumerator BackupAnimationFinish()
    {
        yield return new WaitForSeconds(5);
        FinishedAnimation();
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

        if (cardTableParentObject.childCount != 0)
        {
            foreach (Transform child in cardTableParentObject)
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

                Vector3 tempPos = new Vector3((i - (deckCount - 1) / 2f) * 0.075f, 0, 0);
                GameObject temp = Instantiate(cardPrefab, new Vector3(0, 0, 0), Quaternion.identity, cardTableParentObject.transform);
                temp.GetComponent<Transform>().SetLocalPositionAndRotation(tempPos, Quaternion.identity);
                temp.transform.localEulerAngles = new Vector3(0, 0, 0);
                if (i == 0)
                {
                    temp.transform.localEulerAngles = new Vector3(0,180,0);
                }

                string path = tempCards[i].cardTexturePath();
                if (tempCards[i].suit != "S" && gameObject.CompareTag($"Actor{_SB._localUserNumber}"))
                {
                    ItemDescription itemDes = temp.AddComponent<ItemDescription>();
                    itemDes.itemTitle = $"{tempCards[i].value}";
                    itemDes.itemDescription = $"{tempCards[i].cardFancyName()}";
                    itemDes.trumpCardType = -1;
                }

                temp.GetComponent<Renderer>().materials[1].enableInstancing = true;
                temp.GetComponent<Renderer>().materials[1].mainTexture = Resources.Load<Texture>(path);
                temp.name = $"{tempCards[i].value}{tempCards[i].suit}{tempCards[i].image}";
                tempText += $"{temp.name},";
                Debug.Log(path);
            }
            Debug.Log(tempText); 
        }

        if (tempSpecialCards.Count > 0)
        {
            string tempSpecialText = "TempSpecialCards : ";
            for (int i = 0; i < tempSpecialCards.Count; i++)
            {
                float deckCount = tempSpecialCards.Count;

                Vector3 tempRotation = new Vector3(90, 0, (i - (deckCount - 1) / 2f) * 10f);
                GameObject temp = Instantiate(cardPrefab, new Vector3(0, 0, 0), Quaternion.identity, cardParentObject.transform);
                temp.transform.localPosition = new Vector3(0, 0, 0);
                temp.transform.localEulerAngles = tempRotation;
                temp.transform.Rotate(temp.transform.up, -2f, Space.Self);
                //temp.GetComponent<RectTransform>().SetLocalPositionAndRotation(tempPos, Quaternion.identity);

                string path = tempSpecialCards[i].cardTexturePath();
                if (tempSpecialCards[i].suit == "S" && gameObject.CompareTag($"Actor{_SB._localUserNumber}"))
                {
                    ItemDescription itemDes = temp.AddComponent<ItemDescription>();
                    switch (tempSpecialCards[i].image)
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
                        case 6: //AddBet
                            itemDes.itemTitle = "Capacitor";
                            itemDes.itemDescription = "Add 1 to the current bet value. (Can be stacked)";
                            break;
                        case 7: //LowerBet
                            itemDes.itemTitle = "Resitor";
                            itemDes.itemDescription = "Remove 1 from the current bet value. (Can be stacked)";
                            break;
                        case 8: //CoolGuy
                            itemDes.itemTitle = "Really good friend";
                            itemDes.itemDescription = "\"He's a really cool guy, he's my bestest of friends, he gave us all these cool cards.\" Give everyone 1 Trump card.";
                            break;
                    }
                    itemDes.trumpCardType = tempSpecialCards[i].image;

                }

                temp.GetComponent<Renderer>().materials[1].enableInstancing = true;
                temp.GetComponent<Renderer>().materials[1].mainTexture = Resources.Load<Texture>(path);
                temp.name = $"{tempSpecialCards[i].value}{tempSpecialCards[i].suit}{tempSpecialCards[i].image}";
                temp.layer = LayerMask.NameToLayer("SpecialCardLayer");
                tempSpecialText += $"{temp.name},";
                Debug.Log(path);
            }
            Debug.Log(tempSpecialText);
        }
    }
}
