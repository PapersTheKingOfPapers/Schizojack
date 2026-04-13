using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.Rendering.Universal;
using System.Collections;

[RequireComponent(typeof(SchizojackActorFrontend))]
public class SchizojackBackend : MonoBehaviour
{
    SchizojackActorFrontend _frontEnd;

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

    List<Actor> _actors = new List<Actor>();

    public TMP_Text handText;
    public TMP_Text deckText;
    public TMP_Text gameStateText;
    public TMP_Text actorStateText;

    public Button hitCardButton;
    public Button shuffleButton;

    private bool _canShuffle = true;

    private int _currentTurn = 0; //  Who's turn it is
    private bool _actorActedThisTurn = false; // If the current player (the one above this) has acted this round.

    public GameObject CardPrefab;
    public GameObject playerHandGO;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _frontEnd = this.GetComponent<SchizojackActorFrontend>();

        foreach (var actor in _frontEnd.Actors)
        {
            _actors.Add(new Actor());
        }

        ResetDecks();
    }

    // Update is called once per frame
    void Update()
    {
        if(_currentTurn > _actors.Count - 1)
        {
            _currentTurn = 0;
        }

        handText.text = CardsToString(_actors[0].actorDeck);
        deckText.text = CardsToString(_actualDeck);

        if (_frontEnd.Actors[_currentTurn].finishedAnimation == true)
        {
            _frontEnd.Actors[_currentTurn].finishedAnimation = false;
            _actorActedThisTurn = false;
            _currentTurn++;
        }

        if (_currentTurn > _actors.Count - 1)
        {
            _currentTurn = 0;
        }

        if (_currentTurn != 0)
        {
            if ((_actors[_currentTurn].actorLost == false || _actors[_currentTurn].actorWon == false) && _actorActedThisTurn == false)
            {
                _actors[_currentTurn].deckValueLow = DeckValue(_actors[_currentTurn].actorDeck, false);
                _actors[_currentTurn].deckValueHigh = DeckValue(_actors[_currentTurn].actorDeck, true);

                if (_actors[_currentTurn].deckValueLow > 21 && _actors[_currentTurn].deckValueHigh > 21)
                {
                    _actors[_currentTurn].actorLost = true;
                }
                else if (_actors[_currentTurn].deckValueLow == 21 || _actors[_currentTurn].deckValueHigh == 21)
                {
                    _actors[_currentTurn].actorWon = true;
                }

                if (_actors[_currentTurn].deckValueLow < 17 || _actors[_currentTurn].deckValueHigh < 17)
                {
                    ActorHit(_currentTurn);
                    _actorActedThisTurn = true;
                }
                else
                {
                    ActorStand(_currentTurn);
                    _actorActedThisTurn = true;
                }

                _actors[_currentTurn].deckValueLow = DeckValue(_actors[_currentTurn].actorDeck, false);
                _actors[_currentTurn].deckValueHigh = DeckValue(_actors[_currentTurn].actorDeck, true);
            }
        }
        else
        {
            UpdateGameState();
        }

        if (_actors[0].actorLost)
        {
            hitCardButton.interactable = false;
        }
        if (_actors[0].actorWon)
        {
            hitCardButton.interactable = false;
        }

        shuffleButton.interactable = _canShuffle;
    }

    public void UpdateGameState()
    {
        actorStateText.text = "";

        _actors[0].deckValueLow = DeckValue(_actors[0].actorDeck, false);
        _actors[0].deckValueHigh = DeckValue(_actors[0].actorDeck, true);

        if (_actors[0].deckValueLow > 21 && _actors[0].deckValueHigh > 21)
        {
            gameStateText.text = $"Hand is over 21, you loose. Your hand is worth {_actors[0].deckValueLow} with an Ace worth 1, and {_actors[0].deckValueHigh} with an Ace worth 11";
            _actors[0].actorLost = true;
        }
        else if (_actors[0].deckValueLow == 21 || _actors[0].deckValueHigh == 21)
        {
            gameStateText.text = $"You win, Your hand is worth {_actors[0].deckValueLow} with an Ace worth 1, and {_actors[0].deckValueHigh} with an Ace worth 11";
            _actors[0].actorWon = true;
        }
        else
        {
            gameStateText.text = $"Your hand is worth {_actors[0].deckValueLow} with an Ace worth 1, and {_actors[0].deckValueHigh} with an Ace worth 11";
        }

        for (int i = 0; i < _actors.Count; i++)
        {
            actorStateText.text += $"| Actor {i + 1}'s Hand Value : L{_actors[i].deckValueLow}/H{_actors[i].deckValueHigh} |";
        }

        //_frontEnd.UpdateActorHands(_actors);
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

    public int DeckValue(List<Card> deck, bool aceHigh)
    {
        int _deckValue = 0;
        foreach (Card card in deck)
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

    public void ActorHit(int actorIndex)
    {
        _actors[actorIndex].actorDeck = CardHit(_actors[actorIndex].actorDeck);
        _frontEnd.Actors[actorIndex].UpdateAnimatedCard(_actors[actorIndex].actorDeck.Last());
        _frontEnd.ActorHit(actorIndex, _actors[actorIndex].actorDeck);
        _actorActedThisTurn = true;
    }

    public void ActorStand(int actorIndex)
    {
        _frontEnd.ActorStand(actorIndex);
        _actorActedThisTurn = true;
    }

    // UI Functions
    public void PlayerHitUI()
    {
        _canShuffle = false;
        ActorHit(0);
        //ActorsPlayRound();
        UpdateGameState();
    }
    public void PlayerStandUI()
    {
        _canShuffle = false;
        ActorStand(0);
        //ActorsPlayRound();
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
        foreach (Actor actor in _actors)
        {
            actor.Reset();
        }
        _canShuffle = true;
        hitCardButton.interactable = true;
        _currentTurn = 0;
        UpdateGameState();
        _frontEnd.UpdateActorHands(_actors);
    }
}

// Card Class
public class Card
{
    public int value { get; set; } // Value of card
    public string suit { get; set; } // A = Spades, B = Hearts, C = Clubs, D = Diamonds
    public int image { get; set; } // 0/Null = Number card, uses value for image, 1 = Jack, 2 = Queen, 3 = King
    public Card(int value, string suit)
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

    public string cardTexturePath()
    {
        string path = "PaperCards/";

        // A = Spades, B = Hearts, C = Clubs, D = Diamonds
        switch (this.suit)
        {
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
        switch (this.image)
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
                if (this.value == 11)
                {
                    path += "Ace";
                }
                else
                {
                    path += $"{this.value}";
                }
                break;
        }

        return path;
    }
}

public class Actor
{
    public List<Card> actorDeck { get; set; }
    public bool actorLost { get; set; }
    public bool actorWon { get; set; }
    public int deckValueLow { get; set; }
    public int deckValueHigh { get; set; }
    public Actor()
    {
        this.actorDeck = new List<Card>();
        actorLost = false;
        actorWon = false;
        deckValueLow = 0;
        deckValueHigh = 0;
    }
    public Actor(List<Card> actorDeck)
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
