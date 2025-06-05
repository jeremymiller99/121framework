using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class ChainLightningModifier : ModifierSpell
{
    private int maxChains = 3;
    private float chainRange = 4f;
    private float damageReduction = 0.7f; // Each chain reduces damage to 70% of previous

    public ChainLightningModifier(Spell innerSpell) : base(innerSpell)
    {
    }

    protected override string GetModifierName()
    {
        return "chain lightning";
    }

    protected override string GetModifierDescription()
    {
        return "Spell chains to nearby enemies, dealing reduced damage to each subsequent target.";
    }

    protected override void ApplyModifierAttributes(JObject attributes)
    {
        if (attributes["max_chains"] != null)
        {
            string chainsExpr = attributes["max_chains"].ToString();
            maxChains = EvaluateExpression(chainsExpr);
        }

        if (attributes["chain_range"] != null)
        {
            string rangeExpr = attributes["chain_range"].ToString();
            chainRange = EvaluateExpressionFloat(rangeExpr);
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

        // Create chaining projectile with special OnHit behavior
        Vector3 direction = target - where;
        var onHit = CreateChainingOnHit(finalDamage, damageType, team);
        
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

    private System.Action<Hittable, Vector3> CreateChainingOnHit(int damage, Damage.Type damageType, Hittable.Team team)
    {
        HashSet<Hittable> hitTargets = new HashSet<Hittable>();

        return (Hittable other, Vector3 impact) =>
        {
            if (other.team != team && !hitTargets.Contains(other))
            {
                // Deal damage to initial target
                other.Damage(new Damage(damage, damageType));
                hitTargets.Add(other);
                
                // Start chain reaction
                StartChainReaction(other.owner.transform.position, damage, damageType, team, hitTargets, 0);
            }
        };
    }

    private void StartChainReaction(Vector3 fromPosition, int currentDamage, Damage.Type damageType, Hittable.Team team, HashSet<Hittable> hitTargets, int chainCount)
    {
        if (chainCount >= maxChains) return;

        // Find nearest enemy within chain range
        Collider2D[] colliders = Physics2D.OverlapCircleAll(fromPosition, chainRange);
        Hittable nearestTarget = null;
        float nearestDistance = float.MaxValue;

        foreach (var collider in colliders)
        {
            // Check for EnemyController or PlayerController
            var enemyController = collider.GetComponent<EnemyController>();
            var playerController = collider.GetComponent<PlayerController>();
            
            Hittable hittable = null;
            if (enemyController != null)
            {
                hittable = enemyController.hp;
            }
            else if (playerController != null)
            {
                hittable = playerController.hp;
            }
            
            if (hittable != null && hittable.team != team && !hitTargets.Contains(hittable))
            {
                float distance = Vector3.Distance(fromPosition, collider.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTarget = hittable;
                }
            }
        }

        if (nearestTarget != null)
        {
            // Calculate reduced damage
            int chainDamage = Mathf.RoundToInt(currentDamage * damageReduction);
            
            // Deal damage
            nearestTarget.Damage(new Damage(chainDamage, damageType));
            hitTargets.Add(nearestTarget);
            
            Debug.Log($"Chain lightning #{chainCount + 1}: {chainDamage} damage to {nearestTarget.owner.name}");
            
            // Create visual chain effect (simple projectile)
            Vector3 direction = nearestTarget.owner.transform.position - fromPosition;
            var chainOnHit = CreateStandardOnHit(0, damageType, team); // No damage since we already dealt it
            
            GameManager.Instance.projectileManager.CreateProjectile(
                GetSprite(),
                "straight",
                fromPosition,
                direction,
                20f, // Fast chain projectile
                chainOnHit,
                0.1f // Short lifetime
            );
            
            // Continue chain
            StartChainReaction(nearestTarget.owner.transform.position, chainDamage, damageType, team, hitTargets, chainCount + 1);
        }
        else
        {
            Debug.Log($"Chain lightning ended at chain #{chainCount} - no more targets in range");
        }
    }
} 