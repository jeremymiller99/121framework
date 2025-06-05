using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

public class SplitterModifier : ModifierSpell
{
    private float angle = 10f; // Angle in degrees
    private float manaMultiplier = 1.5f;

    public SplitterModifier(Spell innerSpell) : base(innerSpell)
    {
    }

    protected override string GetModifierName()
    {
        return "split";
    }

    protected override string GetModifierDescription()
    {
        return "Spell is cast twice in slightly different directions; increased mana cost.";
    }

    protected override void ApplyModifierAttributes(JObject attributes)
    {
        if (attributes["angle"] != null)
        {
            string angleExpr = attributes["angle"].ToString();
            angle = EvaluateExpressionFloat(angleExpr);
        }

        if (attributes["mana_multiplier"] != null)
        {
            string manaExpr = attributes["mana_multiplier"].ToString();
            manaMultiplier = EvaluateExpressionFloat(manaExpr);
        }

        // Apply the modifier
        appliedModifiers.AddManaCostModifier(manaMultiplier, ModifierType.Multiplicative);
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team, SpellModifiers modifiers = null)
    {
        // Combine modifiers
        SpellModifiers combinedModifiers = CombineModifiers(modifiers, appliedModifiers);
        
        // Calculate base direction
        Vector3 baseDirection = (target - where).normalized;
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x);
        
        // Calculate the two target positions
        float angleRad = angle * Mathf.Deg2Rad;
        float leftAngle = baseAngle - angleRad / 2f;
        float rightAngle = baseAngle + angleRad / 2f;
        
        Vector3 leftDirection = new Vector3(Mathf.Cos(leftAngle), Mathf.Sin(leftAngle), 0);
        Vector3 rightDirection = new Vector3(Mathf.Cos(rightAngle), Mathf.Sin(rightAngle), 0);
        
        Vector3 leftTarget = where + leftDirection * Vector3.Distance(where, target);
        Vector3 rightTarget = where + rightDirection * Vector3.Distance(where, target);
        
        // Cast both spells simultaneously
        var leftCast = innerSpell.Cast(where, leftTarget, team, combinedModifiers);
        var rightCast = innerSpell.Cast(where, rightTarget, team, combinedModifiers);
        
        yield return leftCast;
        yield return rightCast;
        
        // Set our last cast time
        last_cast = Time.time;
    }
} 