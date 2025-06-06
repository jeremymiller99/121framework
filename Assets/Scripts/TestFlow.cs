using UnityEngine;

public class TestFlow : MonoBehaviour
{
    [Header("Debug Controls")]
    public KeyCode testClassSelectionKey = KeyCode.C;
    public KeyCode testRelicSelectionKey = KeyCode.R;
    public KeyCode clearSelectionKey = KeyCode.X;

    void Update()
    {
        if (Input.GetKeyDown(testClassSelectionKey))
        {
            TestClassSelection();
        }
        
        if (Input.GetKeyDown(testRelicSelectionKey))
        {
            TestRelicSelection();
        }
        
        if (Input.GetKeyDown(clearSelectionKey))
        {
            ClearSelections();
        }
    }
    
    void TestClassSelection()
    {
        Debug.Log("=== TESTING CLASS SELECTION ===");
        Debug.Log($"ClassSelectionUI.Instance: {(ClassSelectionUI.Instance != null ? "Found" : "NULL")}");
        Debug.Log($"CharacterClassManager.Instance: {(CharacterClassManager.Instance != null ? "Found" : "NULL")}");
        
        if (ClassSelectionUI.Instance != null)
        {
            Debug.Log("Showing class selection UI");
            ClassSelectionUI.Instance.ShowClassSelection();
        }
        else
        {
            Debug.LogError("ClassSelectionUI.Instance is NULL!");
        }
    }
    
    void TestRelicSelection()
    {
        Debug.Log("=== TESTING RELIC SELECTION ===");
        Debug.Log($"RelicSelectionUI.Instance: {(RelicSelectionUI.Instance != null ? "Found" : "NULL")}");
        Debug.Log($"RelicManager.Instance: {(RelicManager.Instance != null ? "Found" : "NULL")}");
        
        if (RelicManager.Instance != null)
        {
            Debug.Log("Triggering relic offer choice");
            RelicManager.Instance.OfferRelicChoice();
        }
        else
        {
            Debug.LogError("RelicManager.Instance is NULL!");
        }
    }
    
    void ClearSelections()
    {
        Debug.Log("=== CLEARING ALL SELECTIONS ===");
        
        if (CharacterClassManager.Instance != null)
        {
            CharacterClassManager.Instance.ClearSelection();
            Debug.Log("Character class selection cleared");
        }
        
        if (ClassSelectionUI.Instance != null)
        {
            ClassSelectionUI.Instance.HideClassSelection();
            Debug.Log("Class selection UI hidden");
        }
        
        if (RelicSelectionUI.Instance != null)
        {
            RelicSelectionUI.Instance.HideRelicSelection();
            Debug.Log("Relic selection UI hidden");
        }
        
        if (RelicManager.Instance != null)
        {
            RelicManager.Instance.ClearAllRelics();
            Debug.Log("All relics cleared");
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("=== DEBUG CONTROLS ===");
        GUILayout.Label($"Press '{testClassSelectionKey}' to test class selection");
        GUILayout.Label($"Press '{testRelicSelectionKey}' to test relic selection");
        GUILayout.Label($"Press '{clearSelectionKey}' to clear all selections");
        
        GUILayout.Space(10);
        GUILayout.Label("=== STATUS ===");
        GUILayout.Label($"ClassSelectionUI: {(ClassSelectionUI.Instance != null ? "OK" : "NULL")}");
        GUILayout.Label($"RelicSelectionUI: {(RelicSelectionUI.Instance != null ? "OK" : "NULL")}");
        GUILayout.Label($"CharacterClassManager: {(CharacterClassManager.Instance != null ? "OK" : "NULL")}");
        GUILayout.Label($"RelicManager: {(RelicManager.Instance != null ? "OK" : "NULL")}");
        
        if (CharacterClassManager.Instance != null)
        {
            var selectedClass = CharacterClassManager.Instance.GetSelectedClass();
            GUILayout.Label($"Selected Class: {(selectedClass != null ? selectedClass.name : "None")}");
        }
        
        GUILayout.EndArea();
    }
} 