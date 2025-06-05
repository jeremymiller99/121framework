using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellUI : MonoBehaviour
{
    public GameObject icon;
    public RectTransform cooldown;
    public TextMeshProUGUI manacost;
    public TextMeshProUGUI damage;
    public GameObject highlight;
    public Spell spell;
    float last_text_update;
    const float UPDATE_DELAY = 1;
    public GameObject dropbutton;

    private int slotIndex = -1; // This will be set by SpellUIContainer
    private SpellUIContainer container;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        last_text_update = 0;
        
        // Set up drop button functionality
        if (dropbutton != null)
        {
            var button = dropbutton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(DropSpell);
                Debug.Log($"Drop button configured for spell slot");
            }
        }
        
        // Set up click-to-select functionality on the main spell slot
        var slotButton = gameObject.GetComponent<Button>();
        if (slotButton == null)
        {
            // Add a Button component if it doesn't exist
            slotButton = gameObject.AddComponent<Button>();
        }
        slotButton.onClick.AddListener(SelectThisSlot);
        
        // Make sure the button is interactable and doesn't interfere with visuals
        slotButton.transition = Selectable.Transition.None; // No visual transition
        Debug.Log($"Click-to-select configured for spell slot");
    }

    public void Initialize(int index, SpellUIContainer parentContainer)
    {
        slotIndex = index;
        container = parentContainer;
    }

    public void SetSpell(Spell spell)
    {
        this.spell = spell;
        
        // Restore icon properties and set spell icon
        if (icon != null)
        {
            var image = icon.GetComponent<Image>();
            if (image != null)
            {
                // Apply rarity color tint to the icon border/background
                var rarity = spell.GetRarity();
                var rarityColor = rarity.GetColor();
                
                // Blend the rarity color with white for a subtle tint
                image.color = Color.Lerp(Color.white, rarityColor, 0.4f);
                GameManager.Instance.spellIconManager.PlaceSprite(spell.GetIcon(), image);
            }
        }
        
        // Update text immediately when spell is set with rarity-colored damage text
        if (manacost != null)
            manacost.text = spell.GetManaCost().ToString();
        if (damage != null)
        {
            damage.text = spell.GetDamage().ToString();
            // Tint damage text slightly with rarity color
            var rarity = spell.GetRarity();
            damage.color = Color.Lerp(Color.white, rarity.GetColor(), 0.3f);
        }
            
        // Show drop button when spell is present
        if (dropbutton != null)
            dropbutton.SetActive(true);
    }

    public void SetEmpty(int slotNumber)
    {
        this.spell = null;
        
        // Set placeholder icon (could be a generic "empty" icon)
        if (icon != null)
        {
            var image = icon.GetComponent<Image>();
            if (image != null)
            {
                // Set to a transparent or default icon
                image.color = new Color(1f, 1f, 1f, 0.3f); // Semi-transparent
                // You could also set a specific empty slot icon here
                GameManager.Instance.spellIconManager.PlaceSprite(0, image); // Use default icon
            }
        }
        
        // Set placeholder text showing slot number
        if (manacost != null)
            manacost.text = slotNumber.ToString();
        if (damage != null)
        {
            damage.text = "---";
            damage.color = Color.white; // Reset to default color
        }
            
        // Reset cooldown
        if (cooldown != null)
            cooldown.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
            
        // Hide drop button when slot is empty
        if (dropbutton != null)
            dropbutton.SetActive(false);
    }

    // Called when drop button is clicked
    public void DropSpell()
    {
        if (spell != null && container != null && slotIndex >= 0)
        {
            Debug.Log($"Dropping spell: {spell.GetName()} from slot {slotIndex + 1}");
            container.DropSpellFromSlot(slotIndex);
        }
    }

    // Called when this spell slot is clicked to select it
    public void SelectThisSlot()
    {
        if (container != null && slotIndex >= 0)
        {
            container.SelectSlot(slotIndex);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (spell == null) return;
        
        if (Time.time > last_text_update + UPDATE_DELAY)
        {
            manacost.text = spell.GetManaCost().ToString();
            damage.text = spell.GetDamage().ToString();
            last_text_update = Time.time;
        }
        
        float since_last = Time.time - spell.last_cast;
        float perc;
        if (since_last > spell.GetCooldown())
        {
            perc = 0;
        }
        else
        {
            perc = 1-since_last / spell.GetCooldown();
        }
        cooldown.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 48 * perc);
    }
}
