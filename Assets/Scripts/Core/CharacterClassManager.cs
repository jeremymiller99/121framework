using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class CharacterClassManager : MonoBehaviour
{
    private Dictionary<string, CharacterClass> characterClasses;
    private CharacterClass selectedClass;
    
    public static CharacterClassManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadClassesFromJson();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadClassesFromJson()
    {
        characterClasses = new Dictionary<string, CharacterClass>();
        
        try
        {
            TextAsset classesJson = Resources.Load<TextAsset>("classes");
            if (classesJson != null)
            {
                string jsonContent = classesJson.text;
                JObject classData = JsonConvert.DeserializeObject<JObject>(jsonContent);
                
                foreach (var kvp in classData)
                {
                    string className = kvp.Key;
                    JObject classInfo = kvp.Value as JObject;
                    
                    CharacterClass charClass = new CharacterClass(
                        className,
                        classInfo["sprite"]?.Value<int>() ?? 0,
                        classInfo["health"]?.ToString() ?? "100",
                        classInfo["mana"]?.ToString() ?? "50",
                        classInfo["mana_regeneration"]?.ToString() ?? "5",
                        classInfo["spellpower"]?.ToString() ?? "10",
                        classInfo["speed"]?.ToString() ?? "5"
                    );
                    
                    characterClasses[className] = charClass;
                }
                
                Debug.Log($"Loaded {characterClasses.Count} character classes");
                
                // Don't default to any class - force player to choose
                selectedClass = null;
                Debug.Log("No default class selected - player must choose");
            }
            else
            {
                Debug.LogError("Failed to load classes.json from Resources folder");
                CreateDefaultClasses();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading character classes: {e.Message}");
            CreateDefaultClasses();
        }
    }

    private void CreateDefaultClasses()
    {
        // Create a default mage class if loading fails
        characterClasses = new Dictionary<string, CharacterClass>();
        var mageClass = new CharacterClass(
            "mage",
            0,
            "95 wave 12 * + wave wave * 2 * +",
            "90 wave 15 * + wave wave * 3 * +",
            "10 wave 2 * + wave 10 / +",
            "wave 12 *",
            "5 wave 15 / +"
        );
        characterClasses["mage"] = mageClass;
        selectedClass = null; // Don't default to any class
        Debug.Log("Created default mage class - no class selected");
    }

    public void SelectClass(string className)
    {
        if (characterClasses.ContainsKey(className))
        {
            selectedClass = characterClasses[className];
            Debug.Log($"Selected character class: {className}");
        }
        else
        {
            Debug.LogWarning($"Character class '{className}' not found");
        }
    }

    public void ClearSelection()
    {
        selectedClass = null;
        Debug.Log("Character class selection cleared");
    }

    public CharacterClass GetSelectedClass()
    {
        return selectedClass;
    }

    public List<CharacterClass> GetAllClasses()
    {
        return new List<CharacterClass>(characterClasses.Values);
    }

    public List<string> GetClassNames()
    {
        return new List<string>(characterClasses.Keys);
    }

    public CharacterClass GetClass(string className)
    {
        return characterClasses.TryGetValue(className, out CharacterClass charClass) ? charClass : null;
    }
} 