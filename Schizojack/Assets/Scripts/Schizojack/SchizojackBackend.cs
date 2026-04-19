using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.Hardware;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using static Unity.Collections.AllocatorManager;

[RequireComponent(typeof(SchizojackActorFrontend))]
[RequireComponent(typeof(SchizojackNetworkBackend))]

public class SchizojackBackend : MonoBehaviour
{
    SchizojackActorFrontend _frontEnd;
    SchizojackNetworkBackend _networkBackEnd;

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

    List<Card> _specialCards = new List<Card>
    {
        new Card("S", 0), new Card("S", 1), new Card("S", 2), new Card("S", 3),
        new Card("S", 4), new Card("S", 5)
    };

    List<Card> _actualDeck = new List<Card>();

    List<Actor> _actors = new List<Actor>();

    public TMP_Text handText;
    public TMP_Text deckText;
    public TMP_Text gameStateText;
    public TMP_Text actorStateText;
    public TMP_Text trumpCardReturnText;

    public Button hitCardButton;
    public Button shuffleButton;

    private bool _canShuffle = true;

    // private bool[] _botActors = { false, true, true, true }; // 0 is always false, for it is the host.
                                                             // 1,2,3 can be either true or false, depending
                                                             // on how many players there are.
                                                             // Un-used for the time being.

    private int _currentTurn = 0; //  Who's turn it is

    public Camera localUserCamera;
    [HideInInspector] public int _localUserNumber = 0;

    private bool _actorActedThisTurn = false; // If the current player (the one above this) has acted this round.

    private bool _actorTurnFinalizer = false; // If the action the player took this turn was a finalizer (Stand, Hit), since using a trump
                                              // card may not be a finalizer (Raise or lower target, Peek, Nice fella, and so on..).

    public bool sessionStarted = false;

    [HideInInspector] public int blackjackTarget = 21;

    [HideInInspector] public int standsInARow = 0;

    [HideInInspector] public int baseDamageThisRound = 2;
    [HideInInspector] public int damageThisRoundAddition = 0;
    [HideInInspector] public int damageThisRound = 0;

    private bool _roundFinishState = false;
    private bool _roundFinishRan = false;
    public bool _trumpCardUsed = false;

    [HideInInspector] public bool sessionFinished = false;

    [SerializeField] private InputActionAsset _inputActionAssets;

    private bool _cantDieThisRound = false;

    private InputAction _cardHit;
    private InputAction _cardStand;

    public GameObject CardPrefab;
    public GameObject playerHandGO;

    private LayerMask layerMask;

    [HideInInspector] public int trumpActorIndex;
    [HideInInspector] public int trumpCardType;
    [HideInInspector] public int trumpTargetIndex;
    [HideInInspector] public string trumpText;

    private void Awake()
    {
        layerMask = LayerMask.GetMask("Default");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _frontEnd = this.GetComponent<SchizojackActorFrontend>();
        _networkBackEnd = this.GetComponent<SchizojackNetworkBackend>();

        //Inputs
        _cardHit = _inputActionAssets.FindAction("Player/CardHit");
        _cardHit.Enable();

        _cardStand = _inputActionAssets.FindAction("Player/CardStand");
        _cardStand.Enable();

        _cardHit.performed += _ => NetworkActorHit();
        _cardStand.performed += _ => NetworkActorStand();
    }

    private void OnDisable()
    {
        _cardHit.Disable();
        _cardStand.Disable();
    }

    public void InitializeActors()
    {
        foreach (var actor in _frontEnd.Actors)
        {
            _actors.Add(new Actor());
            actor._SB = this;
        }
        
        ResetDecks();
        StartNewRound();
        sessionStarted = true;
        deckText.gameObject.SetActive(false);
    }

    public bool IsLocalActorTurn()
    {
        return _currentTurn == _localUserNumber;
    }
    public bool IsHost()
    {
        return _localUserNumber == 0;
    }

