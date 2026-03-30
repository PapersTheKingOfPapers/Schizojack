using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

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
        new Card(12,"A"), new Card(12,"B"), new Card(12,"C"), new Card(12,"D"),
        new Card(13,"A"), new Card(13,"B"), new Card(13,"C"), new Card(13,"D"),
        new Card(14,"A"), new Card(14,"B"), new Card(14,"C"), new Card(14,"D")
    };

    List<Card> _actualDeck = new List<Card>();

    public List<Card> playerDeck = new List<Card>();

    public TMP_Text handText;
    public TMP_Text deckText;
    public TMP_Text gameStateText;

    public Button hitCardButton;
    public Button shuffleButton;

    private bool _canShuffle = true;

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
    public int value { get; set; }
    public string suit { get; set; }
    public Card (int value, string suit)
    {
        this.value = value;
        this.suit = suit;
    }
}
