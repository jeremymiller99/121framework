using UnityEngine;
using System.Collections.Generic;

// Effect that gains mana
public class GainManaEffect : IRelicEffect
{
    private int manaAmount;
    public bool IsTemporary => false;

    public GainManaEffect(int amount)
    {
        this.manaAmount = amount;
    }

    public void Execute()
    {
        OnTriggerCondition();
    }

    public void OnTriggerCondition()
    {
        var player = GameManager.Instance?.player?.GetComponent<PlayerController>();
        if (player?.spellcaster != null)
        {
            player.spellcaster.mana = Mathf.Min(player.spellcaster.mana + manaAmount, player.spellcaster.max_mana);
            Debug.Log($"Relic effect: Gained {manaAmount} mana. Current: {player.spellcaster.mana}/{player.spellcaster.max_mana}");
        }
    }

    public void EndEffect()
    {
        // Not applicable for instant effects
    }
}

// Effect that gains spell power (can be temporary or permanent)
public class GainSpellPowerEffect : IRelicEffect
{
    private string amountExpression;
    private string untilCondition;
    private int appliedAmount;
    private bool isActive;
    
    public bool IsTemporary => !string.IsNullOrEmpty(untilCondition);

    public GainSpellPowerEffect(string amount, string until = null)
    {
        this.amountExpression = amount;
        this.untilCondition = until;
        this.appliedAmount = 0;
        this.isActive = false;
    }

    public void Execute()
    {
        OnTriggerCondition();
    }

    public void OnTriggerCondition()
    {
        var player = GameManager.Instance?.player?.GetComponent<PlayerController>();
        if (player?.spellcaster != null)
        {
            // Calculate the amount using RPN evaluator
            var variables = new Dictionary<string, int>
            {
                ["wave"] = player.currentWave
            };
            
            int amount = RPNEvaluator.Evaluate(amountExpression, variables);
            
            if (IsTemporary)
            {
                // For temporary effects, apply the boost
                if (!isActive)
                {
                    appliedAmount = amount;
                    player.spellcaster.spellPower += appliedAmount;
                    isActive = true;
                    
                    // Update all spells with new spell power
                    player.spellcaster.UpdateSpellPowerAndWave(player.spellcaster.spellPower, player.currentWave);
                    
                    Debug.Log($"Relic effect: Gained {appliedAmount} temporary spell power. Current: {player.spellcaster.spellPower}");
                    
                    // Subscribe to the end condition
                    if (untilCondition == "move")
                    {
                        EventBus.Instance.OnPlayerMove += OnEndConditionMet;
                    }
                    else if (untilCondition == "cast-spell")
                    {
                        EventBus.Instance.OnSpellCast += OnSpellCastEndCondition;
                    }
                }
            }
            else
            {
                // Permanent effect
                player.spellcaster.spellPower += amount;
                player.spellcaster.UpdateSpellPowerAndWave(player.spellcaster.spellPower, player.currentWave);
                Debug.Log($"Relic effect: Gained {amount} permanent spell power. Current: {player.spellcaster.spellPower}");
            }
        }
    }

    public void EndEffect()
    {
        if (IsTemporary && isActive)
        {
            var player = GameManager.Instance?.player?.GetComponent<PlayerController>();
            if (player?.spellcaster != null)
            {
                player.spellcaster.spellPower -= appliedAmount;
                player.spellcaster.UpdateSpellPowerAndWave(player.spellcaster.spellPower, player.currentWave);
                Debug.Log($"Relic effect: Lost {appliedAmount} temporary spell power. Current: {player.spellcaster.spellPower}");
            }
            
            isActive = false;
            appliedAmount = 0;
            
            // Unsubscribe from events
            EventBus.Instance.OnPlayerMove -= OnEndConditionMet;
            EventBus.Instance.OnSpellCast -= OnSpellCastEndCondition;
        }
    }

    private void OnEndConditionMet(Vector3 position)
    {
        EndEffect();
    }

    private void OnSpellCastEndCondition(Spell spell, Vector3 from, Vector3 to)
    {
        EndEffect();
    }
}

