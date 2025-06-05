using UnityEngine;
using System.Collections.Generic;

public static class SpellModifierInfo
{
    private static Dictionary<string, ModifierInfo> modifierData = new Dictionary<string, ModifierInfo>
    {
        ["damage-amplified"] = new ModifierInfo
        {
            displayName = "Damage Amplified",
            description = "60% more damage, 40% more mana cost",
            icon = "",
            effect = "Enhanced destructive power"
        },
        ["speed-amplified"] = new ModifierInfo
        {
            displayName = "Speed Amplified", 
            description = "2x projectile speed",
            icon = "",
            effect = "Lightning-fast projectiles"
        },
        ["doubled"] = new ModifierInfo
        {
            displayName = "Doubled",
            description = "Casts twice with a delay",
            icon = "",
            effect = "Double the magical fury"
        },
        ["split"] = new ModifierInfo
        {
            displayName = "Split",
            description = "Fires in two directions",
            icon = "",
            effect = "Branching magical energy"
        },
        ["chaotic"] = new ModifierInfo
        {
            displayName = "Chaotic",
            description = "80% more damage, spiraling trajectory",
            icon = "",
            effect = "Unpredictable but powerful"
        },
        ["homing"] = new ModifierInfo
        {
            displayName = "Homing",
            description = "Seeks enemies automatically",
            icon = "",
            effect = "Never miss your target"
        },
        ["piercing"] = new ModifierInfo
        {
            displayName = "Piercing",
            description = "Passes through multiple enemies",
            icon = "",
            effect = "Penetrates enemy defenses"
        },
        ["chain lightning"] = new ModifierInfo
        {
            displayName = "Chain Lightning",
            description = "Jumps between nearby enemies",
            icon = "",
            effect = "Chains destruction"
        }
    };
    
    public static ModifierInfo GetInfo(string modifierName)
    {
        if (modifierData.TryGetValue(modifierName, out ModifierInfo info))
        {
            return info;
        }
        
        // Fallback for unknown modifiers
        return new ModifierInfo
        {
            displayName = char.ToUpper(modifierName[0]) + modifierName.Substring(1),
            description = "Unknown modifier effect",
            icon = "?",
            effect = "Mysterious enhancement"
        };
    }
    
    public static string GetStackedName(string modifierName, int count)
    {
        var info = GetInfo(modifierName);
        
        if (count == 1)
            return info.displayName;
        else if (count == 2)
            return $"{info.displayName} DOUBLED";
        else if (count == 3)
            return $"{info.displayName} TRIPLED";
        else
            return $"{info.displayName} x{count}";
    }
    
    public static string GetStackedDescription(string modifierName, int count)
    {
        var info = GetInfo(modifierName);
        
        if (count == 1)
            return info.effect;
        else if (count == 2)
            return $"{info.effect} - DOUBLED EFFECT!";
        else if (count == 3)
            return $"{info.effect} - TRIPLED EFFECT!";
        else
            return $"{info.effect} - {count}x EFFECT!";
    }
}

public class ModifierInfo
{
    public string displayName;
    public string description;
    public string icon;
    public string effect;
} 