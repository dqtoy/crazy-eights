using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Player superclass implementing core functionalities
/// </summary>

public abstract class Player {

    public List<Card> cardsInHand;                              // List of the cards the player has in his hand
    public Transform parentGroup;                               // The group in which cards will be added
    public Vector3 handPos;                                     // Screen position of the "hand" of the player ( where the cards will be dealt )

    public int id;

    public Player(int id, Transform parentGroup, Vector3 handPos )
    {
        cardsInHand = new List<Card>();

        this.parentGroup = parentGroup;
        this.handPos = handPos;
        this.id = id;
    }

    public void ReceiveCard(Card newCard, float animDelay)
    {
        cardsInHand.Add(newCard);
        
        // Animate it to the player's hand pos
        newCard.Animate(handPos, parentGroup, animDelay, id);
    }

    // Checks for the player's first valid move ( if any )
    public Card ReturnFirstValidMove()
    {
        foreach (Card c in cardsInHand)
        {
            if (Rulemaster.IsMoveValid(GameplayCore.Instance.topCard, c))
            {
                return c;
            }
        }
        return null;
    }

    public void ResetPlayer()
    {
        for (int i = cardsInHand.Count - 1; i >= 0; i--)
        {
            // Reset the card and remove it from the list
            cardsInHand[i].Reset();
            cardsInHand.RemoveAt(i);
        }
    }
}

public class HumanPlayer : Player
{
    public HumanPlayer(int id, Transform parentGroup, Vector3 handPos, _UnityEventInt onNextTurn) : base(id, parentGroup, handPos)
    {
        onNextTurn.AddListener(OnNextTurn);
    }

    // Listener for the GameplayCore's NextTurn event
    public void OnNextTurn(int playerTurn)
    {
        if (playerTurn == id)
        {
            // Check if we have valid moves
            if (ReturnFirstValidMove() == null)
            {
                // Set the boolean indicating that the player can now draw a card
                GameplayCore.Instance.playerMustDraw = true;

                // Prompt the player to draw cards with a notification ( only if there are cards left )
                if (!GameplayCore.Instance.noCardsLeft)
                {
                    UICore.Instance.ShowDrawCardText();
                }
            }

        }
    }

    // A coroutine method for automatically drawing a card after 1 second
    // Curently not used ( but may come in handy later )
    private IEnumerator AutomaticallyDrawACard()
    {
        yield return new WaitForSeconds(1);

        // Player has no more options, draw a card from the deck
        if (!GameplayCore.Instance.noCardsLeft)
        {
            ReceiveCard(GameplayCore.Instance.cardDeck.DrawCard(), 0);
        }

        GameplayCore.Instance.EndTurn();
    }
}

public class AIPlayer : Player
{
    public AIPlayer(int id, Transform parentGroup, Vector3 handPos, _UnityEventInt onNextTurn) : base(id, parentGroup, handPos)
    {
        onNextTurn.AddListener(OnNextTurn);
    }

    public void OnNextTurn(int playerTurn)
    {
        if (playerTurn == id)
        {
            // Wait shortly as though the AI is "thinking" ( uses the GameplayCore's Monobehaviour to start the coroutine )
            GameplayCore.Instance.StartCoroutine(TryToPlayACard());
        }
    }

    private IEnumerator TryToPlayACard()
    {
        yield return new WaitForSeconds(1);

        Card c = ReturnFirstValidMove();

        if (c != null)
        {
            GameplayCore.Instance.PlayCard(c);
            cardsInHand.Remove(c);
        }
        else
        {
            // AI has no options, draw a card from the deck and end the turn
            if (!GameplayCore.Instance.noCardsLeft)
            {
                ReceiveCard(GameplayCore.Instance.cardDeck.DrawCard(), 0.4f);
            }
            GameplayCore.Instance.EndTurn();
        }
        
    }
}