using UnityEngine;
using System;

// Trigger for when player takes damage
public class TakeDamageTrigger : IRelicTrigger
{
    private IRelicEffect effect;

    public TakeDamageTrigger(IRelicEffect effect)
    {
        this.effect = effect;
    }

    public void Initialize()
    {
        EventBus.Instance.OnPlayerDamageTaken += OnPlayerDamaged;
    }

    public void Cleanup()
    {
        EventBus.Instance.OnPlayerDamageTaken -= OnPlayerDamaged;
    }

    private void OnPlayerDamaged(Hittable player)
    {
        effect?.OnTriggerCondition();
    }
}

// Trigger for when player kills an enemy
public class OnKillTrigger : IRelicTrigger
{
    private IRelicEffect effect;

    public OnKillTrigger(IRelicEffect effect)
    {
        this.effect = effect;
    }

    public void Initialize()
    {
        EventBus.Instance.OnEnemyKilled += OnEnemyKilled;
    }

    public void Cleanup()
    {
        EventBus.Instance.OnEnemyKilled -= OnEnemyKilled;
    }

    private void OnEnemyKilled(Hittable killer, Hittable killed)
    {
        // Only trigger if player was the killer
        if (killer != null && killer.team == Hittable.Team.PLAYER)
        {
            effect?.OnTriggerCondition();
        }
    }
}

// Trigger for standing still for a certain duration
public class StandStillTrigger : IRelicTrigger
{
    private IRelicEffect effect;
    private float requiredDuration;
    private float currentStillTime;
    private Vector3 lastPosition;
    private bool isTracking;

    public StandStillTrigger(IRelicEffect effect, float duration)
    {
        this.effect = effect;
        this.requiredDuration = duration;
        this.currentStillTime = 0f;
        this.isTracking = false;
    }

    public void Initialize()
    {
        EventBus.Instance.OnPlayerMove += OnPlayerMove;
        EventBus.Instance.OnPlayerStandStill += OnPlayerStandStill;
        
        // Start tracking using coroutine manager
        if (CoroutineManager.Instance != null)
        {
            CoroutineManager.Instance.StartCoroutine(TrackStandStill());
        }
        else if (GameManager.Instance?.player != null)
        {
            // Fallback: try to get CoroutineManager from player
            var coroutineManager = GameManager.Instance.player.GetComponent<CoroutineManager>();
            if (coroutineManager != null)
            {
                coroutineManager.StartCoroutine(TrackStandStill());
            }
            else
            {
                Debug.LogWarning("CoroutineManager not found! StandStillTrigger may not work properly.");
            }
        }
    }

    public void Cleanup()
    {
        EventBus.Instance.OnPlayerMove -= OnPlayerMove;
        EventBus.Instance.OnPlayerStandStill -= OnPlayerStandStill;
        isTracking = false;
    }

    private void OnPlayerMove(Vector3 position)
    {
        // Reset the timer when player moves
        currentStillTime = 0f;
        lastPosition = position;
        
        // End temporary effects when player moves
        if (effect != null && effect.IsTemporary)
        {
            effect.EndEffect();
        }
    }

    private void OnPlayerStandStill(float duration)
    {
        if (duration >= requiredDuration)
        {
            effect?.OnTriggerCondition();
        }
    }

    private System.Collections.IEnumerator TrackStandStill()
    {
        isTracking = true;
        lastPosition = GameManager.Instance.player.transform.position;
        currentStillTime = 0f;

        while (isTracking && GameManager.Instance?.player != null)
        {
            Vector3 currentPosition = GameManager.Instance.player.transform.position;
            
            // Check if player has moved (with small tolerance for floating point precision)
            if (Vector3.Distance(currentPosition, lastPosition) < 0.1f)
            {
                currentStillTime += 0.1f;
                
                // Fire stand still event if we've reached the required duration
                if (currentStillTime >= requiredDuration)
                {
                    EventBus.Instance.FirePlayerStandStill(currentStillTime);
                }
            }
            else
            {
                // Player moved
                EventBus.Instance.FirePlayerMove(currentPosition);
                currentStillTime = 0f;
                lastPosition = currentPosition;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
}

// Trigger for spell casting
public class SpellCastTrigger : IRelicTrigger
{
    private IRelicEffect effect;

    public SpellCastTrigger(IRelicEffect effect)
    {
        this.effect = effect;
    }

    public void Initialize()
    {
        EventBus.Instance.OnSpellCast += OnSpellCast;
    }

    public void Cleanup()
    {
        EventBus.Instance.OnSpellCast -= OnSpellCast;
    }

    private void OnSpellCast(Spell spell, Vector3 from, Vector3 to)
    {
        effect?.OnTriggerCondition();
    }
}

// Trigger for wave completion
public class WaveCompleteTrigger : IRelicTrigger
{
    private IRelicEffect effect;

    public WaveCompleteTrigger(IRelicEffect effect)
    {
        this.effect = effect;
    }

    public void Initialize()
    {
        EventBus.Instance.OnWaveComplete += OnWaveComplete;
    }

    public void Cleanup()
    {
        EventBus.Instance.OnWaveComplete -= OnWaveComplete;
    }

    private void OnWaveComplete(int waveNumber)
    {
        effect?.OnTriggerCondition();
    }
} 