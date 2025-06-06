using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class RelicSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject relicSelectionPanel;
    public Button[] relicChoiceButtons;
    public Image[] relicChoiceIcons;
    public TextMeshProUGUI[] relicChoiceNames;
    public TextMeshProUGUI[] relicChoiceDescriptions;
    public TextMeshProUGUI selectionTitleText;
    
    private List<RelicManager.RelicData> currentChoices;
    private bool waitingForSelection = false;
    
    public static RelicSelectionUI Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("RelicSelectionUI instance created successfully");
        }
        else
        {
            Debug.LogWarning("Duplicate RelicSelectionUI found, destroying this one");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("RelicSelectionUI Start() called");
        if (relicSelectionPanel != null)
        {
            relicSelectionPanel.SetActive(false);
            Debug.Log("Relic selection panel initially hidden");
        }
        else
        {
            Debug.LogError("relicSelectionPanel is NULL in Start()!");
        }
        
        // Setup button listeners
        for (int i = 0; i < relicChoiceButtons.Length; i++)
        {
            int buttonIndex = i; // Capture for closure
            relicChoiceButtons[i].onClick.RemoveAllListeners();
            relicChoiceButtons[i].onClick.AddListener(() => SelectRelic(buttonIndex));
        }
        Debug.Log($"Setup {relicChoiceButtons.Length} relic choice buttons");
    }

    public void ShowRelicSelection(List<RelicManager.RelicData> choices)
    {
        Debug.Log($"ShowRelicSelection() called with {(choices != null ? choices.Count : 0)} choices");
        if (choices == null || choices.Count == 0)
        {
            Debug.LogWarning("No relic choices provided");
            return;
        }

        currentChoices = choices;
        waitingForSelection = true;
        
        // Setup UI
        if (selectionTitleText != null)
        {
            selectionTitleText.text = "Choose a Relic";
        }
        
        // Setup choice buttons
        for (int i = 0; i < relicChoiceButtons.Length; i++)
        {
            if (i < choices.Count)
            {
                SetupRelicChoice(i, choices[i]);
                relicChoiceButtons[i].gameObject.SetActive(true);
                Debug.Log($"Setup relic choice button {i} for {choices[i].name}");
            }
            else
            {
                relicChoiceButtons[i].gameObject.SetActive(false);
            }
        }
        
        // Show panel
        if (relicSelectionPanel != null)
        {
            relicSelectionPanel.SetActive(true);
            Debug.Log("Relic selection panel is now visible");
        }
        else
        {
            Debug.LogError("relicSelectionPanel is NULL! Cannot show relic selection.");
        }
        
        Debug.Log($"Showing relic selection with {choices.Count} choices");
    }

    void SetupRelicChoice(int index, RelicManager.RelicData relicData)
    {
        // Set relic name
        if (relicChoiceNames[index] != null)
        {
            relicChoiceNames[index].text = relicData.name;
        }
        
        // Set relic description
        if (relicChoiceDescriptions[index] != null)
        {
            string description = $"{relicData.trigger.description}, {relicData.effect.description}";
            relicChoiceDescriptions[index].text = description;
        }
        
        // Set relic icon using sprite manager if available
        if (relicChoiceIcons[index] != null)
        {
            if (GameManager.Instance?.relicIconManager != null)
            {
                // Check if we have enough sprites
                if (relicData.sprite < GameManager.Instance.relicIconManager.GetCount())
                {
                    // Use actual sprite from RelicIconManager
                    GameManager.Instance.relicIconManager.PlaceSprite(relicData.sprite, relicChoiceIcons[index]);
                    // Reset color to white to show the sprite properly
                    relicChoiceIcons[index].color = Color.white;
                    Debug.Log($"Applied sprite {relicData.sprite} for relic {relicData.name}");
                }
                else
                {
                    // Sprite index out of range, use color coding
                    Color relicColor = GetRelicColor(relicData.sprite);
                    relicChoiceIcons[index].color = relicColor;
                    Debug.LogWarning($"RelicIconManager has {GameManager.Instance.relicIconManager.GetCount()} sprites, but relic {relicData.name} needs sprite index {relicData.sprite}. Using color coding instead.");
                }
            }
            else
            {
                // Fallback to color coding if no sprite manager
                Color relicColor = GetRelicColor(relicData.sprite);
                relicChoiceIcons[index].color = relicColor;
                Debug.LogWarning("RelicIconManager not found, using color coding for relic icons");
            }
        }
    }

    Color GetRelicColor(int spriteIndex)
    {
        // Same color mapping as RelicUI
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

    void SelectRelic(int choiceIndex)
    {
        if (!waitingForSelection || currentChoices == null || choiceIndex >= currentChoices.Count)
        {
            Debug.LogWarning($"Invalid relic selection: index {choiceIndex}");
            return;
        }

        var selectedRelic = currentChoices[choiceIndex];
        Debug.Log($"Player selected relic: {selectedRelic.name}");
        
        // Give relic to player
        if (RelicManager.Instance != null)
        {
            RelicManager.Instance.SelectRelic(selectedRelic);
        }
        
        // Hide selection UI
        HideRelicSelection();
        
        // Continue game flow
        ContinueAfterSelection();
    }

    void ContinueAfterSelection()
    {
        // Find and notify the enemy spawner to continue
        var enemySpawner = FindObjectOfType<EnemySpawner>();
        if (enemySpawner != null)
        {
            // Use a coroutine to ensure UI changes are applied first
            StartCoroutine(ContinueAfterSelectionCoroutine(enemySpawner));
        }
    }

    System.Collections.IEnumerator ContinueAfterSelectionCoroutine(EnemySpawner spawner)
    {
        yield return null; // Wait one frame
        spawner.NextWave();
    }

    public void HideRelicSelection()
    {
        waitingForSelection = false;
        currentChoices = null;
        
        if (relicSelectionPanel != null)
        {
            relicSelectionPanel.SetActive(false);
        }
    }

    public bool IsWaitingForSelection()
    {
        return waitingForSelection;
    }
} 