// Effect that increases max health
public class GainMaxHealthEffect : IRelicEffect
{
    private int healthAmount;
    public bool IsTemporary => false;

    public GainMaxHealthEffect(int amount)
    {
        this.healthAmount = amount;
    }

    public void Execute()
    {
        OnTriggerCondition();
    }

    public void OnTriggerCondition()
    {
        var player = GameManager.Instance?.player?.GetComponent<PlayerController>();
        if (player?.hp != null)
        {
            player.hp.SetMaxHP(player.hp.max_hp + healthAmount);
            Debug.Log($"Relic effect: Gained {healthAmount} max health. Current: {player.hp.hp}/{player.hp.max_hp}");
        }
    }

    public void EndEffect()
    {
        // Not applicable for permanent effects
    }
}

// Effect that heals the player
public class HealEffect : IRelicEffect
{
    private int healAmount;
    public bool IsTemporary => false;

    public HealEffect(int amount)
    {
        this.healAmount = amount;
    }

    public void Execute()
    {
        OnTriggerCondition();
    }

    public void OnTriggerCondition()
    {
        var player = GameManager.Instance?.player?.GetComponent<PlayerController>();
        if (player?.hp != null)
        {
            player.hp.hp = Mathf.Min(player.hp.hp + healAmount, player.hp.max_hp);
            Debug.Log($"Relic effect: Healed {healAmount} health. Current: {player.hp.hp}/{player.hp.max_hp}");
        }
    }

    public void EndEffect()
    {
        // Not applicable for instant effects
    }
}

// Static class to handle next spell modifiers from relics
public static class NextSpellModifierManager
{
    private static int nextSpellPowerBonus = 0;
    private static bool hasNextSpellModifier = false;

    public static void AddNextSpellPowerBonus(int bonus)
    {
        nextSpellPowerBonus += bonus;
        hasNextSpellModifier = true;
        Debug.Log($"Added {bonus} spell power bonus for next spell. Total bonus: {nextSpellPowerBonus}");
    }

    public static void ApplyAndClearModifiers(Spell spell)
    {
        if (hasNextSpellModifier && spell != null)
        {
            spell.spellPower += nextSpellPowerBonus;
            Debug.Log($"Applied {nextSpellPowerBonus} spell power bonus to {spell.GetName()}. New spell power: {spell.spellPower}");
            
            // Clear the modifiers after use
            nextSpellPowerBonus = 0;
            hasNextSpellModifier = false;
        }
    }

    public static bool HasModifiers()
    {
        return hasNextSpellModifier;
    }

    public static void ClearModifiers()
    {
        nextSpellPowerBonus = 0;
        hasNextSpellModifier = false;
    }
}

// Effect that modifies the next spell (one-time)
public class NextSpellModifierEffect : IRelicEffect
{
    private int spellPowerBonus;
    private bool isActive;
    
    public bool IsTemporary => true;

    public NextSpellModifierEffect(int spellPowerBonus)
    {
        this.spellPowerBonus = spellPowerBonus;
        this.isActive = false;
    }

    public void Execute()
    {
        OnTriggerCondition();
    }

    public void OnTriggerCondition()
    {
        if (!isActive)
        {
            isActive = true;
            
            // Add the spell power bonus to the next spell modifier manager
            NextSpellModifierManager.AddNextSpellPowerBonus(spellPowerBonus);
            
            // Subscribe to spell cast event to track when the effect is consumed
            EventBus.Instance.OnSpellCast += OnSpellCast;
            
            Debug.Log($"Relic effect: Next spell will gain {spellPowerBonus} spell power");
        }
    }

    public void EndEffect()
    {
        if (isActive)
        {
            isActive = false;
            EventBus.Instance.OnSpellCast -= OnSpellCast;
        }
    }

    private void OnSpellCast(Spell spell, Vector3 from, Vector3 to)
    {
        // End the effect after one use
        Debug.Log($"Relic effect: Next spell modifier consumed by {spell?.GetName()}");
        EndEffect();
    }
} 