    // Update is called once per frame
    void Update()
    {
        damageThisRound = baseDamageThisRound + damageThisRoundAddition;

        if(sessionStarted == true && _roundFinishState == false && sessionFinished == false)
        {
            // Reset the current turn if _currentTurn is over the amount of actors present.
            FixCurrentTurnCycle();

            UseTrumpCardLoop();

            // Debug text for host.
            handText.text = CardsToString(_actors[_localUserNumber].actorDeck);
            deckText.text = CardsToString(_actualDeck);

            while (_actors[_currentTurn].actorDead) {
                _currentTurn++;
                FixCurrentTurnCycle();
            }
             
            // If animation finished for current actor, and they did a finalizing action, move to next actor.
            if (_frontEnd.Actors[_currentTurn].finishedAnimation == true && _actorTurnFinalizer == true)
            {
                _actorActedThisTurn = false;
                _frontEnd.Actors[_currentTurn].finishedAnimation = false;
                _currentTurn++;
                _actorTurnFinalizer = false;
            }
            // If animation finished for current actor, but they used a non-finalizing action.
            else if (_frontEnd.Actors[_currentTurn].finishedAnimation == true)
            {
                _frontEnd.Actors[_currentTurn].finishedAnimation = false;
                _actorActedThisTurn = false;
            }

            FixCurrentTurnCycle();

            // Bot Functions

            /*if (_currentTurn != 0)
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
            }*/

            UpdateGameState();

            if(IsHost())
            {
                OwnerUpdateCycle();
            }

            // Host's buttons.
            if (_actors[_localUserNumber].actorLost)
            {
                hitCardButton.interactable = false;
            }
            if (_actors[_localUserNumber].actorWon)
            {
                hitCardButton.interactable = false;
            }

            shuffleButton.interactable = _canShuffle;
        }
        else if(_roundFinishState == true && sessionFinished == false && _roundFinishRan == false)
        {
            _roundFinishRan = true;
            //Check Winners
            foreach (Actor actor in _actors)
            {
                actor.CalculateDeckValues();
                if (actor.DeckValueIsTarget(blackjackTarget))
                {
                    actor.actorWon = true;
                }
            }
            //Take Damage
            foreach (Actor actor in _actors)
            {
                if (_actors.Count(x => x.actorWon) != 0)
                {
                    if (!actor.DeckValueIsTarget(blackjackTarget))
                    {
                        actor.actorLost = true;
                        if (_cantDieThisRound == false)
                        {
                            actor.AddDamage(damageThisRound);
                            if (actor.actorDamaged >= 10)
                            {
                                actor.actorDead = true;
                            }
                        }
                        else
                        {
                            int healthAfterDamage = actor.actorDamaged + damageThisRound;
                            if (healthAfterDamage >= 10)
                            {
                                actor.SetDamaged(9);
                            }
                            else
                            {
                                actor.AddDamage(damageThisRound);
                            }
                        }
                    }
                }
            }

            if(_cantDieThisRound == true)
                _cantDieThisRound = false;

            UpdateGameState();
            for (int i = 0; i < _actors.Count; i++)
                Debug.Log($"Actor{i}'s stats : DamageTaken = {_actors[i].actorDamaged}, ActorLost = {_actors[i].actorLost}, ActorWon = {_actors[i].actorWon}, ActorDead = {_actors[i].actorDead}");

            //Session over check
            if (_actors.Count(x => x.actorWon) == 1 && _actors.Count(x => x.actorDead) == _actors.Count-1)
            {
                sessionFinished = true;
                int winnerIndex = _actors.FindIndex(a => a.actorWon);
                Debug.Log($"Session Finished Bool : {sessionFinished}");
                _networkBackEnd.SessionOverRpc(winnerIndex);
            }

            if(sessionFinished == false && IsHost())
            {
                _networkBackEnd.StartNewRoundRpc();
            }
        }
    }

    private void OwnerUpdateCycle()
    {
        if (_roundFinishState == false && _roundFinishRan == false)
        {
            if (standsInARow == _actors.Count)
            {
                _networkBackEnd.FinishCurrentRoundRpc();
            }
        }
    }
    private void ClientUpdateCycle()
    {

    }

    private void FixCurrentTurnCycle()
    {
        // Reset the current turn if _currentTurn is over the amount of actors present.
        if (_currentTurn > _actors.Count - 1)
        {
            _currentTurn = 0;
        }
    }

