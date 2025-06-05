using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

public class DoublerModifier : ModifierSpell
{
    private float delay = 0.5f;
    private float manaMultiplier = 1.5f;
    private float cooldownMultiplier = 1.5f;

    public DoublerModifier(Spell innerSpell) : base(innerSpell)
    {
    }

    protected override string GetModifierName()
    {
        return "doubled";
    }

    protected override string GetModifierDescription()
    {
        return "Spell is cast a second time after a small delay; increased mana cost and cooldown.";
    }

    protected override void ApplyModifierAttributes(JObject attributes)
    {
        if (attributes["delay"] != null)
        {
            string delayExpr = attributes["delay"].ToString();
            delay = EvaluateExpressionFloat(delayExpr);
        }

        if (attributes["mana_multiplier"] != null)
        {
            string manaExpr = attributes["mana_multiplier"].ToString();
            manaMultiplier = EvaluateExpressionFloat(manaExpr);
        }

        if (attributes["cooldown_multiplier"] != null)
        {
            string cooldownExpr = attributes["cooldown_multiplier"].ToString();
            cooldownMultiplier = EvaluateExpressionFloat(cooldownExpr);
        }

        // Apply the modifiers
        appliedModifiers.AddManaCostModifier(manaMultiplier, ModifierType.Multiplicative);
        appliedModifiers.AddCooldownModifier(cooldownMultiplier, ModifierType.Multiplicative);
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team, SpellModifiers modifiers = null)
    {
        // Combine modifiers
        SpellModifiers combinedModifiers = CombineModifiers(modifiers, appliedModifiers);
        
        // Cast the first spell
        yield return innerSpell.Cast(where, target, team, combinedModifiers);
        
        // Wait for delay
        yield return new WaitForSeconds(delay);
        
        // Cast the second spell
        yield return innerSpell.Cast(where, target, team, combinedModifiers);
        
        // Set our last cast time
        last_cast = Time.time;
    }
} 