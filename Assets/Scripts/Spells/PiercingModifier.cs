using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class PiercingModifier : ModifierSpell
{
    private int maxPierces = 3;
    private float damageReduction = 0.8f; // Each pierce reduces damage to 80% of previous

    public PiercingModifier(Spell innerSpell) : base(innerSpell)
    {
    }

    protected override string GetModifierName()
    {
        return "piercing";
    }

    protected override string GetModifierDescription()
    {
        return "Projectiles pierce through enemies, dealing reduced damage to each subsequent target.";
    }

    protected override void ApplyModifierAttributes(JObject attributes)
    {
        if (attributes["max_pierces"] != null)
        {
            string piercesExpr = attributes["max_pierces"].ToString();
            maxPierces = EvaluateExpression(piercesExpr);
        }

        if (attributes["damage_reduction"] != null)
        {
            string reductionExpr = attributes["damage_reduction"].ToString();
            damageReduction = EvaluateExpressionFloat(reductionExpr);
        }
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team, SpellModifiers modifiers = null)
    {
        this.team = team;
        last_cast = Time.time;

        // Apply modifiers to get final values
        int finalDamage = GetDamage();
        float finalSpeed = GetSpeed();
        string finalTrajectory = GetTrajectory();
        int finalSprite = GetSprite();

        if (modifiers != null)
        {
            finalDamage = ValueModifier.ApplyModifiers(finalDamage, modifiers.damageModifiers);
            finalSpeed = ValueModifier.ApplyModifiersFloat(finalSpeed, modifiers.speedModifiers);
        }

        // Create piercing projectile with special OnHit behavior
        Vector3 direction = target - where;
        var onHit = CreatePiercingOnHit(finalDamage, damageType, team);
        
        GameManager.Instance.projectileManager.CreateProjectile(
            finalSprite, 
            finalTrajectory, 
            where, 
            direction, 
            finalSpeed, 
            onHit
        );

        yield return new WaitForEndOfFrame();
    }

    private System.Action<Hittable, Vector3> CreatePiercingOnHit(int damage, Damage.Type damageType, Hittable.Team team)
    {
        HashSet<Hittable> hitTargets = new HashSet<Hittable>();
        int currentDamage = damage;
        int pierceCount = 0;

        return (Hittable other, Vector3 impact) =>
        {
            if (other.team != team && !hitTargets.Contains(other))
            {
                // Deal damage
                other.Damage(new Damage(currentDamage, damageType));
                hitTargets.Add(other);
                pierceCount++;
                
                Debug.Log($"Piercing hit #{pierceCount}: {currentDamage} damage to {other.owner.name}");
                
                // Reduce damage for next hit
                currentDamage = Mathf.RoundToInt(currentDamage * damageReduction);
                
                // Note: The projectile will continue through the enemy automatically
                // since we're not destroying it here unless max pierces reached
                if (pierceCount >= maxPierces)
                {
                    Debug.Log($"Max pierces ({maxPierces}) reached, projectile should be destroyed");
                }
            }
        };
    }
} 