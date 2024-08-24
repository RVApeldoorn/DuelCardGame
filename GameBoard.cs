namespace TheCardGame;

public class GameBoard : PlayerObserver
{
    private static GameBoard? GBInstance;
    private Player player1;
    private Player player2;
    private Player currentPlayer;
    private Player opponentPlayer;
    private List<string> creaturesAttacked;
    private List<(SpellCard, CreatureCard)> spellstack;
    private int turnCnt;
    private bool gameEnded;

    /*To follow the singleton pattern, only one instance of the Gameboard class can be 
    created using getGameBoard()*/
    public static GameBoard getGameBoard(Player player1, Player player2)
    {
        GBInstance ??= new GameBoard(player1, player2);
        return GBInstance;
    }

    private GameBoard(Player player1, Player player2)
    {
        if (player1.getName() == player2.getName())
        {
            throw new System.InvalidOperationException("[GameBoard] The two players should have a unique name.");
        }
        this.player1 = player1;
        this.player2 = player2;
        this.currentPlayer = player1;
        this.opponentPlayer = player2;
        this.turnCnt = 0;
        this.gameEnded = false;
        this.creaturesAttacked = new List<string>();
        this.spellstack = new List<(SpellCard, CreatureCard)>();
        this.player1.addObserver(this);
        this.player2.addObserver(this);
        this.startNewGame();
    }

    public bool takeCard()
    {
        Player currentTurnPlayer = this.getCurrentTurnPlayer();
        Card? card = currentTurnPlayer.takeCard();
        if (card == null)
        {
            System.Console.WriteLine($"[GameBoard] {currentTurnPlayer.getName()} could not take card.");
            return false;
        }
        else
        {
            System.Console.WriteLine($"[GameBoard] {currentTurnPlayer.getName()} took card {card.getId()} from deck into hand.");
            return true;
        }
    }

    public bool drawCard(string cardId)
    {
        Player currentTurnPlayer = this.getCurrentTurnPlayer();

        //Check is player has enough unused, tapped energy to draw the card
        Card? cardE = Support.findCard(currentTurnPlayer.getCards(), cardId);
        if (cardE is null)
        {
            System.Console.WriteLine($"[GameBoard] {currentTurnPlayer.getName()} Didn't draw card {cardId}: Not found.");
            return false;
        }

        if ((this.energyTapped(currentPlayer) - currentPlayer.getEnergyUsedThisTurn()) < cardE.getEnergyCost())
        {
            System.Console.WriteLine($"[GameBoard] {currentTurnPlayer.getName()} Didn't draw card {cardId}: Not enough energy");
            return false;
        }

        Card? card = currentTurnPlayer.drawCard(cardId);
        if (card is null)
        {
            System.Console.WriteLine($"[GameBoard] {currentTurnPlayer.getName()} Didn't draw card {cardId}: Not in his hand.");
            return false;
        }

        currentPlayer.setEnergyUsedThisTurn(card.getEnergyCost());
        System.Console.WriteLine($"[GameBoard] {currentTurnPlayer.getName()} draw card {card.getId()}.");

        //If drawn card is creaturecard, apply effect of the cast
        CreatureCard? cCard = card as CreatureCard;
        if (cCard is not null)
        {
            this.getOpponentPlayer().applyEffect(cCard.getEffectCreature());
        }
        return true;
    }

    /*Adds a SpellCard and destination (CreatureCard) to the spellstack*/
    public bool addSpell(string spellCardId, string creatureCardId)
    {
        // Get the current turn player and the opponent player
        Player currentPlayer = getCurrentTurnPlayer();
        Player opponentPlayer = getOpponentPlayer();

        // Check if the spell card is in the current or opponent player's hand and if the player has sufficient energy
        SpellCard? spellCard = currentPlayer.getCards().Find(card => card.getId() == spellCardId) as SpellCard;
        if (spellCard == null)
        {
            //Card not found
            spellCard = opponentPlayer.getCards().Find(card => card.getId() == spellCardId) as SpellCard;
            if (spellCard == null)
            {
                Console.WriteLine($"[GameBoard] Spell card with ID '{spellCardId}' is in neither players hands.");
                return false;
            }
            else if ((this.energyTapped(opponentPlayer) - opponentPlayer.getEnergyUsedThisTurn()) < spellCard.getEnergyCost())
            {
                //Interupting opponent player hasnt got enough energy to cast the spell
                System.Console.WriteLine($"[GameBoard] {opponentPlayer.getName()} Didn't draw card {spellCard.getId()}: Not enough energy");
                return false;
            }
        }
        else if ((this.energyTapped(currentPlayer) - currentPlayer.getEnergyUsedThisTurn()) < spellCard.getEnergyCost())
        {
            //Current player hasnt got enough energy to cast the spell
            System.Console.WriteLine($"[GameBoard] {currentPlayer.getName()} Didn't draw card {spellCard.getId()}: Not enough energy");
            return false;
        }

        // Check if the creature card is on the board (either on the current player's side or the opponent's side)
        CreatureCard? creatureCard = currentPlayer.getCards().Find(card => card.getId() == creatureCardId) as CreatureCard;
        if (creatureCard == null)
        {
            creatureCard = opponentPlayer.getCards().Find(card => card.getId() == creatureCardId) as CreatureCard;
            if (creatureCard == null)
            {
                Console.WriteLine($"[GameBoard] Creature card with ID '{creatureCardId}' is not on the board.");
                return false;
            }
        }

        // Add the spell to the spellstack/board
        spellCard.onDraw();
        spellstack.Add((spellCard, creatureCard));

        // Output a message indicating the spell card was successfully applied
        Console.WriteLine($"[GameBoard] Spell card '{spellCardId}' added to the stack.");
        return true;
    }

