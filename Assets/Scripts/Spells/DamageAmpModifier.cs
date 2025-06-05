using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

public class DamageAmpModifier : ModifierSpell
{
    private float damageMultiplier = 1.5f;
    private float manaMultiplier = 1.5f;

    public DamageAmpModifier(Spell innerSpell) : base(innerSpell)
    {
    }

    protected override string GetModifierName()
    {
        return "damage-amplified";
    }

    protected override string GetModifierDescription()
    {
        return "Increased damage and increased mana cost.";
    }

    protected override void ApplyModifierAttributes(JObject attributes)
    {
        if (attributes["damage_multiplier"] != null)
        {
            string damageExpr = attributes["damage_multiplier"].ToString();
            damageMultiplier = EvaluateExpressionFloat(damageExpr);
        }

        if (attributes["mana_multiplier"] != null)
        {
            string manaExpr = attributes["mana_multiplier"].ToString();
            manaMultiplier = EvaluateExpressionFloat(manaExpr);
        }

        // Apply the modifiers
        appliedModifiers.AddDamageModifier(damageMultiplier, ModifierType.Multiplicative);
        appliedModifiers.AddManaCostModifier(manaMultiplier, ModifierType.Multiplicative);
    }
} 