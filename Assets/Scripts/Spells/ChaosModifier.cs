using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

public class ChaosModifier : ModifierSpell
{
    private float damageMultiplier = 1.5f;
    private string projectileTrajectory = "spiraling";

    public ChaosModifier(Spell innerSpell) : base(innerSpell)
    {
    }

    protected override string GetModifierName()
    {
        return "chaotic";
    }

    protected override string GetModifierDescription()
    {
        return "Significantly increased damage, but projectile is spiraling.";
    }

    protected override void ApplyModifierAttributes(JObject attributes)
    {
        if (attributes["damage_multiplier"] != null)
        {
            string damageExpr = attributes["damage_multiplier"].ToString();
            damageMultiplier = EvaluateExpressionFloat(damageExpr);
        }

        if (attributes["projectile_trajectory"] != null)
        {
            projectileTrajectory = attributes["projectile_trajectory"].ToString();
        }

        // Apply the modifiers
        appliedModifiers.AddDamageModifier(damageMultiplier, ModifierType.Multiplicative);
    }

    protected override string GetModifiedTrajectory()
    {
        return projectileTrajectory;
    }
} 