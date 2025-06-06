using UnityEngine;
using System;

// Interface for relic triggers
public interface IRelicTrigger
{
    void Initialize();
    void Cleanup();
}

// Interface for relic effects
public interface IRelicEffect
{
    void Execute();
    void OnTriggerCondition();
    bool IsTemporary { get; }
    void EndEffect(); // For temporary effects
}

// Main Relic class
[System.Serializable]
public class Relic
{
    public string name;
    public int sprite;
    public string triggerDescription;
    public string effectDescription;
    public IRelicTrigger trigger;
    public IRelicEffect effect;

    public Relic(string name, int sprite, string triggerDesc, string effectDesc, IRelicTrigger trigger, IRelicEffect effect)
    {
        this.name = name;
        this.sprite = sprite;
        this.triggerDescription = triggerDesc;
        this.effectDescription = effectDesc;
        this.trigger = trigger;
        this.effect = effect;
        
        // Initialize the trigger
        trigger?.Initialize();
    }

    public void Destroy()
    {
        trigger?.Cleanup();
        if (effect != null && effect.IsTemporary)
        {
            effect.EndEffect();
        }
    }

    public string GetDescription()
    {
        return $"{triggerDescription}, {effectDescription}";
    }
} 