    public void FinishCurrentRound()
    {
        _currentTurn = 0;
        standsInARow = 0;
        _roundFinishState = true;
    }
    public void StartNewRound()
    {
        _currentTurn = 0;

        RoundDeckReset();

        if (IsHost())
        {
            for (int i = 0; i < _actors.Count; i++)
            {
                Card trump1 = new Card(_specialCards[UnityEngine.Random.Range(0, _specialCards.Count)]);
                Card trump2 = new Card(_specialCards[UnityEngine.Random.Range(0, _specialCards.Count)]);

                _networkBackEnd.GiveTrumpCardRpc(i, trump1.image);
                _networkBackEnd.GiveTrumpCardRpc(i, trump2.image);
            }
        }

        ChangeBlackjackTarget(21); 
        standsInARow = 0;
        _actorActedThisTurn = false;
        _actorTurnFinalizer = false;
        _roundFinishState = false;
        _roundFinishRan = false;
    }
    
    public void GiveTrumpCard(int actorIndex, int type)
    {
        _actors[actorIndex].actorSpecialDeck.Add(new Card("S", type));
        _frontEnd.UpdateActorHands(_actors);
    }

    public void RequestTrumpCardUsage(int actorIndex, int cardType, int targetIndex)
    {
        _networkBackEnd.ActorTrumpCardUseRequestRpc(actorIndex, cardType, targetIndex);
    }

    public void TrumpCardUsage(int actorIndex, int cardType, int targetIndex)
    {
        switch (cardType)
        {
            default:
                _networkBackEnd.ActorTrumpCardUseEveryoneRpc(actorIndex, cardType, targetIndex, "");
                break;
            case 4:
                _networkBackEnd.ActorTrumpCardUseEveryoneRpc(actorIndex, cardType, targetIndex, _actualDeck.First().cardFancyName());
                break;

        }
    }

    public void UseTrumpCardLoop()
    {   
        if(_frontEnd.Actors[_currentTurn].finishedAnimation == true && _trumpCardUsed == true)
        {
            switch (trumpCardType)
            {
                case 0: //Reset21
                    ChangeBlackjackTarget(21);
                    break;
                case 1: //To27
                    ChangeBlackjackTarget(27);
                    break;
                case 2: //To17
                    ChangeBlackjackTarget(17);
                    break;
                case 3: //RoundReset
                    RoundDeckReset();
                    TrumpCardReturnText($"Actor{trumpActorIndex} has restart the current round!");
                    _currentTurn = 0;
                    break;
                case 4: //Peek
                    if (IsLocalActorTurn())
                    {
                        TrumpCardReturnText($"Next card in the deck is {trumpText}.");
                    }
                    break;
                case 5: //Survivor
                    _cantDieThisRound = true;
                    break;
            }
            _trumpCardUsed = false;
            _frontEnd.Actors[_currentTurn].finishedAnimation = false;
        }
    }

    public void TrumpCardUsageClient(int actorIndex, int cardType, int targetIndex, string text)
    {
        _frontEnd.Actors[actorIndex].UpdateAnimatedCard(new Card("S", cardType));
        _frontEnd.ActorThrow(actorIndex);
        trumpActorIndex = actorIndex;
        trumpCardType = cardType;
        trumpTargetIndex = targetIndex;
        trumpText = text;
        _trumpCardUsed = true;
        _actors[actorIndex].actorSpecialDeck.Remove(
            _actors[actorIndex].actorSpecialDeck.Find(c => c.suit == "S" && c.image == cardType)
            );
        _frontEnd.UpdateActorDeck(_actors);
    }

    public void SessionOver(int winnerIndex)
    {
        sessionFinished = true;
        gameStateText.text = $"Actor{winnerIndex} is the winner and last man standing! Session is over. Please contact event hoster to restart game.";
    }

    public void ChangeBlackjackTarget(int newTarget)
    {
        blackjackTarget = newTarget;
    }

