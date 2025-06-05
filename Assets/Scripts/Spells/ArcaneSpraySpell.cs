using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

public class ArcaneSpraySpell : Spell
{
    private int N; // Number of projectiles
    private float spray; // Spray angle in radians
    private float projectileLifetime;

    public ArcaneSpraySpell(SpellCaster owner) : base(owner)
    {
        // Default values will be overridden by JSON
        N = 7;
        spray = 0.3f;
        projectileLifetime = 0.1f;
    }

    public override void SetAttributes(JObject attributes)
    {
        base.SetAttributes(attributes);

        if (attributes["N"] != null)
        {
            string nExpr = attributes["N"].ToString();
            N = EvaluateExpression(nExpr);
        }

        if (attributes["spray"] != null)
        {
            string sprayExpr = attributes["spray"].ToString();
            spray = EvaluateExpressionFloat(sprayExpr);
        }

        // Parse projectile lifetime if specified
        if (attributes["projectile"] != null && attributes["projectile"]["lifetime"] != null)
        {
            string lifetimeExpr = attributes["projectile"]["lifetime"].ToString();
            projectileLifetime = EvaluateExpressionFloat(lifetimeExpr);
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

        // Calculate base direction
        Vector3 baseDirection = (target - where).normalized;
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x);

        // Create projectiles in a spray pattern
        var onHit = CreateStandardOnHit(finalDamage, damageType, team);
        
        for (int i = 0; i < N; i++)
        {
            // Calculate angle offset for this projectile
            float angleOffset;
            if (N == 1)
            {
                angleOffset = 0;
            }
            else
            {
                // Spread projectiles evenly across the spray angle
                float t = (float)i / (N - 1); // 0 to 1
                angleOffset = (t - 0.5f) * spray; // -spray/2 to +spray/2
            }

            float projectileAngle = baseAngle + angleOffset;
            Vector3 projectileDirection = new Vector3(Mathf.Cos(projectileAngle), Mathf.Sin(projectileAngle), 0);

            GameManager.Instance.projectileManager.CreateProjectile(
                finalSprite,
                finalTrajectory,
                where,
                projectileDirection,
                finalSpeed,
                onHit,
                projectileLifetime
            );
        }

        yield return new WaitForEndOfFrame();
    }
} 