    /*Applies the Spellcards in the spellstack onto the specified CreatureCards (FIFO)*/
    public void resolveSpellStack()
    {
        foreach (var spell in this.getSpellStack())
        {
            spell.Item1.applySpellEffect(spell.Item2);
            if (spell.Item1.getEffect().Type is SpellEffectType.Temporary)
            {
                spell.Item1.dispose();
            }
            else
            {
                //Permanent cards stay active on the board after the resolution of the stack
            }
        }
        spellstack.Clear(); //Clear the spell stack after resolving all spells
    }

    public List<(SpellCard, CreatureCard)> getSpellStack()
    {
        return this.spellstack;
    }

    private void swapPlayer()
    {
        if (this.currentPlayer.getName() == this.player1.getName())
        {
            this.currentPlayer = this.player2;
            this.opponentPlayer = this.player1;
        }
        else
        {
            this.currentPlayer = this.player1;
            this.opponentPlayer = this.player2;
        }
    }

    public bool newTurn()
    {
        if (this.gameEnded)
        {
            return false;
        }
        this.turnCnt++;
        if (this.takeCard())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void endTurn()
    {
        //Untap lands at beginning of next turn
        foreach (Card card in this.opponentPlayer.getCards())
        {
            card.onEndTurn();
        }
        opponentPlayer.setEnergyUsedThisTurn(-1);
        this.getCurrentTurnPlayer().trimCards(7);
        this.creaturesAttacked.Clear();
        this.swapPlayer();
    }

    public Player getCurrentTurnPlayer()
    {
        return this.currentPlayer;
    }

    public Player getOpponentPlayer()
    {
        return this.opponentPlayer;
    }

    public int getCurrentTurn()
    {
        return this.turnCnt;
    }

    /*Puts all given defenders in defending state and attacker in attacking state after check if 
    given attacking card is a CreatureCard and if it hasn't been used in the current turn*/
    public bool letsAttack(string cardId, List<string> opponentDefenseCardIds)
    {
        foreach (Card oCard in this.opponentPlayer.getCards())
        {
            foreach (string defenseCardId in opponentDefenseCardIds)
            {
                if (oCard.getId() == defenseCardId)
                {
                    oCard.goDefending();
                }
            }
        }

        Card card = Support.findCard(this.currentPlayer.getCards(), cardId);
        CreatureCard? attackCard = card as CreatureCard;
        if (attackCard is not null)
        {
            if (this.creaturesAttacked.Contains(cardId))
            {
                Console.WriteLine($"[GameBoard] You already attacked with {cardId} in this turn");
                return false;
            }
            this.creaturesAttacked.Add(cardId);
            attackCard.goAttacking();
            performAttack(attackCard);
            return true;
        }
        Console.WriteLine($"[GameBoard] You can only attack with a Creature card, {cardId} rejected");
        return false;
    }

    /*Starts actual attack with in letsAttack specified cards. determining if attack is defended and inflicting 
    damage accordingly on the opponent player.*/
    public bool performAttack(CreatureCard card)
    {
        System.Console.WriteLine($"[GameBoard] {card.getId()} Peforms attack.");
        bool defended = false;
        int attackValue = card.GetAttack();
        foreach (Card dCard in getOpponentPlayer().getCards())
        {
            CreatureCard? cdCard = dCard as CreatureCard;
            if (cdCard is not null && cdCard.State is IsDefending)
            {
                (bool cardDefended, int attackValueLeft) = absorbAttack(attackValue, cdCard);
                if (cardDefended)
                {
                    defended = true;
                    attackValue = attackValueLeft;
                }
            }
        }

        if (!defended)
        {
            Console.WriteLine($"[GameBoard] Attack from {this.getCurrentTurnPlayer().getName()} caused {attackValue} damage to {this.getOpponentPlayer().getName()}");
            getOpponentPlayer().decreaseHealthValue(attackValue);
            return false;
        }
        Console.WriteLine($"[GameBoard] Attack from {this.getCurrentTurnPlayer().getName()} defended by {this.getOpponentPlayer().getName()}");
        return true;
    }

    /*Calculates remaining attack value after a defending card absorbs part of the attack, updates the card's state 
    and returns adjusted attack value.*/
    public (bool, int) absorbAttack(int iAttackValue, CreatureCard card)
    {
        int defenseValue = card.GetDefence();
        int attackValueLeft = iAttackValue - defenseValue;
        int defenseValueLeft = card.subtractDefenseValue(iAttackValue);
        System.Console.WriteLine($"[GameBoard] Card '{card.getId()}' with defense-value {defenseValue} absorbed attack-value {iAttackValue}. Attack value left: {attackValueLeft}");
        if (defenseValueLeft <= 0)
        {
            card.State = new OnTheDisposedPile(card.State);
        }
        return (true, attackValueLeft);
    }

    /* Tap Energry from given land-card */
    public void tapFromCard(string cardId)
    {
        foreach (Player player in new Player[] { player1, player2 })
        {
            foreach (Card card in player.getCards())
            {
                if (card.getId() == cardId)
                {
                    card.tapEnergy();
                    return;
                }
            }
        }
    }

    /*Sum of energy from current tapped lands*/
    public int energyTapped(Player player)
    {
        int sumEnergy = 0;
        foreach (Card card in player.getCards())
        {
            LandCard? landCard = card as LandCard;
            if (landCard is not null)
            {
                sumEnergy += landCard.getEnergyLevel();
            }
        }
        return sumEnergy;
    }

    /*Handles the event of a player's death and declares opposing player as winner*/
    public override void playerDied(PlayerDiedEvent pde)
    {

        System.Console.WriteLine($"[GameBoard] Player {pde.getPlayerName()} died. Health: {pde.getHealth()}, {pde.getReason()}");
        if (pde.getPlayerName() == this.player1.getName())
        {
            System.Console.WriteLine($"[GameBoard] Player {this.player2.getName()} is the winner!");
        }
        else
        {
            System.Console.WriteLine($"[GameBoard] Player {this.player1.getName()} is the winner!");
        }
        this.gameEnded = true;
    }

    public void startNewGame()
    {
        //<end>
        //Here would be the code to reset the gameboard fields, players and their cards

        //<start>
        //Filling hand of players with a number of cards at the start of a game
        if (this.player1.fillHand(7))
        {
            this.player2.fillHand(7);
        }
    }

    /*Very fancy player highlighting*/
    public void printPlayer(Player player)
    {
        Console.BackgroundColor = ConsoleColor.DarkGreen;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write($"Player: {player.getName()}");
        Console.ResetColor();
    }

    /*Prints current games turn, players, cards, states*/
    public void log()
    {
        System.Console.WriteLine("\n<Current situation>");
        System.Console.WriteLine($"Current turn-player: {this.currentPlayer.getName()}, Turn: {this.getCurrentTurn()}\n");
        System.Console.WriteLine($"Player {this.player1.getName()}: Health: {this.player1.getHealthValue()}");
        System.Console.WriteLine($"Player {this.player2.getName()}: Health: {this.player2.getHealthValue()}\n");

        List<Card> cards_player1 = this.player1.getCards();

        this.printPlayer(this.player1);
        System.Console.WriteLine($" (ontheboard/indeck/inhand/indiscard-pile) {Support.countCards<OnTheBoard>(cards_player1)}/{Support.countCards<InTheDeck>(cards_player1)}/{Support.countCards<InTheHand>(cards_player1)}/{Support.countCards<OnTheDisposedPile>(cards_player1)}");
        System.Console.WriteLine($"Player {this.player1.getName()} on the board: " + Support.CardIdsHumanFormatted<OnTheBoard>(cards_player1));
        System.Console.WriteLine($"Player {this.player1.getName()} tapped lands: " + Support.CardIdsHumanFormatted<IsTapped>(cards_player1));
        System.Console.WriteLine($"Player {this.player1.getName()} in deck: " + Support.CardIdsHumanFormatted<InTheDeck>(cards_player1));
        System.Console.WriteLine($"Player {this.player1.getName()} in hand: " + Support.CardIdsHumanFormatted<InTheHand>(cards_player1));
        System.Console.WriteLine($"Player {this.player1.getName()} on the discard-pile: " + Support.CardIdsHumanFormatted<OnTheDisposedPile>(cards_player1) + "\n");

        List<Card> cards_player2 = this.player2.getCards();
        this.printPlayer(this.player2);

        System.Console.WriteLine($" (ontheboard/indeck/inhand/indiscard-pile) {Support.countCards<OnTheBoard>(cards_player2)}/{Support.countCards<InTheDeck>(cards_player2)}/{Support.countCards<InTheHand>(cards_player2)}/{Support.countCards<OnTheDisposedPile>(cards_player2)}");
        System.Console.WriteLine($"Player {this.player2.getName()} on the board: " + Support.CardIdsHumanFormatted<OnTheBoard>(cards_player2));
        System.Console.WriteLine($"Player {this.player2.getName()} tapped lands: " + Support.CardIdsHumanFormatted<IsTapped>(cards_player2));
        System.Console.WriteLine($"Player {this.player2.getName()} in deck: " + Support.CardIdsHumanFormatted<InTheDeck>(cards_player2));
        System.Console.WriteLine($"Player {this.player2.getName()} in hand: " + Support.CardIdsHumanFormatted<InTheHand>(cards_player2));
        System.Console.WriteLine($"Player {this.player2.getName()} on the discard-pile: " + Support.CardIdsHumanFormatted<OnTheDisposedPile>(cards_player2));

        System.Console.WriteLine("<==== END Current situation>\n");
    }
}
