using System.Runtime.CompilerServices;

namespace TheCardGame;

public class Player
{
    private List<Card> cards;
    private int healthValue;
    private readonly string name = string.Empty;
    private List<PlayerObserver> observers = new List<PlayerObserver>();
    private List<Action> effectList;
    private int energyUsedThisTurn;

    public Player(string name, int initialLife)
    {
        this.cards = new List<Card>();
        this.healthValue = initialLife;
        this.name = name;
        this.effectList = new List<Action> { Effect_1, Effect_2 };
        this.energyUsedThisTurn = 0;

    }
    /*Take number of cards from deck into hand*/
    public bool fillHand(int cardsToDraw)
    {
        Card? Taken;
        for (int cnt = 0; cnt < cardsToDraw; cnt++)
        {
            Taken = this.takeCard();
            if (Taken is null) { return false; }
        }
        return true;
    }

    public void setEnergyUsedThisTurn(int value)
    {
        if (value < 0)
        {
            this.energyUsedThisTurn = 0;
            return;
        }
        this.energyUsedThisTurn += value;
    }

    public int getEnergyUsedThisTurn()
    {
        return this.energyUsedThisTurn;
    }

    public void addObserver(PlayerObserver po)
    {
        this.observers.Add(po);
    }

    public void removeObserver(PlayerObserver po)
    {
        this.observers.Remove(po);

    }

    public void setCards(List<Card> cards)
    {
        this.cards = cards;
    }

    public List<Card> getCards()
    {
        return this.cards;
    }

    public string getName()
    {
        return this.name;
    }

    public void decreaseHealthValue(int value)
    {
        this.healthValue -= value;
        if (this.healthValue <= 0)
        {
            PlayerDiedEvent pde = new PlayerDiedEvent(this.getName(), this.getHealthValue(), "Health below or is zero");
            foreach (PlayerObserver po in this.observers)
            {
                po.playerDied(pde);
            }
        }
    }

    public int getHealthValue()
    {
        return this.healthValue;
    }

    /*Take the first card from his deck and put it in his hand*/
    public Card? takeCard()
    {
        foreach (Card card in this.cards)
        {
            if (card.isNotYetInTheGame())
            {
                if (card.onIsTaken() is true)
                {
                    return card;
                }
            }
        }

        PlayerDiedEvent pde = new PlayerDiedEvent(this.getName(), this.getHealthValue(), "No more cards in deck");
        foreach (PlayerObserver po in this.observers)
        {
            po.playerDied(pde);
        }
        return null;
    }

    /*Draw a card from his hand*/
    public Card? drawCard(string cardId)
    {

        foreach (Card card in this.cards)
        {
            if (card.getId() == cardId)
            {
                if (card.onDraw() is true)
                {
                    return card;
                }
            }
        }
        return null;
    }

    /*Removes excess cards from hand at the end of turn*/
    public void trimCards(int maxCards)
    {

        int cnt = Support.countCards<InTheHand>(this.cards);
        if (cnt <= maxCards)
        {
            System.Console.WriteLine($"[Player] {this.getName()} trimmed 0 cards into discard pile.");
            return;
        }

        int cntDisposed = 0;
        foreach (Card card in this.cards)
        {

            if (Support.cardIsIn<InTheHand>(card))
            {
                bool isDisposed = card.dispose();
                if (isDisposed)
                {
                    System.Console.WriteLine($"[Player] Card {card.getId()} is disposed.");
                    cntDisposed++;
                }
            }

            cnt = Support.countCards<InTheHand>(this.cards);
            if (cnt <= maxCards)
            {
                break;
            }
        }
        System.Console.WriteLine($"[Player] Disposed {cntDisposed} cards");
    }

    /*This effect removes one random card from the hand*/
    public void Effect_1()
    {
        int cnt = Support.countCards<InTheHand>(this.cards);
        if (cnt == 0)
        {
            System.Console.WriteLine($"[Player] {this.getName()} hand is empty. No card to dispose of.");
            return;
        }
        foreach (Card card in this.cards)
        {
            if (Support.cardIsIn<InTheHand>(card))
            {
                bool isDisposed = card.dispose();
                if (isDisposed)
                {
                    System.Console.WriteLine($"[Player] Card {card.getId()} is disposed from the hand of {this.name}.");
                    return;
                }
            }
        }
    }

    public void Effect_2()
    {
        //Example of an permanent effect
    }

    /*This method applies the effect from the cast of an apponents creature onto the player*/
    public void applyEffect(int select)
    {
        if (select >= 0 && select < effectList.Count)
        {
            System.Console.WriteLine("[Player] Applying effect of creature:");
            this.effectList[select]?.Invoke();
        }
        else
        {
            Console.WriteLine("[Player] Invalid index or effect not found.");
        }
    }
}

