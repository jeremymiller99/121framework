using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RelicUI : MonoBehaviour
{
    // Your original fields
    public PlayerController player;
    public int index;
    public Image icon;
    public GameObject highlight;
    public TextMeshProUGUI label;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Update display with relic from new system
        UpdateDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (RelicManager.Instance == null) return;
        
        var playerRelics = RelicManager.Instance.GetPlayerRelics();
        
        if (index < playerRelics.Count)
        {
            var relic = playerRelics[index];
            
            // Set relic icon
            if (icon != null && GameManager.Instance.relicIconManager != null)
            {
                GameManager.Instance.relicIconManager.PlaceSprite(relic.sprite, icon);
            }
            else if (icon != null)
            {
                // Use color coding if no sprite manager
                icon.color = GetRelicColor(relic.sprite);
            }
            
            // Set label
            if (label != null)
            {
                label.text = relic.name;
            }
            
            // Show highlight if relic is active (for temporary effects)
            if (highlight != null)
            {
                highlight.SetActive(relic.effect != null && relic.effect.IsTemporary);
            }
            
            // Show this UI element
            gameObject.SetActive(true);
        }
        else
        {
            // Hide this UI element if no relic at this index
            gameObject.SetActive(false);
        }
    }

    Color GetRelicColor(int spriteIndex)
    {
        Color[] colors = {
            Color.green,    // 0 - Green Gem
            Color.yellow,   // 1 - Jade Elephant  
            Color.cyan,     // 2 - Golden Mask
            Color.magenta,  // 3 - Cursed Scroll
            Color.blue,     // 4 - Mystic Orb
            Color.red,      // 5 - Vitality Crystal
            new Color(1f, 0.5f, 0f), // 6 - Phoenix Feather (orange)
            new Color(0.5f, 0f, 1f)  // 7 - Soul Harvester (purple)
        };
        
        return colors[spriteIndex % colors.Length];
    }
}
