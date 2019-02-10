using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum CardSuite { Hearts = 0, Spades = 1, Diamonds = 2, Clubs = 3 };

public class Card : MonoBehaviour {

    public int rank;                                    // The card's rank will be from 0 to 12 ( A = 0; 2-10; J; D; K = 12)
    public CardSuite suite;                             // The suite ( hearts, spades, diamonds, clubs )
    public Image cardImage;                             // The card's graphical representation
    public int playerID;                                // If this card belongs to player 1 ( id==0 ) the user can see it

    private Vector3 handPos = Vector3.one;              // Position to which the card will be animated when dealt
    private bool cardInHand = false;                    // Whether the card has animated to the hand or not
    private Sprite newSprite;                           // The sprite which the card will receive after it is dealt to the hand ( animated )
    private Transform newGroup;                         // The group in which the card will be put after animating

    public void Reset()
    {
        // Resets the card as it was originally
        handPos = Vector3.one;
        cardInHand = false;
        gameObject.SetActive(false);
        transform.SetParent(GameplayCore.Instance.preloadedCardsGroup);
    }

    public void SetRankAndSuite(CardSuite suite, int rank, Sprite newSprite)
    {
        this.rank = rank;
        this.suite = suite;
        this.newSprite = newSprite;
    }

    // Animates the dealing of a card to the player's hand
    public void Animate(Vector3 newPos, Transform newGroup, float delay, int playerID)
    {
        this.newGroup = newGroup;
        this.playerID = playerID;

        // Play a card slide sound with a given delay
        StartCoroutine(PlaySlideSound(delay));

        transform.DOLocalRotate(new Vector3(0, 0, 180f), 0.35f, RotateMode.Fast)
            .SetDelay(delay)
            .SetEase(Ease.OutSine);

        transform.DOLocalMove(newPos, 0.35f)
            .SetDelay(delay)
            .SetEase(Ease.OutSine)
            .OnComplete(OnAnimationCompleted);
    }

    private void OnAnimationCompleted()
    {
        cardInHand = true;

        // Pop card with a scale animation
        transform.localScale = Vector3.zero;
        transform.DOScale(1, 0.1f)
            .SetEase(Ease.OutSine);

        transform.SetParent(newGroup, false);

        // Only cards belonging to the first player can be seen by him
        if (playerID == 0)
        {
            cardImage.sprite = newSprite;
        }
    }

    // Overload method for revealing the first card of the deck on game start
    public void Animate(Vector3 newPos, Transform newGroup)
    {
        // Play a card sound with no delay
        SoundCore.Instance.PlayCardSlideSound();

        playerID = -2; // This card does not belong to any player

        transform.DOMove(newPos, 0.25f)
            .SetEase(Ease.OutSine);

        transform.SetParent(newGroup, false);

        cardImage.sprite = newSprite;
    }

    // Animates the card from the hand to the field
    public void AnimateCardInPlay(Transform newGroup)
    {
        // Play a card sound
        SoundCore.Instance.PlayCardPlaceSound();

        cardImage.sprite = newSprite;

        // Kill previous tweens ( from OnPointerEnter / OnExit events ) so that they don't mess with the new animation
        transform.DOKill();

        transform.SetParent(newGroup, true);
        transform.DOMove(Vector3.zero, 0.25f)
            .SetEase(Ease.OutSine)
            .OnComplete(() => GameplayCore.Instance.UpdateTopCard(this));
    }

    // Hovering over player cards should animate them slightly
    // Only allow it after cards have been dealt to the player's hand
    public void OnPointerEnter()
    {
        if (cardInHand && playerID == 0 && GameplayCore.Instance.cardsDealt)
        {
            if (handPos == Vector3.one)
            {
                // Save the current position the first time the card has been pointed at
                handPos = transform.localPosition;
            }

            // Animate slightly up
            transform.DOLocalMoveY(40f, 0.2f)
                .SetEase(Ease.OutSine)
                .SetRelative();
        }
    }

    public void OnPointerExit()
    {
        if (cardInHand && playerID == 0 && GameplayCore.Instance.cardsDealt)
        {
            if (handPos == Vector3.one)
            {
                return;
            }

            // Return to hand pos
            transform.DOLocalMoveY(handPos.y, 0.2f)
            .SetEase(Ease.OutSine);
        }
    }

    // On Card Tap
    public void TryToPlayCard()
    {
        if (playerID == -1)
        {
            // Player is clicking the deck image, try drawing a card if applicable
            GameplayCore.Instance.TryDrawingCardFromDeck();
            return;
        }

        // Only try playing the card if it is in the hand of player1 and it's the player's turn
        if (playerID == 0 && GameplayCore.Instance.cardsDealt && GameplayCore.Instance.playerTurn == 0)
        {
            GameplayCore.Instance.PlayCard(this);
        }
    }

    private IEnumerator PlaySlideSound(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        // Play the sound after the delay
        SoundCore.Instance.PlayCardSlideSound();
    }
}