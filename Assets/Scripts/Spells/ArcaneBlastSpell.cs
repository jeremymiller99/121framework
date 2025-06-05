using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

public class ArcaneBlastSpell : Spell
{
    private int N; // Number of secondary projectiles
    private int secondaryDamage;
    private float secondarySpeed;
    private float secondaryLifetime;
    private int secondarySprite;

    public ArcaneBlastSpell(SpellCaster owner) : base(owner)
    {
        // Default values will be overridden by JSON
        N = 8;
        secondaryDamage = 5;
        secondarySpeed = 20f;
        secondaryLifetime = 0.1f;
        secondarySprite = 0;
    }

    public override void SetAttributes(JObject attributes)
    {
        base.SetAttributes(attributes);

        if (attributes["N"] != null)
        {
            string nExpr = attributes["N"].ToString();
            N = EvaluateExpression(nExpr);
        }

        if (attributes["secondary_damage"] != null)
        {
            string secDamageExpr = attributes["secondary_damage"].ToString();
            secondaryDamage = EvaluateExpression(secDamageExpr);
        }

        if (attributes["secondary_projectile"] != null)
        {
            var secProjectile = attributes["secondary_projectile"];
            if (secProjectile["speed"] != null)
            {
                string speedExpr = secProjectile["speed"].ToString();
                secondarySpeed = EvaluateExpressionFloat(speedExpr);
            }
            if (secProjectile["lifetime"] != null)
            {
                string lifetimeExpr = secProjectile["lifetime"].ToString();
                secondaryLifetime = EvaluateExpressionFloat(lifetimeExpr);
            }
            if (secProjectile["sprite"] != null)
                secondarySprite = (int)secProjectile["sprite"];
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
        int finalSecondaryDamage = secondaryDamage;

        if (modifiers != null)
        {
            finalDamage = ValueModifier.ApplyModifiers(finalDamage, modifiers.damageModifiers);
            finalSpeed = ValueModifier.ApplyModifiersFloat(finalSpeed, modifiers.speedModifiers);
            finalSecondaryDamage = ValueModifier.ApplyModifiers(finalSecondaryDamage, modifiers.damageModifiers);
        }

        // Create main projectile with explosion on hit
        Vector3 direction = target - where;
        var onHit = CreateExplosionOnHit(finalSecondaryDamage, damageType, team);
        
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

    private System.Action<Hittable, Vector3> CreateExplosionOnHit(int explosionDamage, Damage.Type damageType, Hittable.Team team)
    {
        return (Hittable other, Vector3 impact) =>
        {
            // Damage the hit target with primary damage
            if (other.team != team)
            {
                other.Damage(new Damage(GetDamage(), damageType));
            }

            // Create explosion of secondary projectiles
            float angleStep = 360f / N;
            for (int i = 0; i < N; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 explosionDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                
                var secondaryOnHit = CreateStandardOnHit(explosionDamage, damageType, team);
                
                GameManager.Instance.projectileManager.CreateProjectile(
                    secondarySprite,
                    "straight",
                    impact,
                    explosionDirection,
                    secondarySpeed,
                    secondaryOnHit,
                    secondaryLifetime
                );
            }
        };
    }
} 