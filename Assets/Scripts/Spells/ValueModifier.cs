using System.Collections.Generic;
using UnityEngine;

public enum ModifierType
{
    Additive,
    Multiplicative
}

public class ValueModifier
{
    public ModifierType type;
    public float value;

    public ValueModifier(ModifierType type, float value)
    {
        this.type = type;
        this.value = value;
    }

    public static int ApplyModifiers(int baseValue, List<ValueModifier> modifiers)
    {
        return Mathf.RoundToInt(ApplyModifiersFloat(baseValue, modifiers));
    }

    public static float ApplyModifiersFloat(float baseValue, List<ValueModifier> modifiers)
    {
        if (modifiers == null || modifiers.Count == 0)
            return baseValue;

        float result = baseValue;
        float multiplicativeProduct = 1f;

        // Apply all additive modifiers first
        foreach (var modifier in modifiers)
        {
            if (modifier.type == ModifierType.Additive)
            {
                result += modifier.value;
            }
        }

        // Then apply all multiplicative modifiers
        foreach (var modifier in modifiers)
        {
            if (modifier.type == ModifierType.Multiplicative)
            {
                multiplicativeProduct *= modifier.value;
            }
        }

        result *= multiplicativeProduct;
        return result;
    }
}

public class SpellModifiers
{
    public List<ValueModifier> damageModifiers;
    public List<ValueModifier> manaCostModifiers;
    public List<ValueModifier> cooldownModifiers;
    public List<ValueModifier> speedModifiers;

    public SpellModifiers()
    {
        damageModifiers = new List<ValueModifier>();
        manaCostModifiers = new List<ValueModifier>();
        cooldownModifiers = new List<ValueModifier>();
        speedModifiers = new List<ValueModifier>();
    }

    public void AddDamageModifier(float value, ModifierType type = ModifierType.Additive)
    {
        damageModifiers.Add(new ValueModifier(type, value));
    }

    public void AddManaCostModifier(float value, ModifierType type = ModifierType.Additive)
    {
        manaCostModifiers.Add(new ValueModifier(type, value));
    }

    public void AddCooldownModifier(float value, ModifierType type = ModifierType.Additive)
    {
        cooldownModifiers.Add(new ValueModifier(type, value));
    }

    public void AddSpeedModifier(float value, ModifierType type = ModifierType.Additive)
    {
        speedModifiers.Add(new ValueModifier(type, value));
    }
} 