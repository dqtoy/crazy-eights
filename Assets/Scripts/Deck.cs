using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Deck {

    public int[] deckData = Enumerable.Range(0, 52).ToArray();      // Array of int from 0 to 51 representing all cards
    public Stack<int> deck;                                         // The actual deck Stack from which cards will be popped
    
    private CardFactory cardFactory;

    // Random instance used for the shuffle deck algorithm ( put System in front otherwise it will use Unity.Random )
    private System.Random _random = new System.Random();

    public Deck(Transform deckGroup, GameObject cardPrefab, List<Sprite> cardSprites)
    {
        // A new card factory with the needed parameters and poolsize of 20 preloaded cards
        cardFactory = new CardFactory(deckGroup, cardPrefab, cardSprites, 20);
    }

    // Call this every time a new game has been started
    public void Reset()
    {
        // Shuffle the deckData array
        ShuffleDeck();

        // Create a new deck Stack from the shuffled data
        deck = new Stack<int>(deckData);

        // Make all cards in the pool available
        cardFactory.ResetPool();
    }

    // Draws a card from the Deck Stack
    public Card DrawCard()
    {
        int num = deck.Pop();

        // Check if this was the last drawn card
        if (deck.Count == 0)
        {
            // Notify the Core that there are no more cards
            GameplayCore.Instance.NoCardsInDeck();
        }

        return cardFactory.CreateCard(num).GetComponent<Card>();
    }

    // Shuffles the deck randomly ( using the Fisher Yates Shuffle )
    // Taken from: https://www.dotnetperls.com/fisher-yates-shuffle
    private void ShuffleDeck()
    {
        int n = deckData.Length;

        for (int i = 0; i < n; i++)
        {
            int r = i + _random.Next(n - i);
            int temp = deckData[r];
            deckData[r] = deckData[i];
            deckData[i] = temp;
        }
    }
    
}
