using System.Dynamic;
using Microsoft.VisualBasic;

namespace TheCardGame;

public enum SpellEffectType
{
    Temporary,
    Permanent
}

public abstract class Card
{
    private int energyCost = 0; //The amount of energy required to play this card 
    private string description;
    private string cardId; //The unique id of this card in the game
    private CardState theState;

    public Card(string cardId, int energy)
    {
        this.cardId = cardId;
        this.description = string.Empty;
        this.theState = new InTheDeck(this);
        this.energyCost = energy;
    }

    public CardState State
    {
        get { return theState; }
        set { theState = value; }
    }

    public string getId()
    {
        return this.cardId;
    }

    public int getEnergyCost()
    {
        return this.energyCost;
    }

    public virtual int subtractDefenseValue(int iAttackValue) { throw new NotImplementedException(); }

    public virtual int getActualAttackValue() { throw new NotImplementedException(); }

    public virtual int getEnergyLevel() { throw new NotImplementedException(); }

    public virtual int getEffectCreature() { throw new NotImplementedException(); }

    public virtual bool dispose()
    {
        return this.State.dispose();
    }

    public virtual void onEndTurn()
    {
        this.State.onEndTurn();
    }

    public bool onDraw()
    {
        return this.State.onDraw();
    }

    public bool onIsTaken()
    {
        return this.State.onIsTaken();
    }

    public bool isNotYetInTheGame()
    {
        return this.State.isNotYetInTheGame();
    }

    public virtual void goDefending() { }

    public virtual void goAttacking() { }

    public virtual void tapEnergy() { }
}

public class SpellEffect
{
    public SpellEffectType Type { get; }
    public int attackModifier { get; }
    public int defenseModifier { get; }

    public SpellEffect(SpellEffectType type, int attackModifier, int defenseModifier)
    {
        this.Type = type;
        this.attackModifier = attackModifier;
        this.defenseModifier = defenseModifier;
    }
}

public abstract class LandCard : Card
{
    /* Provides the energy to play the other cards */
    private int _energyLevel = 0;

    public LandCard(string cardId, int energylevel) : base(cardId, 0)
    {
        _energyLevel = energylevel;
    }

    public override int getEnergyLevel()
    {
        return this._energyLevel;
    }

    public override void tapEnergy()
    {
        this.State.tapEnergy();
    }


}

public abstract class SpellCard : Card
{
    private SpellEffect spelleffect;

    public SpellCard(string cardId, int energy, SpellEffect spell) : base(cardId, energy)
    {
        this.spelleffect = spell;
    }

    public SpellEffect getEffect()
    {
        return this.spelleffect;
    }

    public void applySpellEffect(CreatureCard targetCreature)
    {
        //Apply temporary or permanent spell based on the effect type
        switch (spelleffect.Type)
        {
            case SpellEffectType.Temporary:
                targetCreature.ApplyModification(spelleffect.attackModifier, spelleffect.defenseModifier);
                break;
            case SpellEffectType.Permanent:
                //Code to cast a permanent spell
                break;
            default:
                break;
        }
    }
}

public abstract class CreatureCard : Card
{
    /*Used to attack opponenent (decrease opponent lifePoint) or for defense.*/
    private int baseAttack; //The initial attack value defined on this card
    private int attackModifier; //The later to add or subtract modifier from the cast of spells
    private int baseDefence; //The initial defence value defined on this card
    private int defenceModifier; //The later to add or subtract modifier from the cast of spells
    private int effect; //The effect a creature may or may not have as a position in the effectlist in the player class

    public CreatureCard(string cardId, int energycost, int effect, int attack, int defence) : base(cardId, energycost)
    {
        this.effect = effect;
        this.baseAttack = attack;
        this.baseDefence = defence;
    }

    public int GetAttack()
    {
        return this.baseAttack + this.attackModifier;
    }

    public int GetDefence()
    {
        return this.baseDefence + this.defenceModifier;
    }

    public void ApplyModification(int attackModifier, int defenseModifier)
    {
        this.attackModifier += attackModifier;
        this.defenceModifier += defenseModifier;
        Console.WriteLine($"[Card] Modified attack/defence values for {this.getId()}: {this.baseAttack + this.attackModifier},{this.baseDefence + this.defenceModifier}");
    }

    public void RemoveModification(int attackModifier, int defenseModifier)
    {
        this.attackModifier -= attackModifier;
        this.defenceModifier -= defenseModifier;
        Console.WriteLine($"[Card] Modified attack/defence values for {this.getId()}: {this.baseAttack + this.attackModifier},{this.baseDefence + this.defenceModifier}");

    }

    public override int getEffectCreature()
    {
        return effect;
    }

    public override void goDefending()
    {
        this.State.goDefending();
    }

    public override void goAttacking()
    {
        this.State.goAttacking();
    }

    public override int subtractDefenseValue(int iAttackValue)
    {
        // Calculate the effective defense value
        int effectiveDefense = GetDefence();

        // Subtracts the attack value from the effective defense
        effectiveDefense -= iAttackValue;

        // Ensures the effective defense doesn't go below 0
        if (effectiveDefense < 0)
        {
            effectiveDefense = 0;
        }

        // Update the defense modifier if needed
        this.defenceModifier = effectiveDefense - baseDefence;

        return effectiveDefense;
    }
}