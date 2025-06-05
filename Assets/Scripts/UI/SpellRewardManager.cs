using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;

public class SpellRewardManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject spellRewardPanel;
    public TextMeshProUGUI spellRewardText;
    public Button acceptSpellButton;
    public Button declineSpellButton;
    
    private SpellBuilder spellBuilder;
    private Spell currentRewardSpell;
    private bool waitingForChoice = false;
    private bool hasOfferedRewardThisWave = false;

    void Start()
    {
        spellBuilder = new SpellBuilder();
        
        // Setup button listeners with debug logging
        if (acceptSpellButton != null)
        {
            acceptSpellButton.onClick.AddListener(AcceptSpellReward);
            Debug.Log("Accept spell button listener added successfully");
        }
        else
        {
            Debug.LogError("Accept spell button is null! Check inspector references.");
        }
        
        if (declineSpellButton != null)
        {
            declineSpellButton.onClick.AddListener(DeclineSpellReward);
            Debug.Log("Decline spell button listener added successfully");
        }
        else
        {
            Debug.LogError("Decline spell button is null! Check inspector references.");
        }
            
        // Hide panel initially
        if (spellRewardPanel != null)
            spellRewardPanel.SetActive(false);
        else
            Debug.LogError("Spell reward panel is null! Check inspector references.");
    }

    void Update()
    {
        // Reset flag when new wave starts
        if (GameManager.Instance.state == GameManager.GameState.INWAVE)
        {
            hasOfferedRewardThisWave = false;
        }
    }

    public void ShowSpellReward()
    {
        Debug.Log("ShowSpellReward called");
        
        // Don't show multiple rewards for the same wave
        if (hasOfferedRewardThisWave)
        {
            Debug.Log("Already offered reward this wave, skipping");
            return;
        }
        
        var playerController = GameManager.Instance.player?.GetComponent<PlayerController>();
        if (playerController == null || playerController.spellcaster == null)
        {
            Debug.LogError("Could not find player or spellcaster for spell reward");
            return;
        }

        // Generate random spell with current spell power and wave
        currentRewardSpell = spellBuilder.BuildRandomSpell(
            playerController.spellcaster, 
            playerController.spellcaster.spellPower, 
            playerController.currentWave
        );

        Debug.Log($"Generated reward spell: {currentRewardSpell?.GetName()}");
        hasOfferedRewardThisWave = true;
        ShowSpellRewardUI();
    }

    void ShowSpellRewardUI()
    {
        if (spellRewardPanel == null || currentRewardSpell == null) return;

        // Hide any other UI that might be showing before showing spell reward
        var enemySpawner = FindObjectOfType<EnemySpawner>();
        if (enemySpawner != null)
        {
            // Hide continue button and level info text
            if (enemySpawner.continueButton != null)
                enemySpawner.continueButton.SetActive(false);
            if (enemySpawner.levelInfoText != null)
                enemySpawner.levelInfoText.gameObject.SetActive(false);
        }

        waitingForChoice = true;
        spellRewardPanel.SetActive(true);
        Debug.Log("Spell reward panel activated");

        if (spellRewardText != null)
        {
            // Get spell information
            var rarity = currentRewardSpell.GetRarity();
            var modifiers = currentRewardSpell.GetModifiers();
            var rarityColor = rarity.GetColor();
            
            // Build the simplified reward text
            string rewardText = "";
            
            // Get the base spell name without modifiers
            string baseSpellName = GetBaseSpellName(currentRewardSpell);
            
            // Header with spell name and rarity
            rewardText += $"<size=24><b><color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}>";
            rewardText += $"{rarity.GetPrefix()}{baseSpellName}";
            rewardText += "</color></b></size>\n\n";
            
            // Modifiers section (if any)
            if (modifiers.Count > 0)
            {
                var modifierGroups = modifiers.GroupBy(m => m).ToDictionary(g => g.Key, g => g.Count());
                
                foreach (var modifierGroup in modifierGroups)
                {
                    string modifierName = modifierGroup.Key;
                    int count = modifierGroup.Value;
                    var modifierInfo = SpellModifierInfo.GetInfo(modifierName);
                    
                    if (count > 1)
                    {
                        rewardText += $"<color=#FFD700><b>{SpellModifierInfo.GetStackedName(modifierName, count)}</b></color>\n";
                    }
                    else
                    {
                        rewardText += $"<color=#87CEEB>{modifierInfo.displayName}</color>\n";
                    }
                }
                rewardText += "\n";
            }
            
            // Stats
            rewardText += $"Damage: <b>{currentRewardSpell.GetDamage()}</b> | ";
            rewardText += $"Mana: <b>{currentRewardSpell.GetManaCost()}</b> | ";
            rewardText += $"Cooldown: <b>{currentRewardSpell.GetCooldown():F1}s</b>\n\n";
            
            // Simple prompt
            rewardText += "Take this spell?";
            
            spellRewardText.text = rewardText;
            Debug.Log("Enhanced spell reward text updated");
            
            // Change button colors based on rarity for extra juice
            if (acceptSpellButton != null)
            {
                var buttonColors = acceptSpellButton.colors;
                buttonColors.normalColor = Color.Lerp(Color.white, rarityColor, 0.3f);
                buttonColors.highlightedColor = Color.Lerp(Color.white, rarityColor, 0.5f);
                acceptSpellButton.colors = buttonColors;
            }
        }
        
        // Force UI refresh to ensure proper display
        Canvas.ForceUpdateCanvases();
        
        // Log button states for debugging
        if (acceptSpellButton != null)
        {
            Debug.Log($"Accept button - Active: {acceptSpellButton.gameObject.activeInHierarchy}, Interactable: {acceptSpellButton.interactable}");
        }
        if (declineSpellButton != null)
        {
            Debug.Log($"Decline button - Active: {declineSpellButton.gameObject.activeInHierarchy}, Interactable: {declineSpellButton.interactable}");
        }
    }

    void AcceptSpellReward()
    {
        Debug.Log("AcceptSpellReward called!");
        
        if (currentRewardSpell == null) 
        {
            Debug.LogError("Current reward spell is null in AcceptSpellReward");
            return;
        }

        var playerController = GameManager.Instance.player?.GetComponent<PlayerController>();
        if (playerController != null)
        {
            Debug.Log($"Adding spell {currentRewardSpell.GetName()} to player");
            playerController.ShowSpellReward(currentRewardSpell);
        }
        else
        {
            Debug.LogError("Could not find player controller in AcceptSpellReward");
        }

        HideSpellRewardUI();
    }

    void DeclineSpellReward()
    {
        Debug.Log($"DeclineSpellReward called! Player declined spell: {currentRewardSpell?.GetName()}");
        HideSpellRewardUI();
    }

    void HideSpellRewardUI()
    {
        Debug.Log("HideSpellRewardUI called");
        waitingForChoice = false;
        currentRewardSpell = null;
        
        // Reset button colors
        if (acceptSpellButton != null)
        {
            var buttonColors = acceptSpellButton.colors;
            buttonColors.normalColor = Color.white;
            buttonColors.highlightedColor = new Color(0.96f, 0.96f, 0.96f);
            acceptSpellButton.colors = buttonColors;
        }
        
        // Immediately hide the spell reward panel
        if (spellRewardPanel != null)
        {
            spellRewardPanel.SetActive(false);
            Debug.Log("Spell reward panel hidden");
        }
        
        // Force UI refresh to ensure the panel is hidden before continuing
        Canvas.ForceUpdateCanvases();
        
        // Small delay to ensure UI is properly updated before continuing
        StartCoroutine(DelayedContinue());
    }
    
    private System.Collections.IEnumerator DelayedContinue()
    {
        // Wait one frame to ensure UI changes are applied
        yield return null;
        
        // Notify enemy spawner that choice was made so it can continue
        var enemySpawner = FindObjectOfType<EnemySpawner>();
        if (enemySpawner != null)
        {
            Debug.Log("Calling NextWave to continue after spell choice");
            enemySpawner.NextWave();
        }
    }

    // Public method that can be called from EnemySpawner's NextWave method
    public bool IsWaitingForChoice()
    {
        return waitingForChoice;
    }
    
    // Public method to reset the spell reward manager state (called during game restart)
    public void ResetState()
    {
        Debug.Log("SpellRewardManager: Resetting state");
        
        waitingForChoice = false;
        currentRewardSpell = null;
        hasOfferedRewardThisWave = false;
        
        if (spellRewardPanel != null)
        {
            spellRewardPanel.SetActive(false);
        }
        
        Debug.Log("SpellRewardManager: State reset complete");
    }
    
    // Helper method to get the base spell name without modifier prefixes
    private string GetBaseSpellName(Spell spell)
    {
        // If it's a modifier spell, get the inner spell's name
        if (spell is ModifierSpell modifierSpell)
        {
            return GetBaseSpellName(modifierSpell.innerSpell);
        }
        return spell.GetName();
    }
    
    // Helper method to get the base spell description without modifier text
    private string GetBaseSpellDescription(Spell spell)
    {
        // If it's a modifier spell, get the inner spell's description
        if (spell is ModifierSpell modifierSpell)
        {
            return GetBaseSpellDescription(modifierSpell.innerSpell);
        }
        return spell.GetDescription();
    }
} 