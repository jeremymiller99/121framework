using UnityEngine;

public enum SpellRarity
{
    Common,     // 0 modifiers - White/Gray
    Uncommon,   // 1 modifier - Green
    Rare,       // 2 modifiers - Blue
    Legendary   // 3+ modifiers - Orange/Gold
}

public static class SpellRarityExtensions
{
    public static string GetDisplayName(this SpellRarity rarity)
    {
        switch (rarity)
        {
            case SpellRarity.Common: return "Common";
            case SpellRarity.Uncommon: return "Uncommon";
            case SpellRarity.Rare: return "Rare";
            case SpellRarity.Legendary: return "Legendary";
            default: return "Common";
        }
    }
    
    public static Color GetColor(this SpellRarity rarity)
    {
        switch (rarity)
        {
            case SpellRarity.Common: return new Color(0.8f, 0.8f, 0.8f); // Light Gray
            case SpellRarity.Uncommon: return new Color(0.2f, 0.8f, 0.2f); // Green
            case SpellRarity.Rare: return new Color(0.2f, 0.5f, 1.0f); // Blue
            case SpellRarity.Legendary: return new Color(1.0f, 0.6f, 0.0f); // Orange
            default: return Color.white;
        }
    }
    
    public static string GetPrefix(this SpellRarity rarity)
    {
        switch (rarity)
        {
            case SpellRarity.Common: return "";
            case SpellRarity.Uncommon: return "[UNCOMMON] ";
            case SpellRarity.Rare: return "[RARE] ";
            case SpellRarity.Legendary: return "[LEGENDARY] ";
            default: return "";
        }
    }
} 