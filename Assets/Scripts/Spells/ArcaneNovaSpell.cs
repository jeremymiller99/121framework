using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

public class ArcaneNovaSpell : Spell
{
    private float radius = 5f;
    private float knockbackForce = 10f;

    public ArcaneNovaSpell(SpellCaster owner) : base(owner)
    {
        // Default values will be overridden by JSON
        radius = 5f;
        knockbackForce = 10f;
    }

    public override void SetAttributes(JObject attributes)
    {
        base.SetAttributes(attributes);

        if (attributes["radius"] != null)
        {
            string radiusExpr = attributes["radius"].ToString();
            radius = EvaluateExpressionFloat(radiusExpr);
        }

        if (attributes["knockback_force"] != null)
        {
            string knockbackExpr = attributes["knockback_force"].ToString();
            knockbackForce = EvaluateExpressionFloat(knockbackExpr);
        }
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team, SpellModifiers modifiers = null)
    {
        this.team = team;
        last_cast = Time.time;

        // Apply modifiers to get final values
        int finalDamage = GetDamage();
        float finalRadius = radius;

        if (modifiers != null)
        {
            finalDamage = ValueModifier.ApplyModifiers(finalDamage, modifiers.damageModifiers);
        }

        // Find all enemies within radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(where, finalRadius);
        
        foreach (var collider in colliders)
        {
            // Check if this collider has an EnemyController (for enemies) or PlayerController (for player)
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
            
            if (hittable != null && hittable.team != team)
            {
                // Deal damage
                hittable.Damage(new Damage(finalDamage, damageType));
                
                // Apply knockback
                Vector3 knockbackDirection = (collider.transform.position - where).normalized;
                var rigidbody = collider.GetComponent<Rigidbody2D>();
                if (rigidbody != null)
                {
                    rigidbody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
                }
                
                Debug.Log($"ArcaneNova hit {collider.name} for {finalDamage} damage");
            }
        }

        // Visual effect - create a brief expanding circle or particle effect
        // For now, we'll just create a short-lived projectile at the cast location
        var onHit = CreateStandardOnHit(0, damageType, team); // No damage since we already dealt it
        GameManager.Instance.projectileManager.CreateProjectile(
            GetSprite(),
            "straight",
            where,
            Vector3.zero, // No movement
            0f, // No speed
            onHit,
            0.2f // Short lifetime for visual effect
        );

        yield return new WaitForEndOfFrame();
    }
} 