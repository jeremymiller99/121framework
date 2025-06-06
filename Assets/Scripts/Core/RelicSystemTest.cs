using UnityEngine;

public class RelicSystemTest : MonoBehaviour
{
    [Header("Test Controls")]
    public bool testRelicSystem = false;
    public bool giveTestRelic = false;
    public bool triggerWaveComplete = false;
    public bool testAllRelics = false;
    public bool testGoldenMask = false;
    public bool testJadeElephant = false;
    public int testWaveNumber = 3;

    [Header("Debug")]
    public bool debugRelicIcons = false;

    void Update()
    {
        if (testRelicSystem)
        {
            testRelicSystem = false;
            TestRelicSystem();
        }

        if (giveTestRelic)
        {
            giveTestRelic = false;
            GiveTestRelic();
        }

        if (triggerWaveComplete)
        {
            triggerWaveComplete = false;
            TriggerTestWaveComplete();
        }

        if (testAllRelics)
        {
            testAllRelics = false;
            TestAllRelics();
        }

        if (testGoldenMask)
        {
            testGoldenMask = false;
            TestGoldenMaskRelic();
        }

        if (testJadeElephant)
        {
            testJadeElephant = false;
            TestJadeElephantRelic();
        }

        if (debugRelicIcons)
        {
            debugRelicIcons = false;
            DebugRelicIcons();
        }
    }

    void TestRelicSystem()
    {
        Debug.Log("=== Testing Relic System ===");
        
        // Test RelicManager
        if (RelicManager.Instance != null)
        {
            Debug.Log($"RelicManager found with {RelicManager.Instance.GetRelicCount()} relics");
            
            // Test offering relic choice
            RelicManager.Instance.OfferRelicChoice();
        }
        else
        {
            Debug.LogError("RelicManager not found!");
        }
        
        // Test CharacterClassManager
        if (CharacterClassManager.Instance != null)
        {
            var selectedClass = CharacterClassManager.Instance.GetSelectedClass();
            if (selectedClass != null)
            {
                Debug.Log($"Selected class: {selectedClass.name}");
                Debug.Log($"Wave 1 stats - HP: {selectedClass.CalculateHealth(1)}, Mana: {selectedClass.CalculateMana(1)}, Spell Power: {selectedClass.CalculateSpellPower(1)}");
            }
            else
            {
                Debug.Log("No class selected, using default");
            }
        }
        else
        {
            Debug.LogError("CharacterClassManager not found!");
        }
    }

    void TestAllRelics()
    {
        Debug.Log("=== Testing All Relic Types ===");
        
        if (RelicManager.Instance == null)
        {
            Debug.LogError("RelicManager not found!");
            return;
        }

        // Test each relic type
        var testRelics = new[]
        {
            // Green Gem - gain mana on damage
            new RelicManager.RelicData
            {
                name = "Test Green Gem",
                sprite = 0,
                trigger = new RelicManager.TriggerData { description = "When you take damage", type = "take-damage" },
                effect = new RelicManager.EffectData { description = "you gain 5 mana", type = "gain-mana", amount = "5" }
            },
            
            // Golden Mask - next spell gains spell power
            new RelicManager.RelicData
            {
                name = "Test Golden Mask",
                sprite = 2,
                trigger = new RelicManager.TriggerData { description = "When you take damage", type = "take-damage" },
                effect = new RelicManager.EffectData { description = "your next spell gains 100 additional spellpower", type = "gain-spellpower", amount = "100", until = "cast-spell" }
            },
            
            // Cursed Scroll - gain mana on kill
            new RelicManager.RelicData
            {
                name = "Test Cursed Scroll",
                sprite = 3,
                trigger = new RelicManager.TriggerData { description = "When you kill an enemy", type = "on-kill" },
                effect = new RelicManager.EffectData { description = "you gain 10 mana", type = "gain-mana", amount = "10" }
            }
        };

        foreach (var relic in testRelics)
        {
            RelicManager.Instance.SelectRelic(relic);
            Debug.Log($"Added test relic: {relic.name}");
        }
        
        Debug.Log($"Total relics after test: {RelicManager.Instance.GetRelicCount()}");
    }

    void TestGoldenMaskRelic()
    {
        Debug.Log("=== Testing Golden Mask Relic ===");
        
        if (RelicManager.Instance == null)
        {
            Debug.LogError("RelicManager not found!");
            return;
        }

        // Give Golden Mask relic
        var goldenMask = new RelicManager.RelicData
        {
            name = "Test Golden Mask",
            sprite = 2,
            trigger = new RelicManager.TriggerData { description = "When you take damage", type = "take-damage" },
            effect = new RelicManager.EffectData { description = "your next spell gains 100 additional spellpower", type = "gain-spellpower", amount = "100", until = "cast-spell" }
        };

        RelicManager.Instance.SelectRelic(goldenMask);
        Debug.Log("Golden Mask relic added. Now trigger damage to activate it.");
        
        // Simulate taking damage
        var player = GameManager.Instance?.player?.GetComponent<PlayerController>();
        if (player?.hp != null)
        {
            EventBus.Instance.DoDamage(player.transform.position, new Damage(1, Damage.Type.PHYSICAL), player.hp);
            Debug.Log("Simulated player taking damage - Golden Mask should now be active for next spell");
        }
    }

