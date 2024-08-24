namespace TheCardGame;

static class Support
{
    /*Count the number of cards in the specififed state*/
    static public int countCards<T>(List<Card> cards)
    {
        int cnt = 0;
        foreach (Card card in cards)
        {
            if (card.State is T)
            {
                cnt++;
            }
        }
        return cnt;
    }

    /*Check state of given card*/
    static public bool cardIsIn<T>(Card card)
    {
        return card.State is T;
    }

    /*Returns from a list of cards a string of cards that match the given state*/
    static public string CardIdsHumanFormatted<T>(List<Card> cards)
    {
        List<string> cardIds = new List<string>();
        foreach (Card card in cards)
        {
            if (card.State is T)
            {
                cardIds.Add(card.getId());
            }
        }
        return String.Join<string>(", ", cardIds);
    }

    /*Returns the specified card. Raise CardNotFoundException if card not there*/
    static public Card findCard(List<Card> sourceList, string cardId)
    {
        int pos = 0;
        Card? cardFound = null;
        foreach (Card card in sourceList)
        {
            if (card.getId() == cardId)
            {
                cardFound = card;
                break;
            }
            pos++;
        }

        if (cardFound == null)
        {
            throw new Exception($"Card with id: '{cardId}' not found");
        }

        return cardFound;
    }
}