    public void UpdateGameState()
    {
        actorStateText.text = "";

        var actor = _actors[_localUserNumber];
        actor.CalculateDeckValues();

        // Convert deck values to readable string
        string valuesText = string.Join(", ", actor.deckValues);

        // Local actor text
        if (actor.DeckValueOverTarget(blackjackTarget))
        {
            gameStateText.text = $"Hand is over {blackjackTarget}. Your hand is worth {valuesText}";
        }
        else if (actor.DeckValueIsTarget(blackjackTarget))
        {
            gameStateText.text = $"You win! Your hand is worth {valuesText}";
        }
        else
        {
            gameStateText.text = $"Your hand is worth {valuesText}, your current damage level is {actor.actorDamaged}. Current deck value target is {blackjackTarget}.";
        }

        // Debug text above screen
        for (int i = 0; i < _actors.Count; i++)
        {
            _actors[i].CalculateDeckValues();
            string actorValues = string.Join("/", _actors[i].deckValues);
            _frontEnd.Actors[i].monitorText.text = $"Damaged: {_actors[i].actorDamaged}\r\nBet: {damageThisRound}\r\nTarget: {blackjackTarget}\r\nDeck Value: {actorValues}\r\n";
            actorStateText.text += $"| Actor {i + 1}'s Hand Value: {actorValues} |";
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

    // Backend Functions, used to call the Network Backend

    public void NetworkActorHit() // Also used for selecting trump cards.
    {
        if (_actorActedThisTurn == false && IsLocalActorTurn() && sessionStarted == true && sessionFinished == false)
        {
            RaycastHit hit;
            Ray ray = localUserCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("TableLayer"))
                {
                    _networkBackEnd.ActorHitRequestRpc(_localUserNumber);
                }
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("CardLayer") && hit.transform.TryGetComponent(out ItemDescription itemDescription))
                {
                    _networkBackEnd.ActorTrumpCardUseRequestRpc(_localUserNumber,itemDescription.trumpCardType,0);
                }
            }
        }
    }
    public void NetworkActorStand()
    {
        if (_actorActedThisTurn == false && IsLocalActorTurn() && sessionStarted == true && sessionFinished == false)
        {
            RaycastHit hit;
            Ray ray = localUserCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("TableLayer"))
                {
                    _networkBackEnd.ActorStandRpc(_localUserNumber);
                }
            }
        }
    }

    // Host Functions, Called by the Schizojack Backend on the host's side on the request of the Network Manager Backend.

    public void ActorHit(int actorIndex)
    {
        _actors[actorIndex].actorDeck = CardHit(_actors[actorIndex].actorDeck);
        _frontEnd.Actors[actorIndex].UpdateAnimatedCard(_actors[actorIndex].actorDeck.Last());
        _frontEnd.ActorHit(actorIndex, _actors[actorIndex].actorDeck);
        _frontEnd.UpdateActorDeck(_actors);
        TrumpCardReturnText("");
        _actorActedThisTurn = true;
        _actorTurnFinalizer = true;
        standsInARow = 0;
    }
    public void ActorStand(int actorIndex)
    {
        standsInARow += 1;
        _frontEnd.ActorStand(actorIndex);
        TrumpCardReturnText("");
        _actorActedThisTurn = true;
        _actorTurnFinalizer = true;
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
    public void RoundDeckReset()
    {
        _actualDeck = new List<Card>(_baseCards);
        _actualDeck = ShuffleDeck(_actualDeck);
        foreach (Actor actor in _actors)
        {
            actor.ClearDeck();
            if(actor.actorDead == false)
            {
                actor.actorLost = false;
                actor.actorWon = false;
            }
        }
        _frontEnd.UpdateActorHands(_actors);
    }
    public Card ActorLatestCard(int actorIndex)
    {
        return _actors[actorIndex].actorDeck.Last();
    }

    // Client Functions, Called by the Network Manager Backend on the client's side.
    public void ActorHitClient(int actorIndex, Card card)
    {
        _actors[actorIndex].actorDeck.Add(card);
        _frontEnd.Actors[actorIndex].UpdateAnimatedCard(_actors[actorIndex].actorDeck.Last());
        _frontEnd.ActorHit(actorIndex, _actors[actorIndex].actorDeck);
        _frontEnd.UpdateActorDeck(_actors);
        TrumpCardReturnText("");
        _actorActedThisTurn = true;
        _actorTurnFinalizer = true;
        standsInARow = 0;
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

    public void TrumpCardReturnText(string text)
    {
        trumpCardReturnText.text = text;
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

    public Card(Card card)
    {
        this.value = card.value;
        this.suit = card.suit;
        this.image = card.image;
    }

    // Special Trump CardInitializer
    // SpecialSuit is always S for Special
    // Type uses the image value
    public Card(string specialSuit, int type)
    {
        this.value = 0;
        this.suit = specialSuit;
        this.image = type;
    }

    public string cardTexturePath()
    {
        string path = "PaperCards/";

        if (this.suit == "S")
        {
            path += "Specials/Special";

            switch (this.image)
            {
                case 0: // Reset21
                    path += "Reset21";
                    break;
                case 1: // To27
                    path += "To27";
                    break;
                case 2: // To17
                    path += "To17";
                    break;
                case 3: // ResetRound
                    path += "ResetRound";
                    break;
                case 4: // Peek
                    path += "Peek";
                    break;
                case 5: // Survivor
                    path += "Survivor";
                    break;
            }
            return path;
        }

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

    public string cardFancyName()
    {
        string name = "";
        // 0/Null = Number card, uses value for image, 1 = Jack, 2 = Queen, 3 = King
        switch (this.image)
        {
            case 1:
                name += "Jack of";
                break;
            case 2:
                name += "Queen of";
                break;
            case 3:
                name += "King of";
                break;
            default:
                if (this.value == 11)
                {
                    name += "Ace of ";
                }
                else
                {
                    name += $"{this.value} of";
                }
                break;
        }

        // A = Spades, B = Hearts, C = Clubs, D = Diamonds
        switch (this.suit)
        {
            case "A":
                name += "Spades";
                break;
            case "B":
                name += "Hearts";
                break;
            case "C":
                name += "Clubs";
                break;
            case "D":
                name += "Diamonds";
                break;
            default:
                break;
        }
        return name;
    }
}

public class Actor
{
    public List<Card> actorDeck { get; set; }
    public List<Card> actorSpecialDeck { get; set; }
    public int actorDamaged { get; set; }
    public bool actorLost { get; set; }
    public bool actorWon { get; set; }
    public bool actorDead { get; set; }
    public List<int> deckValues { get; set; }
    public Actor()
    {
        this.actorDeck = new List<Card>();
        this.actorSpecialDeck = new List<Card>();
        this.actorDamaged = 0;
        this.actorLost = false;
        this.actorWon = false;
        this.actorDead = false;
        this.deckValues = new List<int>();
    }
    public void CalculateDeckValues()
    {
        deckValues.Clear();

        // Start with a single base value
        List<int> totals = new List<int> { 0 };

        foreach (Card card in actorDeck)
        {
            List<int> newTotals = new List<int>();

            foreach (int total in totals)
            {
                if (card.value == 11) // Ace
                {
                    newTotals.Add(total + 1);
                    newTotals.Add(total + 11);
                }
                else
                {
                    newTotals.Add(total + card.value);
                }
            }

            totals = newTotals;
        }

        // Optional: remove duplicates
        deckValues = totals.Distinct().ToList();
    }
    public bool DeckValueOverTarget(int target)
    {
        return this.deckValues.All(x => x > target);
    }
    public bool DeckValueIsTarget(int target)
    {
        return this.deckValues.Any(x => x == target);
    }
    public void AddDamage(int damage)
    {
        this.actorDamaged += damage;
    }
    public void SetDamaged(int damage)
    {
        this.actorDamaged = damage;
    }
    public void ClearDeck()
    {
        this.actorDeck.Clear();
    }
    public void ClearSpecialDeck()
    {
        this.actorSpecialDeck.Clear();
    }
    public void Reset()
    {
        this.actorDeck.Clear();
        this.actorSpecialDeck.Clear();
        this.actorDamaged = 0;
        this.actorLost = false;
        this.actorWon = false;
        this.actorDead = false;
        this.deckValues = new List<int>();
    }
}
