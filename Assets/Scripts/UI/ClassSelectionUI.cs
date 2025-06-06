using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ClassSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject classSelectionPanel;
    public Button[] classButtons;
    public TextMeshProUGUI[] classNameTexts;
    public TextMeshProUGUI[] classDescriptionTexts;
    public Button confirmButton;
    public TextMeshProUGUI selectedClassText;
    
    private string selectedClassName;
    private bool hasSelectedClass = false;
    
    public static ClassSelectionUI Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("ClassSelectionUI instance created successfully");
        }
        else
        {
            Debug.LogWarning("Duplicate ClassSelectionUI found, destroying this one");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("ClassSelectionUI Start() called");
        SetupClassSelection();
        
        // Don't show class selection automatically on start anymore
        // Class selection will be shown by EnemySpawner after mode selection
        HideClassSelection();
        Debug.Log("ClassSelectionUI Start() completed - UI hidden");
    }

    void SetupClassSelection()
    {
        Debug.Log("Setting up class selection UI");
        if (CharacterClassManager.Instance == null)
        {
            Debug.LogWarning("CharacterClassManager not found, cannot setup class selection");
            return;
        }

        var classNames = CharacterClassManager.Instance.GetClassNames();
        Debug.Log($"Found {classNames.Count} character classes: {string.Join(", ", classNames)}");
        
        // Setup class buttons
        for (int i = 0; i < classButtons.Length && i < classNames.Count; i++)
        {
            string className = classNames[i];
            var characterClass = CharacterClassManager.Instance.GetClass(className);
            
            if (characterClass != null)
            {
                // Set class name
                if (classNameTexts[i] != null)
                {
                    classNameTexts[i].text = className.ToUpper();
                }
                
                // Set class description (basic stats at wave 1)
                if (classDescriptionTexts[i] != null)
                {
                    string description = $"Health: {characterClass.CalculateHealth(1)}\n";
                    description += $"Mana: {characterClass.CalculateMana(1)}\n";
                    description += $"Spell Power: {characterClass.CalculateSpellPower(1)}\n";
                    description += $"Speed: {characterClass.CalculateSpeed(1)}";
                    classDescriptionTexts[i].text = description;
                }
                
                // Setup button click
                int buttonIndex = i; // Capture for closure
                classButtons[i].onClick.RemoveAllListeners();
                classButtons[i].onClick.AddListener(() => SelectClass(className));
                classButtons[i].gameObject.SetActive(true);
                Debug.Log($"Setup button {i} for class {className}");
            }
            else
            {
                classButtons[i].gameObject.SetActive(false);
                Debug.LogWarning($"Character class '{className}' not found when setting up UI");
            }
        }
        
        // Hide unused buttons
        for (int i = classNames.Count; i < classButtons.Length; i++)
        {
            classButtons[i].gameObject.SetActive(false);
        }
        
        // Setup confirm button
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(ConfirmSelection);
            confirmButton.interactable = false;
            Debug.Log("Confirm button setup completed");
        }
        
        Debug.Log("Class selection setup completed");
    }

    public void ShowClassSelection()
    {
        Debug.Log($"ShowClassSelection() called - panel reference: {(classSelectionPanel != null ? "Valid" : "NULL")}");
        if (classSelectionPanel != null)
        {
            classSelectionPanel.SetActive(true);
            hasSelectedClass = false;
            selectedClassName = null;
            
            if (confirmButton != null)
                confirmButton.interactable = false;
            if (selectedClassText != null)
                selectedClassText.text = "Select a class";
                
            Debug.Log("Class selection UI is now visible");
        }
        else
        {
            Debug.LogError("classSelectionPanel is NULL! Cannot show class selection.");
        }
    }

    public void HideClassSelection()
    {
        if (classSelectionPanel != null)
        {
            classSelectionPanel.SetActive(false);
        }
    }

    void SelectClass(string className)
    {
        selectedClassName = className;
        hasSelectedClass = true;
        
        if (selectedClassText != null)
        {
            selectedClassText.text = $"Selected: {className.ToUpper()}";
        }
        
        if (confirmButton != null)
        {
            confirmButton.interactable = true;
        }
        
        Debug.Log($"Selected class: {className}");
    }

    void ConfirmSelection()
    {
        if (hasSelectedClass && !string.IsNullOrEmpty(selectedClassName))
        {
            CharacterClassManager.Instance?.SelectClass(selectedClassName);
            HideClassSelection();
            Debug.Log($"Confirmed class selection: {selectedClassName}");
        }
    }

    public bool IsClassSelected()
    {
        return CharacterClassManager.Instance?.GetSelectedClass() != null;
    }
} 