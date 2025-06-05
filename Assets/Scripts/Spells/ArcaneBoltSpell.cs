using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

public class ArcaneBoltSpell : Spell
{
    public ArcaneBoltSpell(SpellCaster owner) : base(owner)
    {
        // Default values will be overridden by JSON
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

        // Create projectile
        Vector3 direction = target - where;
        var onHit = CreateStandardOnHit(finalDamage, damageType, team);
        
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
} 