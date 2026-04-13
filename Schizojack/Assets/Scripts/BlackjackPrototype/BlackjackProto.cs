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
    List<ProtoCard> _baseCards = new List<ProtoCard>
    {
        new ProtoCard(2,"A"), new ProtoCard(2,"B"), new ProtoCard(2,"C"), new ProtoCard(2,"D"),
        new ProtoCard(3,"A"), new ProtoCard(3,"B"), new ProtoCard(3,"C"), new ProtoCard(3,"D"),
        new ProtoCard(4,"A"), new ProtoCard(4,"B"), new ProtoCard(4,"C"), new ProtoCard(4,"D"),
        new ProtoCard(5,"A"), new ProtoCard(5,"B"), new ProtoCard(5,"C"), new ProtoCard(5,"D"),
        new ProtoCard(6,"A"), new ProtoCard(6,"B"), new ProtoCard(6,"C"), new ProtoCard(6,"D"),
        new ProtoCard(7,"A"), new ProtoCard(7,"B"), new ProtoCard(7,"C"), new ProtoCard(7,"D"),
        new ProtoCard(8,"A"), new ProtoCard(8,"B"), new ProtoCard(8,"C"), new ProtoCard(8,"D"),
        new ProtoCard(9,"A"), new ProtoCard(9,"B"), new ProtoCard(9,"C"), new ProtoCard(9,"D"),
        new ProtoCard(10,"A"), new ProtoCard(10,"B"), new ProtoCard(10,"C"), new ProtoCard(10,"D"),
        new ProtoCard(11,"A"), new ProtoCard(11,"B"), new ProtoCard(11,"C"), new ProtoCard(11,"D"),
        new ProtoCard(10,"A",1), new ProtoCard(10,"B",1), new ProtoCard(10,"C",1), new ProtoCard(10,"D",1),
        new ProtoCard(10,"A",2), new ProtoCard(10,"B",2), new ProtoCard(10,"C",2), new ProtoCard(10,"D",2),
        new ProtoCard(10,"A",3), new ProtoCard(10,"B",3), new ProtoCard(10,"C",3), new ProtoCard(10,"D",3)
    };

    List<ProtoCard> _actualDeck = new List<ProtoCard>();

    List<ProtoActor> _actors = new List<ProtoActor>();

    public List<ProtoCard> playerDeck = new List<ProtoCard>();

    public TMP_Text handText;
    public TMP_Text deckText;
    public TMP_Text gameStateText;
    public TMP_Text actorStateText;

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
        _actors.Add(new ProtoActor());
        _actors.Add(new ProtoActor());
        _actors.Add(new ProtoActor());
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
        actorStateText.text = "";

        _playerDeckValue1 = DeckValue(playerDeck, false); // Ace Low - Ace = 1
        _playerDeckValue2 = DeckValue(playerDeck, true); // Ace High - Ace = 11

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

        for (int i = 0; i < _actors.Count; i++)
        {
            actorStateText.text += $"| ProtoActor {i + 1}'s Hand Value : L{_actors[i].deckValueLow}/H{_actors[i].deckValueHigh} |";
        }

        ShowPlayerHand();
    }

    public string CardsToString(List<ProtoCard> cards)
    {
        string output = "";

        if (cards.Count <= 0) return output;

        foreach (ProtoCard card in cards)
        {
            output += $"| {card.value} , {card.suit} |";
        }

        return output;
    }

    public List<ProtoCard> CardHit(List<ProtoCard> hand)
    {
        ProtoCard temp = _actualDeck.First();
        _actualDeck.Remove(temp);
        hand.Add(temp);
        return hand;
    }

    public List<ProtoCard> ShuffleDeck(List<ProtoCard> deck)
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

    public void ActorsPlayRound() // Bots
    {
        foreach(ProtoActor actor in _actors)
        {
            if(actor.actorLost == false || actor.actorWon == false)
            {
                actor.deckValueLow = DeckValue(actor.actorDeck, false);
                actor.deckValueHigh = DeckValue(actor.actorDeck, true);

                if (actor.deckValueLow > 21 && actor.deckValueHigh > 21)
                {
                    actor.actorLost = true;
                }
                else if (actor.deckValueLow == 21 || actor.deckValueHigh == 21)
                {
                    actor.actorWon = true;
                }

                if(actor.deckValueLow < 17 || actor.deckValueHigh < 17)
                {
                    actor.actorDeck = CardHit(actor.actorDeck);
                }

                actor.deckValueLow = DeckValue(actor.actorDeck, false);
                actor.deckValueHigh = DeckValue(actor.actorDeck, true);
            }
        }
    }

    public int DeckValue(List<ProtoCard> deck, bool aceHigh)
    {
        int _deckValue = 0;
        foreach (ProtoCard card in deck)
        {
            if (card.value == 11)
            {
                if (aceHigh)
                {
                    _deckValue += 11;
                }
                else
                {
                    _deckValue += 1;
                }
            }
            else
            {
                _deckValue += card.value;
            }
        }
        return _deckValue;
    }

    // UI Functions
    public void PlayerHitUI()
    {
        _canShuffle = false;
        playerDeck = CardHit(playerDeck);
        ActorsPlayRound();
        UpdateGameState();
    }
    public void PlayerStandUI()
    {
        _canShuffle = false;
        ActorsPlayRound();
        UpdateGameState();
    }

    public void ShuffleDeckUI()
    {
        _actualDeck = ShuffleDeck(_actualDeck);
    }
    public void ResetDecks()
    {
        _actualDeck = new List<ProtoCard>(_baseCards);
        _actualDeck = ShuffleDeck(_actualDeck);
        playerDeck.Clear();
        foreach (ProtoActor actor in _actors)
        {
            actor.Reset();
        }
        _playerLost = false;
        _playerWon = false;
        _canShuffle = true;
        hitCardButton.interactable = true;
        UpdateGameState();
    }
}

// ProtoCard Class
public class ProtoCard
{
    public int value { get; set; } // Value of card
    public string suit { get; set; } // A = Spades, B = Hearts, C = Clubs, D = Diamonds
    public int image { get; set; } // 0/Null = Number card, uses value for image, 1 = Jack, 2 = Queen, 3 = King
    public ProtoCard (int value, string suit)
    {
        this.value = value;
        this.suit = suit;
    }
    public ProtoCard(int value, string suit, int image)
    {
        this.value = value;
        this.suit = suit;
        this.image = image;
    }
}

public class ProtoActor
{
    public List<ProtoCard> actorDeck { get; set; }
    public bool actorLost { get; set; }
    public bool actorWon { get; set; }
    public int deckValueLow { get; set; }
    public int deckValueHigh { get; set; }
    public ProtoActor()
    {
        this.actorDeck = new List<ProtoCard>();
        actorLost = false;
        actorWon = false;
        deckValueLow = 0;
        deckValueHigh = 0;
    }
    public ProtoActor (List<ProtoCard> actorDeck)
    {
        this.actorDeck = actorDeck;
        actorLost = false;
        actorWon = false;
        deckValueLow = 0;
        deckValueHigh = 0;
    }
    public void Reset()
    {
        actorDeck.Clear();
        actorLost = false;
        actorWon = false;
        deckValueLow = 0;
        deckValueHigh = 0;
    }
}
