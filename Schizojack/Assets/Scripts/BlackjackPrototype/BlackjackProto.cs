using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.Rendering.Universal;

public class BlackjackProto : MonoBehaviour
{

    // A = Spades, B = Hearts, C = Clubs, D = Diamonds
    List<Card> _baseCards = new List<Card>
    {
        new Card(2,"A"), new Card(2,"B"), new Card(2,"C"), new Card(2,"D"),
        new Card(3,"A"), new Card(3,"B"), new Card(3,"C"), new Card(3,"D"),
        new Card(4,"A"), new Card(4,"B"), new Card(4,"C"), new Card(4,"D"),
        new Card(5,"A"), new Card(5,"B"), new Card(5,"C"), new Card(5,"D"),
        new Card(6,"A"), new Card(6,"B"), new Card(6,"C"), new Card(6,"D"),
        new Card(7,"A"), new Card(7,"B"), new Card(7,"C"), new Card(7,"D"),
        new Card(8,"A"), new Card(8,"B"), new Card(8,"C"), new Card(8,"D"),
        new Card(9,"A"), new Card(9,"B"), new Card(9,"C"), new Card(9,"D"),
        new Card(10,"A"), new Card(10,"B"), new Card(10,"C"), new Card(10,"D"),
        new Card(11,"A"), new Card(11,"B"), new Card(11,"C"), new Card(11,"D"),
        new Card(10,"A",1), new Card(10,"B",1), new Card(10,"C",1), new Card(10,"D",1),
        new Card(10,"A",2), new Card(10,"B",2), new Card(10,"C",2), new Card(10,"D",2),
        new Card(10,"A",3), new Card(10,"B",3), new Card(10,"C",3), new Card(10,"D",3)
    };

    List<Card> _actualDeck = new List<Card>();

    public List<Card> playerDeck = new List<Card>();

    public TMP_Text handText;
    public TMP_Text deckText;
    public TMP_Text gameStateText;

    public Button hitCardButton;
    public Button shuffleButton;

    private bool _canShuffle = true;

    public GameObject CardPrefab;
    public GameObject playerHandGO;

    private int _playerDeckValue1 = 0; // Ace = 1
    private int _playerDeckValue2 = 0; // Ace = 11

    private bool _playerLost = false;
    private bool _playerWon = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResetDecks();
    }

    // Update is called once per frame
    void Update()
    {
        handText.text = CardsToString(playerDeck);
        deckText.text = CardsToString(_actualDeck);

        if (_playerLost)
        {
            hitCardButton.interactable = false;
        }
        if (_playerWon)
        {
            hitCardButton.interactable = false;
        }

        shuffleButton.interactable = _canShuffle;
    }

    public void UpdateGameState()
    {
        _playerDeckValue1 = 0; // Ace = 1
        _playerDeckValue2 = 0; // Ace = 11

        foreach (Card card in playerDeck) 
        {
            if (card.value == 11)
            {
                _playerDeckValue1 += 1;
                _playerDeckValue2 += 11;
            }
            else
            {
                _playerDeckValue1 += card.value;
                _playerDeckValue2 += card.value;
            }
        }

        if (_playerDeckValue1 > 21 && _playerDeckValue2 > 21)
        {
            gameStateText.text = $"Hand is over 21, you loose. Your hand is worth {_playerDeckValue1} with an Ace worth 1, and {_playerDeckValue2} with an Ace worth 11";
            _playerLost = true;
        }
        else if (_playerDeckValue1 == 21 || _playerDeckValue2 == 21)
        {
            gameStateText.text = $"You win, Your hand is worth {_playerDeckValue1} with an Ace worth 1, and {_playerDeckValue2} with an Ace worth 11";
            _playerWon = true;
        }
        else
        {
            gameStateText.text = $"Your hand is worth {_playerDeckValue1} with an Ace worth 1, and {_playerDeckValue2} with an Ace worth 11";
        }

        ShowPlayerHand();
    }

    public string CardsToString(List<Card> cards)
    {
        string output = "";

        if (cards.Count <= 0) return output;

        foreach (Card card in cards)
        {
            output += $"| {card.value} , {card.suit} |";
        }

        return output;
    }

    public List<Card> CardHit(List<Card> hand)
    {
        Card temp = _actualDeck.First();
        _actualDeck.Remove(temp);
        hand.Add(temp);
        return hand;
    }

    public List<Card> ShuffleDeck(List<Card> deck)
    {
        return deck.OrderBy(i => Guid.NewGuid()).ToList();
    }

    public void ShowPlayerHand()
    {
        if(playerHandGO.transform.childCount != 0)
        {
            foreach (Transform child in playerHandGO.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        
        if(playerDeck.Count > 0)
        {
            for (int i = 0; i < playerDeck.Count; i++)
            {
                float deckCount = playerDeck.Count;

                Vector3 tempPos = new Vector3((i - (deckCount - 1) / 2f) * 150f, 0, 0);
                GameObject temp = Instantiate(CardPrefab, new Vector3(0,0,0), Quaternion.identity, playerHandGO.transform);
                temp.GetComponent<RectTransform>().SetLocalPositionAndRotation(tempPos, Quaternion.identity);
                string path = "PaperCards/";
                // A = Spades, B = Hearts, C = Clubs, D = Diamonds
                switch (playerDeck[i].suit) {
                    case "A":
                        path += "Spades/Spades";
                        break;
                    case "B":
                        path += "Hearts/Hearts";
                        break;
                    case "C":
                        path += "Clubs/Clubs";
                        break;
                    case "D":
                        path += "Diamonds/Diamonds";
                        break;
                    default:
                        break;
                }
                // 0/Null = Number card, uses value for image, 1 = Jack, 2 = Queen, 3 = King
                switch (playerDeck[i].image)
                {
                    case 1:
                        path += "Jack";
                        break;
                    case 2:
                        path += "Queen";
                        break;
                    case 3:
                        path += "King";
                        break;
                    default:
                        if(playerDeck[i].value == 11)
                        {
                            path += "Ace";
                        }
                        else
                        {
                            path += $"{playerDeck[i].value}";
                        }
                        break;
                }
                temp.GetComponent<Image>().sprite = Resources.Load<Sprite>(path);
                temp.name = $"{playerDeck[i].value}{playerDeck[i].suit}{playerDeck[i].image}";
                Debug.Log(path);
            }
        }
    }

    // UI Functions
    public void PlayerHitUI()
    {
        _canShuffle = false;
        playerDeck = CardHit(playerDeck);
        UpdateGameState();
    }
    public void ShuffleDeckUI()
    {
        _actualDeck = ShuffleDeck(_actualDeck);
    }
    public void ResetDecks()
    {
        _actualDeck = new List<Card>(_baseCards);
        _actualDeck = ShuffleDeck(_actualDeck);
        playerDeck = new List<Card>();
        _playerLost = false;
        _playerWon = false;
        _canShuffle = true;
        hitCardButton.interactable = true;
        UpdateGameState();
    }
}

// Card Class
public class Card
{
    public int value { get; set; } // Value of card
    public string suit { get; set; } // A = Spades, B = Hearts, C = Clubs, D = Diamonds
    public int image { get; set; } // 0/Null = Number card, uses value for image, 1 = Jack, 2 = Queen, 3 = King
    public Card (int value, string suit)
    {
        this.value = value;
        this.suit = suit;
    }
    public Card(int value, string suit, int image)
    {
        this.value = value;
        this.suit = suit;
        this.image = image;
    }
}
