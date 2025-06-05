using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

public class HomingModifier : ModifierSpell
{
    private float damageMultiplier = 0.75f;
    private int manaAdder = 10;
    private string projectileTrajectory = "homing";

    public HomingModifier(Spell innerSpell) : base(innerSpell)
    {
    }

    protected override string GetModifierName()
    {
        return "homing";
    }

    protected override string GetModifierDescription()
    {
        return "Homing projectile, with decreased damage and increased mana cost.";
    }

    protected override void ApplyModifierAttributes(JObject attributes)
    {
        if (attributes["damage_multiplier"] != null)
        {
            string damageExpr = attributes["damage_multiplier"].ToString();
            damageMultiplier = EvaluateExpressionFloat(damageExpr);
        }

        if (attributes["mana_adder"] != null)
        {
            string manaExpr = attributes["mana_adder"].ToString();
            manaAdder = EvaluateExpression(manaExpr);
        }

        if (attributes["projectile_trajectory"] != null)
        {
            projectileTrajectory = attributes["projectile_trajectory"].ToString();
        }

        // Apply the modifiers
        appliedModifiers.AddDamageModifier(damageMultiplier, ModifierType.Multiplicative);
        appliedModifiers.AddManaCostModifier(manaAdder, ModifierType.Additive);
    }

    protected override string GetModifiedTrajectory()
    {
        return projectileTrajectory;
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team, SpellModifiers modifiers = null)
    {
        this.team = team;
        last_cast = Time.time;

        // Apply modifiers to get final values
        int finalDamage = GetDamage();
        float finalSpeed = GetSpeed();
        string finalTrajectory = GetTrajectory(); // This will return "homing"
        int finalSprite = GetSprite();

        // Combine modifiers
        SpellModifiers combinedModifiers = CombineModifiers(modifiers, appliedModifiers);
        
        if (combinedModifiers != null)
        {
            finalDamage = ValueModifier.ApplyModifiers(finalDamage, combinedModifiers.damageModifiers);
            finalSpeed = ValueModifier.ApplyModifiersFloat(finalSpeed, combinedModifiers.speedModifiers);
        }

        // Create homing projectile
        Vector3 direction = target - where;
        var onHit = CreateStandardOnHit(finalDamage, damageType, team);
        
        GameManager.Instance.projectileManager.CreateProjectile(
            finalSprite, 
            finalTrajectory, // This should be "homing"
            where, 
            direction, 
            finalSpeed, 
            onHit
        );

        yield return new WaitForEndOfFrame();
    }
} 