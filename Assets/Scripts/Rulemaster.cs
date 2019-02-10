using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Rulemaster {

    private static int specialRank = 7;         // Eights can always be played ( their rank id is 7 )

    // Checks if a move is valid
    public static bool IsMoveValid(Card topDeckCard, Card playedCard)
    {
        int tRank = topDeckCard.rank;
        CardSuite tSuite = topDeckCard.suite;
        
        int pRank = playedCard.rank;
        CardSuite pSuite = playedCard.suite;

        if(tRank == pRank || tSuite == pSuite || pRank == specialRank)
        {
            // this card CAN be played
            return true;
        }

        return false;
    }
}
