using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A factory class for Card objects
/// Preloads cards in a pool
/// </summary>

public class CardFactory {

    private Transform deckGroup;
    private GameObject cardPrefab;
    private List<Sprite> cardSprites;
    private List<GameObject> cardPool;
    
    public CardFactory(Transform deckGroup, GameObject cardPrefab, List<Sprite> cardSprites, int poolSize)
    {
        cardPool = new List<GameObject>();

        this.deckGroup = deckGroup;
        this.cardPrefab = cardPrefab;
        this.cardSprites = cardSprites;

        // Preload card gameObjects and make them inactive
        PreloadEmptyCards(poolSize);
    }

    private void PreloadEmptyCards(int poolSize)
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject card = GameObject.Instantiate(cardPrefab);
            card.SetActive(false);
            card.transform.SetParent(GameplayCore.Instance.preloadedCardsGroup);

            cardPool.Add(card);
        }
    }

    private GameObject GetCardFromPool()
    {
        // Loop through the preloaded cards and return the first unused one
        for (int i = 0; i < cardPool.Count; i++)
        {
            if (!cardPool[i].activeSelf)
            {
                return cardPool[i];
            }
        }

        // If the code reaches here there isn't a free card, create a new one and add it to the pool
        GameObject card = GameObject.Instantiate(cardPrefab);
        cardPool.Add(card);

        return card;
    }
    
    // Creates and returns a card by giving it a corresponding sprite number ( from 0 to 51 )
    public GameObject CreateCard(int spriteNum)
    {
        GameObject card = GetCardFromPool();

        int rank = GetRankFromSpriteNum(spriteNum);
        CardSuite suite = GetSuiteFromSpriteNum(spriteNum);
        
        // Set new card values; set the sprite image to card_back
        card.GetComponent<Card>().SetRankAndSuite(suite, rank, cardSprites[spriteNum]);
        card.GetComponent<Card>().cardImage.sprite = cardSprites[52];

        // Reset transform values
        card.transform.localPosition = Vector3.zero;
        card.transform.localScale = Vector3.one;
        card.transform.rotation = Quaternion.identity;
        card.transform.SetParent(deckGroup, false);

        card.SetActive(true);

        return card;
    }

    public void ResetPool()
    {
        // Loop through the pool and make all preloaded cards inactive
        for (int i = 0; i < cardPool.Count; i++)
        {
            cardPool[i].SetActive(false);
        }
    }

    // UTILITY METHODS >>>

    // Extract a card's rank from a sprite number ( 0-51 )
    public static int GetRankFromSpriteNum(int spriteNum)
    {
        if (spriteNum < 12)
        {
            return spriteNum;
        }
        return spriteNum % 13;
    }

    // Extract a card's suite from a sprite number ( 0-51 )
    public static CardSuite GetSuiteFromSpriteNum(int spriteNum)
    {
        return (CardSuite) (spriteNum/13);
    }

    // Get a card's sprite number from rank and suite
    public static int GetSpriteNumOfCard(CardSuite suite, int rank)
    {
        return rank + (int)suite * 13;
    }

}