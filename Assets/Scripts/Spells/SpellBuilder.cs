using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class SpellBuilder 
{
    private JObject spellsData;
    private List<string> baseSpellNames;
    private List<string> modifierSpellNames;

    public SpellBuilder()
    {
        LoadSpellData();
    }

    private void LoadSpellData()
    {
        try
        {
            TextAsset spellsFile = Resources.Load<TextAsset>("spells");
            if (spellsFile != null)
            {
                spellsData = JObject.Parse(spellsFile.text);
                
                // Categorize spells
                baseSpellNames = new List<string>();
                modifierSpellNames = new List<string>();
                
                foreach (var spell in spellsData)
                {
                    string spellName = spell.Key;
                    var spellObj = spell.Value as JObject;
                    
                    // Check if it's a modifier spell (has modifier-specific properties)
                    if (IsModifierSpell(spellObj))
                    {
                        modifierSpellNames.Add(spellName);
                    }
                    else
                    {
                        baseSpellNames.Add(spellName);
                    }
                }
                
                Debug.Log($"Loaded {baseSpellNames.Count} base spells and {modifierSpellNames.Count} modifier spells");
            }
            else
            {
                Debug.LogError("Could not load spells.json from Resources folder");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading spell data: {e.Message}");
        }
    }

    private bool IsModifierSpell(JObject spellObj)
    {
        // Check for modifier-specific properties
        return spellObj["damage_multiplier"] != null ||
               spellObj["mana_multiplier"] != null ||
               spellObj["speed_multiplier"] != null ||
               spellObj["cooldown_multiplier"] != null ||
               spellObj["delay"] != null ||
               spellObj["angle"] != null ||
               spellObj["projectile_trajectory"] != null ||
               spellObj["mana_adder"] != null ||
               spellObj["max_pierces"] != null ||
               spellObj["max_chains"] != null;
    }

    public Spell Build(SpellCaster owner, string spellName = null)
    {
        if (spellName == null)
        {
            spellName = "arcane_bolt"; // Default spell
        }

        return CreateSpell(owner, spellName);
    }

    public Spell BuildRandomSpell(SpellCaster owner, int spellPower, int currentWave)
    {
        // Start with a random base spell
        string baseSpellName = baseSpellNames[Random.Range(0, baseSpellNames.Count)];
        Spell spell = CreateSpell(owner, baseSpellName);
        
        // Set spell power and wave
        spell.spellPower = spellPower;
        spell.currentWave = currentWave;
        
        // Randomly apply 0-3 modifiers
        int modifierCount = Random.Range(0, 4);
        for (int i = 0; i < modifierCount; i++)
        {
            string modifierName = modifierSpellNames[Random.Range(0, modifierSpellNames.Count)];
            spell = ApplyModifier(spell, modifierName);
        }
        
        return spell;
    }

    private Spell CreateSpell(SpellCaster owner, string spellName)
    {
        if (spellsData == null || !spellsData.ContainsKey(spellName))
        {
            Debug.LogError($"Spell '{spellName}' not found in spell data");
            return new ArcaneBoltSpell(owner); // Fallback
        }

        var spellData = spellsData[spellName] as JObject;
        Spell spell = null;

        // Create the appropriate spell type
        switch (spellName)
        {
            case "arcane_bolt":
                spell = new ArcaneBoltSpell(owner);
                break;
            case "magic_missile":
                spell = new MagicMissileSpell(owner);
                break;
            case "arcane_blast":
                spell = new ArcaneBlastSpell(owner);
                break;
            case "arcane_spray":
                spell = new ArcaneSpraySpell(owner);
                break;
            case "arcane_nova":
                spell = new ArcaneNovaSpell(owner);
                break;
            default:
                Debug.LogWarning($"Unknown base spell type: {spellName}, using ArcaneBolt");
                spell = new ArcaneBoltSpell(owner);
                break;
        }

        // Apply attributes from JSON
        if (spell != null)
        {
            spell.SetAttributes(spellData);
            Debug.Log($"Created spell: {spell.GetName()} - Damage: {spell.GetDamage()}, Mana: {spell.GetManaCost()}, Cooldown: {spell.GetCooldown()}");
        }

        return spell;
    }

    private Spell ApplyModifier(Spell baseSpell, string modifierName)
    {
        if (spellsData == null || !spellsData.ContainsKey(modifierName))
        {
            Debug.LogError($"Modifier '{modifierName}' not found in spell data");
            return baseSpell;
        }

        var modifierData = spellsData[modifierName] as JObject;
        ModifierSpell modifierSpell = null;

        // Create the appropriate modifier type
        switch (modifierName)
        {
            case "damage_amp":
                modifierSpell = new DamageAmpModifier(baseSpell);
                break;
            case "speed_amp":
                modifierSpell = new SpeedAmpModifier(baseSpell);
                break;
            case "doubler":
                modifierSpell = new DoublerModifier(baseSpell);
                break;
            case "splitter":
                modifierSpell = new SplitterModifier(baseSpell);
                break;
            case "chaos":
                modifierSpell = new ChaosModifier(baseSpell);
                break;
            case "homing":
                modifierSpell = new HomingModifier(baseSpell);
                break;
            case "piercing":
                modifierSpell = new PiercingModifier(baseSpell);
                break;
            case "chain_lightning":
                modifierSpell = new ChainLightningModifier(baseSpell);
                break;
            default:
                Debug.LogWarning($"Unknown modifier type: {modifierName}");
                return baseSpell;
        }

        // Apply modifier attributes from JSON
        if (modifierSpell != null)
        {
            modifierSpell.SetAttributes(modifierData);
            Debug.Log($"Applied modifier: {modifierName} to {baseSpell.GetName()} - New damage: {modifierSpell.GetDamage()}, New mana: {modifierSpell.GetManaCost()}");
        }

        return modifierSpell ?? baseSpell;
    }
}