    void TestJadeElephantRelic()
    {
        Debug.Log("=== Testing Jade Elephant Relic ===");
        
        if (RelicManager.Instance == null)
        {
            Debug.LogError("RelicManager not found!");
            return;
        }

        // Give Jade Elephant relic
        var jadeElephant = new RelicManager.RelicData
        {
            name = "Test Jade Elephant",
            sprite = 1,
            trigger = new RelicManager.TriggerData { description = "When you don't move for 3 seconds", type = "stand-still", amount = "3" },
            effect = new RelicManager.EffectData { description = "you gain 10 spellpower (+5/wave)", type = "gain-spellpower", amount = "10 wave 5 * +", until = "move" }
        };

        RelicManager.Instance.SelectRelic(jadeElephant);
        Debug.Log("Jade Elephant relic added. Stand still for 3 seconds to activate it.");
    }

    void GiveTestRelic()
    {
        if (RelicManager.Instance == null)
        {
            Debug.LogError("RelicManager not found!");
            return;
        }

        // Create a test relic data
        var testRelic = new RelicManager.RelicData
        {
            name = "Test Relic",
            sprite = 0,
            trigger = new RelicManager.TriggerData 
            { 
                description = "When you take damage", 
                type = "take-damage" 
            },
            effect = new RelicManager.EffectData 
            { 
                description = "you gain 5 mana", 
                type = "gain-mana", 
                amount = "5" 
            }
        };

        RelicManager.Instance.SelectRelic(testRelic);
        Debug.Log("Test relic given to player");
    }

    void TriggerTestWaveComplete()
    {
        Debug.Log($"Triggering wave {testWaveNumber} complete event");
        EventBus.Instance.FireWaveComplete(testWaveNumber);
    }

    void DebugRelicIcons()
    {
        Debug.Log("=== Debugging Relic Icon System ===");
        
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null!");
            return;
        }
        
        if (GameManager.Instance.relicIconManager == null)
        {
            Debug.LogError("GameManager.Instance.relicIconManager is null! You need to:");
            Debug.LogError("1. Add a RelicIconManager component to a GameObject in your scene");
            Debug.LogError("2. Assign relic sprite images to the 'sprites' array in the RelicIconManager component");
            Debug.LogError("3. Make sure the RelicIconManager's Start() method runs to register itself with GameManager");
            return;
        }
        
        var iconManager = GameManager.Instance.relicIconManager;
        int spriteCount = iconManager.GetCount();
        
        Debug.Log($"RelicIconManager found with {spriteCount} sprites");
        
        if (spriteCount == 0)
        {
            Debug.LogWarning("RelicIconManager has 0 sprites! You need to:");
            Debug.LogWarning("1. Create or import relic sprite images (PNG files)");
            Debug.LogWarning("2. Assign them to the 'sprites' array in the RelicIconManager component");
            Debug.LogWarning("3. Each relic in relics.json references a sprite by index (0, 1, 2, etc.)");
        }
        else
        {
            Debug.Log("Sprite assignments:");
            for (int i = 0; i < spriteCount; i++)
            {
                var sprite = iconManager.Get(i);
                Debug.Log($"  Index {i}: {(sprite != null ? sprite.name : "NULL")}");
            }
        }
        
        // Check relics that need sprites
        if (RelicManager.Instance != null)
        {
            // We can't access allRelicData directly, so let's test with known indices
            int[] neededIndices = { 0, 1, 2, 3, 4, 5, 6, 7 }; // Based on relics.json and custom relics
            Debug.Log("Relics that need sprite indices:");
            foreach (int index in neededIndices)
            {
                if (index >= spriteCount)
                {
                    Debug.LogWarning($"  Sprite index {index} needed but only {spriteCount} sprites available");
                }
                else
                {
                    Debug.Log($"  Sprite index {index}: OK");
                }
            }
        }
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 350));
        GUILayout.Label("Relic System Test");
        
        if (GUILayout.Button("Test Relic System"))
        {
            TestRelicSystem();
        }
        
        if (GUILayout.Button("Give Test Relic"))
        {
            GiveTestRelic();
        }
        
        if (GUILayout.Button($"Trigger Wave {testWaveNumber} Complete"))
        {
            TriggerTestWaveComplete();
        }
        
        if (GUILayout.Button("Test All Relics"))
        {
            TestAllRelics();
        }
        
        if (GUILayout.Button("Test Golden Mask"))
        {
            TestGoldenMaskRelic();
        }
        
        if (GUILayout.Button("Test Jade Elephant"))
        {
            TestJadeElephantRelic();
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Debug Tools:");
        
        if (GUILayout.Button("Debug Relic Icons"))
        {
            DebugRelicIcons();
        }
        
        testWaveNumber = (int)GUILayout.HorizontalSlider(testWaveNumber, 1, 10);
        GUILayout.Label($"Test Wave: {testWaveNumber}");
        
        // Show current relic count
        if (RelicManager.Instance != null)
        {
            GUILayout.Label($"Current Relics: {RelicManager.Instance.GetRelicCount()}");
        }
        
        // Show next spell modifier status
        if (NextSpellModifierManager.HasModifiers())
        {
            GUILayout.Label("Next Spell Modifier: ACTIVE");
        }
        
        // Show icon manager status
        if (GameManager.Instance?.relicIconManager != null)
        {
            GUILayout.Label($"Icon Manager: {GameManager.Instance.relicIconManager.GetCount()} sprites");
        }
        else
        {
            GUILayout.Label("Icon Manager: NOT FOUND");
        }
        
        GUILayout.EndArea();
    }
} 