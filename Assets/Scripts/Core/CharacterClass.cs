using System.Collections.Generic;

[System.Serializable]
public class CharacterClass
{
    public string name;
    public int sprite;
    public string healthFormula;
    public string manaFormula;
    public string manaRegenerationFormula;
    public string spellPowerFormula;
    public string speedFormula;

    public CharacterClass(string name, int sprite, string health, string mana, string manaReg, string spellPower, string speed)
    {
        this.name = name;
        this.sprite = sprite;
        this.healthFormula = health;
        this.manaFormula = mana;
        this.manaRegenerationFormula = manaReg;
        this.spellPowerFormula = spellPower;
        this.speedFormula = speed;
    }

    public int CalculateHealth(int wave)
    {
        var variables = new Dictionary<string, int> { ["wave"] = wave };
        return RPNEvaluator.Evaluate(healthFormula, variables);
    }

    public int CalculateMana(int wave)
    {
        var variables = new Dictionary<string, int> { ["wave"] = wave };
        return RPNEvaluator.Evaluate(manaFormula, variables);
    }

    public int CalculateManaRegeneration(int wave)
    {
        var variables = new Dictionary<string, int> { ["wave"] = wave };
        return RPNEvaluator.Evaluate(manaRegenerationFormula, variables);
    }

    public int CalculateSpellPower(int wave)
    {
        var variables = new Dictionary<string, int> { ["wave"] = wave };
        return RPNEvaluator.Evaluate(spellPowerFormula, variables);
    }

    public int CalculateSpeed(int wave)
    {
        var variables = new Dictionary<string, int> { ["wave"] = wave };
        return RPNEvaluator.Evaluate(speedFormula, variables);
    }
} 