using UnityEngine;
using System;

public class EventBus 
{
    private static EventBus theInstance;
    public static EventBus Instance
    {
        get
        {
            if (theInstance == null)
                theInstance = new EventBus();
            return theInstance;
        }
    }

    public event Action<Vector3, Damage, Hittable> OnDamage;
    
    public event Action<Hittable> OnPlayerDamageTaken;
    public event Action<Hittable, Hittable> OnEnemyKilled;
    public event Action<Spell, Vector3, Vector3> OnSpellCast;
    public event Action<Vector3> OnPlayerMove;
    public event Action<float> OnPlayerStandStill;
    public event Action<int> OnWaveComplete;
    public event Action<int> OnWaveStart;

    public void DoDamage(Vector3 where, Damage dmg, Hittable target)
    {
        OnDamage?.Invoke(where, dmg, target);
        
        if (target.team == Hittable.Team.PLAYER)
        {
            OnPlayerDamageTaken?.Invoke(target);
        }
    }

    public void FireEnemyKilled(Hittable killer, Hittable killed)
    {
        OnEnemyKilled?.Invoke(killer, killed);
    }

    public void FireSpellCast(Spell spell, Vector3 from, Vector3 to)
    {
        OnSpellCast?.Invoke(spell, from, to);
    }

    public void FirePlayerMove(Vector3 position)
    {
        OnPlayerMove?.Invoke(position);
    }

    public void FirePlayerStandStill(float duration)
    {
        OnPlayerStandStill?.Invoke(duration);
    }

    public void FireWaveComplete(int waveNumber)
    {
        OnWaveComplete?.Invoke(waveNumber);
    }

    public void FireWaveStart(int waveNumber)
    {
        OnWaveStart?.Invoke(waveNumber);
    }
}
