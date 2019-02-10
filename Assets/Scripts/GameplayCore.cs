using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// A unity event class which accepts int as parameter
[System.Serializable]
public class _UnityEventInt : UnityEvent<int> { }

public class GameplayCore : MonoBehaviour {

    public GameObject cardPrefab;
    public List<Sprite> cardSprites;

    public Transform preloadedCardsGroup;                               // The parent group for the card pool
    public Transform deckGroup;                                         // The parent group for cards put on the field
    public Transform player1Group;                                      // The parent group for player 1
    public Transform player2Group;                                      // The parent group for player 2

    public List<Player> players;                                        // List of all players in the game

    [HideInInspector] public _UnityEventInt onNextTurn;                 // Event fired after each turn

    // Game logic variables
    [HideInInspector] public bool gameStarted           = false;
    [HideInInspector] public bool cardsDealt            = false;
    [HideInInspector] public bool playerMustDraw        = false;
    [HideInInspector] public bool noCardsLeft           = false;
    [HideInInspector] public bool cardPlayedThisTurn    = false;
    [HideInInspector] public int winner                 = -1;
    [HideInInspector] public int playerTurn             = 0;

    [HideInInspector] public Card topCard;                              // The top card on the field
    [HideInInspector] public Deck cardDeck;                             // The Deck from which cards will be drawn

    private GameObject deckPlaceholder;                                 // The deck image on the field

    private Canvas mainCanvas;
    private float canvasH;

    // Singleton variables
    private static GameplayCore _instance;
    public static GameplayCore Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        // Make sure this is a singleton
        if (_instance == null)
        {
            _instance = this;
        }

        DontDestroyOnLoad(this);

        // Get the canvas from the player1's group and assign needed variables
        mainCanvas = player1Group.transform.parent.GetComponent<Canvas>();
        canvasH = mainCanvas.GetComponent<RectTransform>().rect.height;
        deckPlaceholder = deckGroup.GetChild(0).gameObject;

        // Create a new deck with the given card sprites and prefab object
        cardDeck = new Deck(deckGroup, cardPrefab, cardSprites);

        // Create a new list of players
        players = new List<Player>();

        // Get top & bottom positions of screen for the players' hands
        Vector3 topPos = new Vector3(-deckGroup.localPosition.x, canvasH, 0);
        Vector3 botPos = new Vector3(-deckGroup.localPosition.x, -canvasH, 0);

        // Create 1 "human" and 1 bot player
        HumanPlayer player1 = new HumanPlayer(0, player1Group, botPos, onNextTurn);
        AIPlayer bot1 = new AIPlayer(1, player2Group, topPos, onNextTurn);

        players.Add(player1);
        players.Add(bot1);
    }

    public void StartNewGame()
    {
        if (gameStarted)
        {
            return;
        }

        gameStarted = true;
        deckPlaceholder.SetActive(true);

        UICore.Instance.HideMainMenu();

        // Reset and shuffle the deck
        cardDeck.Reset();

        // Deal 7 cards to both players
        float delayBetweenCards = 0.4f;

        for (int i = 6; i >= 0; i--)
        {
            for (int j = 1; j >= 0; j--)
            {
                players[j].ReceiveCard(cardDeck.DrawCard(), (i*2 + j) * delayBetweenCards);                
            }
        }

        // Reveal the first card after all cards have been dealt
        StartCoroutine(RevealFirstCard((14) * delayBetweenCards));
    }

    public void ResetGame()
    {
        // Restart the game and return to the main menu
        deckPlaceholder.SetActive(true);

        if (topCard)
        {
            topCard.Reset();
        }

        // Reset game logic variables
        winner = -1;
        playerMustDraw = false;

        foreach (Player p in players)
        {
            p.ResetPlayer();
        }

        UICore.Instance.HideGameEndOverlay();
    }

    public void PlayCard(Card card)
    {
        // Check whether the card can be played
        if (Rulemaster.IsMoveValid(topCard, card) && !cardPlayedThisTurn)
        {
            cardPlayedThisTurn = true;

            // Remove this card from the player's cardInHands list
            players[playerTurn].cardsInHand.Remove(card);

            // This card's id does not belong to any player anymore
            card.playerID = -2;

            // Animate the card onto the field
            card.AnimateCardInPlay(deckGroup);
        }
    }

    public void UpdateTopCard(Card card)
    {
        topCard.Reset();
        topCard = card;

        EndTurn();
    }

    // The Deck object will call this method when the deck has no more cards left
    public void NoCardsInDeck()
    {
        noCardsLeft = true;
        deckPlaceholder.SetActive(false);
    }

    public void TryDrawingCardFromDeck()
    {
        // Player has no more options, draw a card from the deck
        if (!noCardsLeft && playerMustDraw)
        {
            UICore.Instance.HideDrawCardText();

            playerMustDraw = false;
            players[0].ReceiveCard(cardDeck.DrawCard(), 0);

            EndTurn();
        }
    }

    public void EndTurn()
    {
        // The next turn will begin after a short delay
        StartCoroutine(InvokeEndEvent());
    }

    public void EndGame()
    {
        // Announce the winner through the UICore
        UICore.Instance.ShowGameEndOverlay(winner);

        // Update bools so that no more events are accepted
        gameStarted = false;
        cardsDealt = false;
    }

    #region PRIVATE METHODS

    private IEnumerator RevealFirstCard(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        // Reveal the first card and decide which player starts first randomly
        Card firstCard = cardDeck.DrawCard();
        firstCard.Animate(Vector3.zero, deckGroup);

        // Save reference to the top deck card ( needed for the Rulemaster )
        topCard = firstCard;
        cardsDealt = true;
        noCardsLeft = false;
        cardPlayedThisTurn = false;

        // Decide whose turn it is
        playerTurn = Random.Range(0, players.Count - 1);

        // Invoke the next turn event ( all non-human players should be registered to listen for it )
        onNextTurn.Invoke(playerTurn);

        // Update the turn arrow which will be shown above the avatar's head
        UICore.Instance.OnNewTurn(playerTurn);
    }

    private IEnumerator InvokeEndEvent()
    {
        yield return new WaitForSeconds(0.5f);

        // Check whether there is a win or draw condition
        bool gamedraw = CheckIfDraw();
        bool gamewon = CheckIfSomeoneWon();

        if (gamewon || gamedraw)
        {
            EndGame();
        }
        else
        {
            playerTurn++;
            if (playerTurn > players.Count - 1)
            {
                playerTurn = 0;
            }
            onNextTurn.Invoke(playerTurn);

            // update the turn arrow which will be shown above the avatar's head
            UICore.Instance.OnNewTurn(playerTurn);

            cardPlayedThisTurn = false;
        }
    }

    private bool CheckIfSomeoneWon()
    {
        foreach (Player p in players)
        {
            if (p.cardsInHand.Count == 0)
            {
                // This player has won the game
                winner = p.id;

                return true;
            }
        }
        return false;
    }

    private bool CheckIfDraw()
    {
        if (noCardsLeft)
        {
            bool noPlayerHasMoves = true;

            foreach (Player p in players)
            {
                Card c = p.ReturnFirstValidMove();
                if (c != null)
                {
                    noPlayerHasMoves = false;
                }
            }

            if (noPlayerHasMoves)
            {
                winner = -2;
                return true;
            }
        }

        return false;
    }
    #endregion
}
