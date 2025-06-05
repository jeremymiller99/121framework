using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public abstract class Spell 
{
    public float last_cast;
    public SpellCaster owner;
    public Hittable.Team team;
    public int spellPower;
    public int currentWave;

    // Base spell attributes that can be modified
    protected string name;
    protected string description;
    protected int icon;
    protected int baseDamage;
    protected Damage.Type damageType;
    protected int baseManaCost;
    protected float baseCooldown;
    protected float baseSpeed;
    protected string baseTrajectory;
    protected int baseSprite;
    
    // Rarity and modifier tracking
    protected List<string> modifiers;
    protected SpellRarity rarity;

    public Spell(SpellCaster owner)
    {
        this.owner = owner;
        this.spellPower = 0;
        this.currentWave = 1;
        
        // Set default values
        name = "Spell";
        description = "A basic spell";
        icon = 0;
        baseDamage = 10;
        damageType = Damage.Type.ARCANE;
        baseManaCost = 10;
        baseCooldown = 1f;
        baseSpeed = 10f;
        baseTrajectory = "straight";
        baseSprite = 0;
        
        // Initialize modifier tracking
        modifiers = new List<string>();
        rarity = SpellRarity.Common;
    }

    public virtual string GetName()
    {
        return name;
    }

    public virtual string GetDescription()
    {
        return description;
    }

    public virtual int GetIcon()
    {
        return icon;
    }
    
    public virtual List<string> GetModifiers()
    {
        return new List<string>(modifiers);
    }
    
    public virtual SpellRarity GetRarity()
    {
        return rarity;
    }
    
    public virtual void SetRarity(SpellRarity newRarity)
    {
        rarity = newRarity;
    }
    
    public virtual void AddModifier(string modifierName)
    {
        modifiers.Add(modifierName);
        UpdateRarityFromModifiers();
    }
    
    protected virtual void UpdateRarityFromModifiers()
    {
        int modifierCount = modifiers.Count;
        if (modifierCount == 0)
            rarity = SpellRarity.Common;
        else if (modifierCount == 1)
            rarity = SpellRarity.Uncommon;
        else if (modifierCount == 2)
            rarity = SpellRarity.Rare;
        else if (modifierCount >= 3)
            rarity = SpellRarity.Legendary;
    }

    // Methods that apply modifiers - to be overridden by modifier spells
    public virtual int GetDamage()
    {
        // baseDamage should already be calculated from JSON expressions in SetAttributes
        return baseDamage;
    }

    public virtual int GetManaCost()
    {
        // baseManaCost should already be calculated from JSON expressions in SetAttributes
        return baseManaCost;
    }

    public virtual float GetCooldown()
    {
        return baseCooldown;
    }

    public virtual float GetSpeed()
    {
        return baseSpeed;
    }

    public virtual string GetTrajectory()
    {
        return baseTrajectory;
    }

    public virtual int GetSprite()
    {
        return baseSprite;
    }

    public bool IsReady()
    {
        return (last_cast + GetCooldown() < Time.time);
    }

    // Method to set attributes from JSON - can be overridden by specific spells
    public virtual void SetAttributes(JObject attributes)
    {
        if (attributes["name"] != null)
            name = attributes["name"].ToString();
        
        if (attributes["description"] != null)
            description = attributes["description"].ToString();
        
        if (attributes["icon"] != null)
            icon = (int)attributes["icon"];

        // Parse damage
        if (attributes["damage"] != null)
        {
            var damageObj = attributes["damage"];
            if (damageObj["amount"] != null)
            {
                string damageExpr = damageObj["amount"].ToString();
                baseDamage = EvaluateExpression(damageExpr);
            }
            if (damageObj["type"] != null)
            {
                string typeStr = damageObj["type"].ToString();
                damageType = ParseDamageType(typeStr);
            }
        }

        if (attributes["mana_cost"] != null)
        {
            string manaCostExpr = attributes["mana_cost"].ToString();
            baseManaCost = EvaluateExpression(manaCostExpr);
        }

        if (attributes["cooldown"] != null)
        {
            string cooldownExpr = attributes["cooldown"].ToString();
            baseCooldown = EvaluateExpressionFloat(cooldownExpr);
        }

        // Parse projectile
        if (attributes["projectile"] != null)
        {
            var projectileObj = attributes["projectile"];
            if (projectileObj["speed"] != null)
            {
                string speedExpr = projectileObj["speed"].ToString();
                baseSpeed = EvaluateExpressionFloat(speedExpr);
            }
            if (projectileObj["trajectory"] != null)
                baseTrajectory = projectileObj["trajectory"].ToString();
            if (projectileObj["sprite"] != null)
                baseSprite = (int)projectileObj["sprite"];
        }
    }

    protected int EvaluateExpression(string expression)
    {
        var variables = new Dictionary<string, int>
        {
            ["power"] = spellPower,
            ["wave"] = currentWave
        };
        
        return RPNEvaluator.Evaluate(expression, variables);
    }

    protected float EvaluateExpressionFloat(string expression)
    {
        var variables = new Dictionary<string, float>
        {
            ["power"] = spellPower,
            ["wave"] = currentWave
        };
        
        return RPNEvaluator.EvaluateFloat(expression, variables);
    }

    protected Damage.Type ParseDamageType(string typeStr)
    {
        switch (typeStr.ToLower())
        {
            case "arcane": return Damage.Type.ARCANE;
            case "fire": return Damage.Type.FIRE;
            case "physical": return Damage.Type.PHYSICAL;
            default: return Damage.Type.ARCANE;
        }
    }

    // Abstract method for casting - each spell implements its own behavior
    public abstract IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team, SpellModifiers modifiers = null);

    // Helper method for creating standard projectile OnHit effects
    protected System.Action<Hittable, Vector3> CreateStandardOnHit(int damage, Damage.Type damageType, Hittable.Team team)
    {
        return (Hittable other, Vector3 impact) =>
        {
            if (other.team != team)
            {
                other.Damage(new Damage(damage, damageType));
            }
        };
    }
}
