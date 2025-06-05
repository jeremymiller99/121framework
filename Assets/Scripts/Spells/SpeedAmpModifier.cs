using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

public class SpeedAmpModifier : ModifierSpell
{
    private float speedMultiplier = 1.75f;

    public SpeedAmpModifier(Spell innerSpell) : base(innerSpell)
    {
    }

    protected override string GetModifierName()
    {
        return "speed-amplified";
    }

    protected override string GetModifierDescription()
    {
        return "Faster projectile speed";
    }

    protected override void ApplyModifierAttributes(JObject attributes)
    {
        if (attributes["speed_multiplier"] != null)
        {
            string speedExpr = attributes["speed_multiplier"].ToString();
            speedMultiplier = EvaluateExpressionFloat(speedExpr);
        }

        // Apply the modifier
        appliedModifiers.AddSpeedModifier(speedMultiplier, ModifierType.Multiplicative);
    }
} 