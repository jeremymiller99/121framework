using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public abstract class ModifierSpell : Spell
{
    public Spell innerSpell; // Made public so we can access it in the UI
    protected SpellModifiers appliedModifiers;

    public ModifierSpell(Spell innerSpell) : base(innerSpell.owner)
    {
        this.innerSpell = innerSpell;
        this.appliedModifiers = new SpellModifiers();
        
        // Copy power and wave from inner spell
        this.spellPower = innerSpell.spellPower;
        this.currentWave = innerSpell.currentWave;
        
        // Copy existing modifiers from inner spell
        this.modifiers = new List<string>(innerSpell.GetModifiers());
        
        // Add this modifier to the list
        this.modifiers.Add(GetModifierName());
        
        // Update rarity based on total modifier count
        UpdateRarityFromModifiers();
    }

    public override string GetName()
    {
        return GetModifierName() + " " + innerSpell.GetName();
    }

    public override string GetDescription()
    {
        return GetModifierDescription() + " " + innerSpell.GetDescription();
    }

    public override int GetIcon()
    {
        return innerSpell.GetIcon();
    }
    
    public override List<string> GetModifiers()
    {
        return new List<string>(modifiers);
    }

    public override int GetDamage()
    {
        int baseDamage = innerSpell.GetDamage();
        return ValueModifier.ApplyModifiers(baseDamage, appliedModifiers.damageModifiers);
    }

    public override int GetManaCost()
    {
        int baseCost = innerSpell.GetManaCost();
        return ValueModifier.ApplyModifiers(baseCost, appliedModifiers.manaCostModifiers);
    }

    public override float GetCooldown()
    {
        float baseCooldown = innerSpell.GetCooldown();
        return ValueModifier.ApplyModifiersFloat(baseCooldown, appliedModifiers.cooldownModifiers);
    }

    public override float GetSpeed()
    {
        float baseSpeed = innerSpell.GetSpeed();
        return ValueModifier.ApplyModifiersFloat(baseSpeed, appliedModifiers.speedModifiers);
    }

    public override string GetTrajectory()
    {
        return GetModifiedTrajectory();
    }

    public override int GetSprite()
    {
        return innerSpell.GetSprite();
    }

    public override void SetAttributes(JObject attributes)
    {
        // Apply modifier-specific attributes
        ApplyModifierAttributes(attributes);
    }

    // Abstract methods for modifier-specific behavior
    protected abstract string GetModifierName();
    protected abstract string GetModifierDescription();
    protected abstract void ApplyModifierAttributes(JObject attributes);
    protected virtual string GetModifiedTrajectory()
    {
        return innerSpell.GetTrajectory();
    }

    // Base implementation that can be overridden for complex modifier behavior
    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team, SpellModifiers modifiers = null)
    {
        // Combine modifiers
        SpellModifiers combinedModifiers = CombineModifiers(modifiers, appliedModifiers);
        
        // Cast the inner spell with combined modifiers
        yield return innerSpell.Cast(where, target, team, combinedModifiers);
        
        // Set our last cast time
        last_cast = Time.time;
    }

    protected SpellModifiers CombineModifiers(SpellModifiers externalModifiers, SpellModifiers internalModifiers)
    {
        SpellModifiers combined = new SpellModifiers();
        
        // Add external modifiers first
        if (externalModifiers != null)
        {
            combined.damageModifiers.AddRange(externalModifiers.damageModifiers);
            combined.manaCostModifiers.AddRange(externalModifiers.manaCostModifiers);
            combined.cooldownModifiers.AddRange(externalModifiers.cooldownModifiers);
            combined.speedModifiers.AddRange(externalModifiers.speedModifiers);
        }
        
        // Add internal modifiers
        combined.damageModifiers.AddRange(internalModifiers.damageModifiers);
        combined.manaCostModifiers.AddRange(internalModifiers.manaCostModifiers);
        combined.cooldownModifiers.AddRange(internalModifiers.cooldownModifiers);
        combined.speedModifiers.AddRange(internalModifiers.speedModifiers);
        
        return combined;
    }
} 