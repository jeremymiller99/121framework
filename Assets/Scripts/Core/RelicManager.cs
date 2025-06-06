using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;

public class RelicManager : MonoBehaviour
{
    [System.Serializable]
    public class RelicData
    {
        public string name;
        public int sprite;
        public TriggerData trigger;
        public EffectData effect;
    }

    [System.Serializable]
    public class TriggerData
    {
        public string description;
        public string type;
        public string amount;
    }

    [System.Serializable]
    public class EffectData
    {
        public string description;
        public string type;
        public string amount;
        public string until;
    }

    private List<RelicData> allRelicData;
    private List<Relic> playerRelics;
    private List<RelicData> availableRelics; // Relics not yet obtained
    
    public static RelicManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            playerRelics = new List<Relic>();
            LoadRelicsFromJson();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Subscribe to wave completion for relic drops
        EventBus.Instance.OnWaveComplete += OnWaveComplete;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            EventBus.Instance.OnWaveComplete -= OnWaveComplete;
            ClearAllRelics();
        }
    }

    private void LoadRelicsFromJson()
    {
        try
        {
            TextAsset relicsJson = Resources.Load<TextAsset>("relics");
            if (relicsJson != null)
            {
                string jsonContent = relicsJson.text;
                allRelicData = JsonConvert.DeserializeObject<List<RelicData>>(jsonContent);
                
                // Add custom relics
                AddCustomRelics();
                
                // Initialize available relics (copy of all relics)
                availableRelics = new List<RelicData>(allRelicData);
                
                Debug.Log($"Loaded {allRelicData.Count} relics from JSON");
            }
            else
            {
                Debug.LogError("Failed to load relics.json from Resources folder");
                allRelicData = new List<RelicData>();
                availableRelics = new List<RelicData>();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading relics: {e.Message}");
            allRelicData = new List<RelicData>();
            availableRelics = new List<RelicData>();
        }
    }

    private void AddCustomRelics()
    {
        // Add at least 3 custom relics as required
        var customRelics = new List<RelicData>
        {
            // Relic 1: Mana on damage dealt
            new RelicData
            {
                name = "Mystic Orb",
                sprite = 4,
                trigger = new TriggerData { description = "Whenever you cast a spell", type = "cast-spell" },
                effect = new EffectData { description = "you replenish 2 mana", type = "gain-mana", amount = "2" }
            },
            
            // Relic 2: Max health on wave completion
            new RelicData
            {
                name = "Vitality Crystal",
                sprite = 5,
                trigger = new TriggerData { description = "Whenever you complete a wave", type = "wave-complete" },
                effect = new EffectData { description = "you gain 10 max health", type = "gain-max-health", amount = "10" }
            },
            
            // Relic 3: Heal when taking damage (ironic but useful)
            new RelicData
            {
                name = "Phoenix Feather",
                sprite = 6,
                trigger = new TriggerData { description = "When you take damage", type = "take-damage" },
                effect = new EffectData { description = "you heal 3 health", type = "heal", amount = "3" }
            },
            
            // Relic 4: Spell power boost when killing enemies
            new RelicData
            {
                name = "Soul Harvester",
                sprite = 7,
                trigger = new TriggerData { description = "When you kill an enemy", type = "on-kill" },
                effect = new EffectData { description = "you gain 2 spell power permanently", type = "gain-spellpower", amount = "2" }
            }
        };

        allRelicData.AddRange(customRelics);
    }

    private void OnWaveComplete(int waveNumber)
    {
        Debug.Log($"OnWaveComplete triggered for wave {waveNumber}");
        // Drop relics every 3rd wave starting from wave 3
        if (waveNumber % 3 == 0 && waveNumber >= 3)
        {
            Debug.Log($"Wave {waveNumber} is a relic wave - offering relic choice");
            OfferRelicChoice();
        }
        else
        {
            Debug.Log($"Wave {waveNumber} is not a relic wave (every 3rd wave starting from wave 3)");
        }
    }

    public void OfferRelicChoice()
    {
        Debug.Log($"OfferRelicChoice() called - available relics: {availableRelics.Count}");
        if (availableRelics.Count == 0)
        {
            Debug.Log("No more relics available to offer");
            return;
        }

        // Select 3 random available relics
        List<RelicData> choiceRelics = new List<RelicData>();
        var shuffled = availableRelics.OrderBy(x => Random.value).ToList();
        
        for (int i = 0; i < Mathf.Min(3, shuffled.Count); i++)
        {
            choiceRelics.Add(shuffled[i]);
        }
        
        Debug.Log($"Selected {choiceRelics.Count} relic choices: {string.Join(", ", choiceRelics.Select(r => r.name))}");

        // Always show relic selection UI and wait for player choice
        if (RelicSelectionUI.Instance != null)
        {
            Debug.Log("RelicSelectionUI found - showing relic selection");
            RelicSelectionUI.Instance.ShowRelicSelection(choiceRelics);
            Debug.Log($"Offered relic choices: {string.Join(", ", choiceRelics.Select(r => r.name))}");
        }
        else
        {
            Debug.LogError("RelicSelectionUI not found! Cannot offer relic choice - this should never happen in normal gameplay.");
            // Don't auto-assign - just log an error and continue without giving a relic
            // This ensures the player always gets to choose
        }
    }

    public void SelectRelic(RelicData relicData)
    {
        // Create the relic instance
        Relic relic = CreateRelicInstance(relicData);
        if (relic != null)
        {
            playerRelics.Add(relic);
            availableRelics.Remove(relicData);
            
            Debug.Log($"Player selected relic: {relic.name} - {relic.GetDescription()}");
        }
    }

    private Relic CreateRelicInstance(RelicData data)
    {
        // Create effect first
        IRelicEffect effect = CreateEffect(data.effect);
        if (effect == null)
        {
            Debug.LogError($"Failed to create effect for relic {data.name}");
            return null;
        }

        // Create trigger with proper effect linking
        IRelicTrigger trigger = CreateTriggerLinked(data.trigger, effect);
        if (trigger == null)
        {
            Debug.LogError($"Failed to create trigger for relic {data.name}");
            return null;
        }

        return new Relic(data.name, data.sprite, data.trigger.description, data.effect.description, trigger, effect);
    }

    private IRelicTrigger CreateTriggerLinked(TriggerData triggerData, IRelicEffect effect)
    {
        return triggerData.type switch
        {
            "take-damage" => new TakeDamageTrigger(effect),
            "on-kill" => new OnKillTrigger(effect),
            "stand-still" => new StandStillTrigger(effect, ParseFloat(triggerData.amount, 3f)),
            "cast-spell" => new SpellCastTrigger(effect),
            "wave-complete" => new WaveCompleteTrigger(effect),
            _ => null
        };
    }

    private IRelicEffect CreateEffect(EffectData effectData)
    {
        return effectData.type switch
        {
            "gain-mana" => new GainManaEffect(ParseInt(effectData.amount, 5)),
            "gain-spellpower" when !string.IsNullOrEmpty(effectData.until) && effectData.until == "cast-spell" 
                => new NextSpellModifierEffect(ParseInt(effectData.amount, 100)),
            "gain-spellpower" => new GainSpellPowerEffect(effectData.amount, effectData.until),
            "gain-max-health" => new GainMaxHealthEffect(ParseInt(effectData.amount, 10)),
            "heal" => new HealEffect(ParseInt(effectData.amount, 5)),
            _ => null
        };
    }

    private int ParseInt(string value, int defaultValue)
    {
        if (string.IsNullOrEmpty(value))
            return defaultValue;
            
        if (int.TryParse(value, out int result))
            return result;
            
        return defaultValue;
    }

    private float ParseFloat(string value, float defaultValue)
    {
        if (string.IsNullOrEmpty(value))
            return defaultValue;
            
        if (float.TryParse(value, out float result))
            return result;
            
        return defaultValue;
    }

    public void ClearAllRelics()
    {
        foreach (var relic in playerRelics)
        {
            relic.Destroy();
        }
        playerRelics.Clear();
        
        // Clear any active next spell modifiers
        NextSpellModifierManager.ClearModifiers();
        
        // Reset available relics
        if (allRelicData != null)
        {
            availableRelics = new List<RelicData>(allRelicData);
        }
    }

    public List<Relic> GetPlayerRelics()
    {
        return new List<Relic>(playerRelics);
    }

    public int GetRelicCount()
    {
        return playerRelics.Count;
    }
} 