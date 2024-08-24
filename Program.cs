namespace TheCardGame;

class Program
{
    public static void Wait()
    {
        System.Console.WriteLine("Press Enter to continue...");
        System.Console.ReadLine();
    }
    
    public static void exampleGame(GameBoard gb){
        //<Round 1>Turn 1 - player 1
        if (!gb.newTurn()) {return;}
        gb.drawCard("Land_1");
        gb.drawCard("Land_2");
        gb.endTurn();
        gb.log();

        Wait();//<Round 1>Turn 2 - player 2
        if (!gb.newTurn()) {return;}
        gb.drawCard("Land_17");
        gb.endTurn();
        gb.log();

        Wait();//<Roudn 2>Turn 3 - player 1
        if (!gb.newTurn()) {return;}
        gb.drawCard("Land_3");
        gb.tapFromCard("Land_1");
        gb.tapFromCard("Land_2");
        gb.drawCard("Creature_1");
        gb.endTurn();
        gb.log();

        Wait();//<Round 2>Turn 4 - player 2
        if (!gb.newTurn()) {return;}
        gb.endTurn();
        gb.log();

        Wait();//<Round 3>Turn 5 - Player 1
        if (!gb.newTurn()) {return;}
        gb.tapFromCard("Land_1");
        gb.addSpell("Spell_1", "Creature_1");
        //>Player 2 interupts to cast spell
        gb.tapFromCard("Land_17");
        gb.addSpell("Spell_16", "Creature_1");
        //>Player 1 casts another spell to counter spell_16 from player 2
        gb.tapFromCard("Land_2");
        gb.addSpell("Spell_2", "Creature_1");
        //>Resolution of the interuption stack is applied
        gb.resolveSpellStack();
        //>Player 1 attacks with Creature_1 with Spell_2 applied. player 2 has no creatures to defend.
        gb.letsAttack("Creature_1", new List<string>(){});
        gb.log();
        // End Example game P1: 10hp, p2: 5hp
    }
    
    static void Main(string[] args)
    {
        Player Player1 = new Player("wizard1", 10);
        Player Player2 = new Player("wizard2", 10);

        CardFactory factory = new CardFactory();

        //Commented out some card to keep console clean
        List<Card> player1_deck = new List<Card>
        {            
            factory.createLandCard("Land_1", 1),
            factory.createLandCard("Land_2", 1),
            factory.createLandCard("Land_3", 1),
            factory.createSpellCard("Spell_1", 1, new SpellEffect(SpellEffectType.Temporary, 3, 3)),
            factory.createSpellCard("Spell_2", 1, new SpellEffect(SpellEffectType.Temporary, 3, 3)),
            factory.createCreatureCard("Creature_1", 1, 0, 2, 2),
            factory.createSpellCard("Spell_3", 1, new SpellEffect(SpellEffectType.Temporary, 1, 1)),
            factory.createSpellCard("Spell_4", 1, new SpellEffect(SpellEffectType.Temporary, 1, 1)),
            factory.createSpellCard("Spell_5", 1, new SpellEffect(SpellEffectType.Temporary, 1, 1)),
            factory.createLandCard("Land_4", 1),
            factory.createLandCard("Land_5", 1),
            factory.createLandCard("Land_6", 1),
            factory.createLandCard("Land_7", 1),
            factory.createSpellCard("Spell_6", 1, new SpellEffect(SpellEffectType.Temporary, 3, 3)),
            factory.createSpellCard("Spell_7", 1, new SpellEffect(SpellEffectType.Temporary, 3, 3)),
            // factory.createCreatureCard("Creature_2", 1, 0, 2, 2),
            // factory.createSpellCard("Spell_8", 1, 1),
            // factory.createSpellCard("Spell_9", 1, 2),
            // factory.createSpellCard("Spell_10", 1, 2),
            // factory.createLandCard("Land_8", 1),
            // factory.createLandCard("Land_9", 1),
            // factory.createLandCard("Land_10", 1),
            // factory.createLandCard("Land_11", 1),
            // factory.createSpellCard("Spell_11", 1, 2),
            // factory.createSpellCard("Spell_12", 1, 2),
            // factory.createCreatureCard("Creature_3", 1, 0, 2, 2),
            // factory.createSpellCard("Spell_13", 1, 3),
            // factory.createSpellCard("Spell_14", 1, 3),
            // factory.createSpellCard("Spell_15", 1, 3),
            // factory.createLandCard("Land_16", 1)
        };
        //Commented out some card to keep console clean
        List<Card> player2_deck = new List<Card>
        {
            factory.createLandCard("Land_17", 1),
            factory.createLandCard("Land_18", 1),
            factory.createLandCard("Land_19", 1),
            factory.createSpellCard("Spell_16", 1, new SpellEffect(SpellEffectType.Temporary, -3, -3)),
            factory.createSpellCard("Spell_17", 1, new SpellEffect(SpellEffectType.Temporary, 3, 3)),
            factory.createCreatureCard("Creature_4", 1, 0, 2, 2),
            factory.createSpellCard("Spell_18", 1, new SpellEffect(SpellEffectType.Temporary, 3, 3)),
            factory.createSpellCard("Spell_19", 1, new SpellEffect(SpellEffectType.Temporary, 3, 3)),
            factory.createSpellCard("Spell_20", 1, new SpellEffect(SpellEffectType.Temporary, 3, 3)),
            factory.createLandCard("Land_20", 1),
            factory.createLandCard("Land_21", 1),
            factory.createLandCard("Land_22", 1),
            factory.createLandCard("Land_23", 1),
            factory.createSpellCard("Spell_21", 1, new SpellEffect(SpellEffectType.Temporary, 3, 3)),
            factory.createSpellCard("Spell_22", 1, new SpellEffect(SpellEffectType.Temporary, 3, 3)),
            // factory.createCreatureCard("Creature_5", 1, 0, 2, 2),
            // factory.createSpellCard("Spell_23", 1, 1),
            // factory.createSpellCard("Spell_24", 1, 2),
            // factory.createSpellCard("Spell_25", 1, 2),
            // factory.createLandCard("Land_24", 1),
            // factory.createLandCard("Land_25", 1),
            // factory.createLandCard("Land_26", 1),
            // factory.createLandCard("Land_27", 1),
            // factory.createSpellCard("Spell_26", 1, 2),
            // factory.createSpellCard("Spell_27", 1, 2),
            // factory.createCreatureCard("Creature_5", 1, 0, 2, 2),
            // factory.createSpellCard("Spell_28", 1, 3),
            // factory.createSpellCard("Spell_29", 1, 3),
            // factory.createSpellCard("Spell_30", 1, 3),
            // factory.createLandCard("Land_28", 1)
        };

        Player1.setCards(player1_deck);
        Player2.setCards(player2_deck);

        GameBoard gb = GameBoard.getGameBoard(Player1, Player2);

        gb.log();
        
        Wait();
        
        //Start the sample game from the assignment description:
        exampleGame(gb);
    }
}