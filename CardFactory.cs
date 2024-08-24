namespace TheCardGame;

abstract class Factory
{
    public abstract LandCard createLandCard(string cardId, int energylevel);
    public abstract SpellCard createSpellCard(string cardId, int energycost, SpellEffect spell);
    public abstract CreatureCard createCreatureCard(string cardId, int energycost, int effect, int attack, int defence);
}

class CardFactory : Factory
{
    public override LandCard createLandCard(string cardId, int energylevel)
    {
        ConcreteLandCard card = new ConcreteLandCard(cardId, energylevel);
        return card;

    }
    public override SpellCard createSpellCard(string cardId, int energycost, SpellEffect spell)
    {
        ConcreteSpellCard card = new ConcreteSpellCard(cardId, energycost, spell);
        return card;

    }
    public override CreatureCard createCreatureCard(string cardId, int energycost, int effect, int attack, int defence)
    {
        ConcreteCreatureCard card = new ConcreteCreatureCard(cardId, energycost, effect, attack, defence);
        return card;
    }
}

/*
The following classes are a concrete implementation for the abstract card types. 
This added layer of abstraction makes it easier to add new cardtypes by implenting 
new concrete classes, without having to touch the 'Cardfactory' (seperation of concerns).
*/
public class ConcreteLandCard : LandCard
{
    public ConcreteLandCard(string cardId, int energylevel) : base(cardId, energylevel) {}
}

public class ConcreteSpellCard : SpellCard
{
    public ConcreteSpellCard(string cardId, int energycost, SpellEffect effect) : base(cardId, energycost, effect) {}
}

public class ConcreteCreatureCard : CreatureCard
{
    public ConcreteCreatureCard(string cardId, int energy, int effect, int attack, int defence) : base(cardId, energy, effect, attack, defence) {}

}
