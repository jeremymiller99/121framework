using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellCaster 
{
    public int mana;
    public int max_mana;
    public int mana_reg;
    public int spellPower;
    public Hittable.Team team;
    public List<Spell> spells;
    public int selectedSpellIndex = 0;
    public const int MAX_SPELLS = 4;

    public IEnumerator ManaRegeneration()
    {
        while (true)
        {
            mana += mana_reg;
            mana = Mathf.Min(mana, max_mana);
            yield return new WaitForSeconds(1);
        }
    }

    public SpellCaster(int mana, int mana_reg, Hittable.Team team)
    {
        this.mana = mana;
        this.max_mana = mana;
        this.mana_reg = mana_reg;
        this.team = team;
        this.spellPower = 0;
        
        spells = new List<Spell>();
        
        // Start with a basic arcane bolt
        var spellBuilder = new SpellBuilder();
        var startingSpell = spellBuilder.Build(this, "arcane_bolt");
        startingSpell.spellPower = spellPower;
        startingSpell.currentWave = 1;
        spells.Add(startingSpell);
    }

    public Spell GetCurrentSpell()
    {
        if (spells.Count == 0) return null;
        if (selectedSpellIndex >= spells.Count) selectedSpellIndex = 0;
        return spells[selectedSpellIndex];
    }

    public void SelectSpell(int index)
    {
        // Allow selection of any slot 0-3, even if empty
        if (index >= 0 && index < MAX_SPELLS)
        {
            selectedSpellIndex = index;
            
            if (index < spells.Count)
            {
                Debug.Log($"Selected: {spells[index].GetName()} (slot {index + 1})");
            }
            else
            {
                Debug.Log($"Selected empty slot {index + 1}");
            }
        }
        else
        {
            Debug.LogWarning($"Invalid spell slot index: {index}. Must be 0-{MAX_SPELLS - 1}");
        }
    }

    public void AddSpell(Spell newSpell)
    {
        if (spells.Count < MAX_SPELLS)
        {
            spells.Add(newSpell);
        }
        else
        {
            // Replace the currently selected spell
            spells[selectedSpellIndex] = newSpell;
        }
    }

    public void RemoveSpell(int index)
    {
        if (index >= 0 && index < spells.Count && spells.Count > 1)
        {
            spells.RemoveAt(index);
            if (selectedSpellIndex >= spells.Count)
            {
                selectedSpellIndex = spells.Count - 1;
            }
        }
    }

    public void UpdateSpellPowerAndWave(int newSpellPower, int currentWave)
    {
        this.spellPower = newSpellPower;
        foreach (var spell in spells)
        {
            spell.spellPower = newSpellPower;
            spell.currentWave = currentWave;
        }
    }

    public IEnumerator Cast(Vector3 where, Vector3 target)
    {
        var currentSpell = GetCurrentSpell();
        if (currentSpell != null && mana >= currentSpell.GetManaCost() && currentSpell.IsReady())
        {
            mana -= currentSpell.GetManaCost();
            yield return currentSpell.Cast(where, target, team);
        }
        yield break;
    }
}
