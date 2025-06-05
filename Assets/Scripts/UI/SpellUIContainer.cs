using UnityEngine;
using System.Collections.Generic;

public class SpellUIContainer : MonoBehaviour
{
    public List<SpellUI> spellUIs;
    private SpellCaster spellCaster;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Don't initialize slots here - wait for SetSpellCaster to be called
        // This ensures we integrate properly with the existing game setup
        Debug.Log($"SpellUIContainer started with {spellUIs?.Count ?? 0} spell UI slots");
        
        // Initialize each spell UI with its index and reference to this container
        for (int i = 0; i < spellUIs.Count; i++)
        {
            if (spellUIs[i] != null)
            {
                spellUIs[i].Initialize(i, this);
            }
        }
    }

    public void SetSpellCaster(SpellCaster caster)
    {
        spellCaster = caster;
        Debug.Log($"SpellUIContainer: SpellCaster set with {caster?.spells?.Count ?? 0} spells");
        UpdateSpellDisplay();
    }

    // Called by SpellUI when drop button is clicked
    public void DropSpellFromSlot(int slotIndex)
    {
        if (spellCaster == null || slotIndex < 0 || slotIndex >= spellCaster.spells.Count)
        {
            Debug.LogWarning($"Cannot drop spell from slot {slotIndex + 1}");
            return;
        }

        string spellName = spellCaster.spells[slotIndex].GetName();
        spellCaster.RemoveSpell(slotIndex);
        Debug.Log($"Dropped spell: {spellName} from slot {slotIndex + 1}");
        
        // Update display immediately
        UpdateSpellDisplay();
    }

    // Called by SpellUI when slot is clicked to select it
    public void SelectSlot(int slotIndex)
    {
        if (spellCaster == null)
            return;
            
        spellCaster.SelectSpell(slotIndex);
        UpdateSpellDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        if (spellCaster != null)
        {
            UpdateSpellDisplay();
        }
    }

    void UpdateSpellDisplay()
    {
        if (spellCaster == null || spellUIs == null) return;

        // Update each spell UI slot (always show all 4 slots)
        for (int i = 0; i < spellUIs.Count; i++)
        {
            if (spellUIs[i] == null) continue;

            // Always keep the slot active
            spellUIs[i].gameObject.SetActive(true);

            // Handle highlighting for any slot (filled or empty)
            bool isSelected = (i == spellCaster.selectedSpellIndex);
            if (spellUIs[i].highlight != null)
            {
                spellUIs[i].highlight.SetActive(isSelected);
            }

            if (i < spellCaster.spells.Count)
            {
                // Show spell in this slot
                spellUIs[i].SetSpell(spellCaster.spells[i]);
            }
            else
            {
                // Show empty slot
                spellUIs[i].SetEmpty(i + 1); // Slot numbers 1-4
            }
        }
    }
}
