using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [Header("Manager Prefabs")]
    public GameObject characterClassManagerPrefab;
    public GameObject relicManagerPrefab;
    
    void Awake()
    {
        // Initialize managers in the correct order
        InitializeManagers();
    }

    void InitializeManagers()
    {
        // Ensure CharacterClassManager exists
        if (CharacterClassManager.Instance == null && characterClassManagerPrefab != null)
        {
            Instantiate(characterClassManagerPrefab);
            Debug.Log("CharacterClassManager instantiated");
        }
        
        // Ensure RelicManager exists
        if (RelicManager.Instance == null && relicManagerPrefab != null)
        {
            Instantiate(relicManagerPrefab);
            Debug.Log("RelicManager instantiated");
        }
        
        // If prefabs are not assigned, create managers directly
        if (CharacterClassManager.Instance == null)
        {
            var ccmGO = new GameObject("CharacterClassManager");
            ccmGO.AddComponent<CharacterClassManager>();
            Debug.Log("CharacterClassManager created directly");
        }
        
        if (RelicManager.Instance == null)
        {
            var rmGO = new GameObject("RelicManager");
            rmGO.AddComponent<RelicManager>();
            Debug.Log("RelicManager created directly");
        }
    }
} 