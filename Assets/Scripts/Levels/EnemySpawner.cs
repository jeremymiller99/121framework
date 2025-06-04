using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    [Header("UI References")]
    public Image level_selector;
    public GameObject button;
    public GameObject continueButton;
    public TextMeshProUGUI waveInfoText;
    public TextMeshProUGUI levelInfoText;
    
    [Header("Game References")]
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;    

    // Data storage
    private List<EnemyData> enemyTypes;
    private List<LevelData> levels;
    private Dictionary<string, EnemyData> enemyLookup;
    
    // Current game state
    private LevelData currentLevel;
    private int currentWave = 0;
    private bool gameInProgress = false;
    private bool waitingForContinue = false;

    void Start()
    {
        LoadGameData();
        SetupLevelSelection();
    }

    void LoadGameData()
    {
        // Load enemies
        string enemyPath = Path.Combine(Application.streamingAssetsPath, "enemies.json");
        if (!File.Exists(enemyPath))
            enemyPath = Path.Combine(Application.dataPath, "Resources/enemies.json");
        
        string enemyJson = File.ReadAllText(enemyPath);
        enemyTypes = JsonConvert.DeserializeObject<List<EnemyData>>(enemyJson);
        
        // Create lookup dictionary
        enemyLookup = new Dictionary<string, EnemyData>();
        foreach (var enemy in enemyTypes)
        {
            enemyLookup[enemy.name] = enemy;
        }

        // Load levels
        string levelPath = Path.Combine(Application.streamingAssetsPath, "levels.json");
        if (!File.Exists(levelPath))
            levelPath = Path.Combine(Application.dataPath, "Resources/levels.json");
            
        string levelJson = File.ReadAllText(levelPath);
        levels = JsonConvert.DeserializeObject<List<LevelData>>(levelJson);

        Debug.Log($"Loaded {enemyTypes.Count} enemy types and {levels.Count} levels");
    }

    void SetupLevelSelection()
    {
        // Clear existing buttons
        foreach (Transform child in level_selector.transform)
        {
            if (child.gameObject != button)
                Destroy(child.gameObject);
        }

        // Create buttons for each level
        for (int i = 0; i < levels.Count; i++)
        {
            GameObject selector = Instantiate(button, level_selector.transform);
            selector.transform.localPosition = new Vector3(0, 130 - i * 60);
            
            var menuController = selector.GetComponent<MenuSelectorController>();
            menuController.spawner = this;
            menuController.SetLevel(levels[i].name);
            
            selector.SetActive(true);
        }
        
        button.SetActive(false);
    }

    public void StartLevel(string levelname)
    {
        currentLevel = levels.FirstOrDefault(l => l.name == levelname);
        if (currentLevel == null)
        {
            Debug.LogError($"Level '{levelname}' not found!");
            return;
        }

        currentWave = 1;
        gameInProgress = true;
        waitingForContinue = false;
        
        level_selector.gameObject.SetActive(false);
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
        
        StartCoroutine(RunLevel());
    }

    IEnumerator RunLevel()
    {
        while (gameInProgress)
        {
            yield return StartCoroutine(SpawnWave());
            
            // Wait for all enemies to be defeated
            yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
            
            GameManager.Instance.state = GameManager.GameState.WAVEEND;
            
            // Check win condition
            if (currentLevel.waves > 0 && currentWave >= currentLevel.waves)
            {
                yield return StartCoroutine(ShowVictoryScreen());
                break;
            }
            
            // Show wave completion screen and wait for continue
            ShowWaveComplete();
            yield return new WaitUntil(() => !waitingForContinue);
            
            currentWave++;
        }
    }

    IEnumerator SpawnWave()
    {
        // Countdown
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        
        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }
        
        GameManager.Instance.state = GameManager.GameState.INWAVE;
        
        // Start spawning all enemy types simultaneously
        List<Coroutine> spawnCoroutines = new List<Coroutine>();
        
        foreach (var spawn in currentLevel.spawns)
        {
            var spawnCoroutine = StartCoroutine(SpawnEnemyType(spawn));
            spawnCoroutines.Add(spawnCoroutine);
        }
        
        // Wait for all spawn coroutines to complete
        foreach (var coroutine in spawnCoroutines)
        {
            yield return coroutine;
        }
    }

    IEnumerator SpawnEnemyType(SpawnData spawn)
    {
        if (!enemyLookup.ContainsKey(spawn.enemy))
        {
            Debug.LogError($"Enemy type '{spawn.enemy}' not found!");
            yield break;
        }

        var enemyTemplate = enemyLookup[spawn.enemy];
        
        // Calculate spawn parameters using RPN
        var variables = new Dictionary<string, int>
        {
            ["wave"] = currentWave,
            ["base"] = 0 // Will be set per property
        };

        // Calculate total count to spawn
        variables["base"] = 1; // Base count is 1
        int totalCount = RPNEvaluator.Evaluate(spawn.count, variables);
        
        // Calculate delay between spawns
        int delay = RPNEvaluator.Evaluate(spawn.delay, variables);
        
        // Calculate enemy stats
        variables["base"] = enemyTemplate.hp;
        int enemyHp = RPNEvaluator.Evaluate(spawn.hp, variables);
        
        variables["base"] = enemyTemplate.speed;
        int enemySpeed = RPNEvaluator.Evaluate(spawn.speed, variables);
        
        variables["base"] = enemyTemplate.damage;
        int enemyDamage = RPNEvaluator.Evaluate(spawn.damage, variables);

        // Spawn enemies according to sequence pattern
        int spawned = 0;
        int sequenceIndex = 0;
        
        while (spawned < totalCount)
        {
            int groupSize = spawn.sequence[sequenceIndex % spawn.sequence.Count];
            int actualGroupSize = Mathf.Min(groupSize, totalCount - spawned);
            
            // Spawn a group of enemies
            for (int i = 0; i < actualGroupSize; i++)
            {
                SpawnSingleEnemy(enemyTemplate, enemyHp, enemySpeed, enemyDamage, spawn.location);
                spawned++;
            }
            
            sequenceIndex++;
            
            // Wait for delay unless this is the last group
            if (spawned < totalCount)
            {
                yield return new WaitForSeconds(delay);
            }
        }
    }

    void SpawnSingleEnemy(EnemyData template, int hp, int speed, int damage, string location)
    {
        SpawnPoint spawnPoint = GetSpawnPoint(location);
        Vector2 offset = Random.insideUnitCircle * 1.8f;
        Vector3 initialPosition = spawnPoint.transform.position + new Vector3(offset.x, offset.y, 0);
        
        GameObject newEnemy = Instantiate(enemy, initialPosition, Quaternion.identity);
        
        // Set sprite
        newEnemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(template.sprite);
        
        // Configure enemy controller
        EnemyController enemyController = newEnemy.GetComponent<EnemyController>();
        enemyController.hp = new Hittable(hp, Hittable.Team.MONSTERS, newEnemy);
        enemyController.speed = speed;
        enemyController.damage = damage;
        
        GameManager.Instance.AddEnemy(newEnemy);
    }

    SpawnPoint GetSpawnPoint(string location)
    {
        if (location == "random")
        {
            return SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        }
        else if (location.StartsWith("random "))
        {
            string spawnType = location.Substring(7).ToUpper();
            var validSpawns = SpawnPoints.Where(sp => sp.kind.ToString() == spawnType).ToArray();
            
            if (validSpawns.Length > 0)
                return validSpawns[Random.Range(0, validSpawns.Length)];
            else
                return SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        }
        else
        {
            return SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        }
    }

    void ShowWaveComplete()
    {
        waitingForContinue = true;
        
        // Enable continue button and show wave info
        if (continueButton != null)
            continueButton.SetActive(true);
            
        if (waveInfoText != null)
        {
            string waveText = $"Wave {currentWave} Complete!\n";
            waveText += $"Enemies Defeated: {GetEnemiesDefeatedThisWave()}\n";
            waveText += $"Next Wave: {currentWave + 1}";
            
            if (currentLevel.waves > 0)
                waveText += $" / {currentLevel.waves}";
                
            waveInfoText.text = waveText;
            waveInfoText.gameObject.SetActive(true);
        }
    }

    int GetEnemiesDefeatedThisWave()
    {
        // Calculate total enemies for current wave
        int total = 0;
        var variables = new Dictionary<string, int>
        {
            ["wave"] = currentWave,
            ["base"] = 1
        };
        
        foreach (var spawn in currentLevel.spawns)
        {
            total += RPNEvaluator.Evaluate(spawn.count, variables);
        }
        
        return total;
    }

    IEnumerator ShowVictoryScreen()
    {
        if (levelInfoText != null)
        {
            levelInfoText.text = $"Victory!\nYou completed {currentLevel.name} mode!\nWaves Survived: {currentWave}";
            levelInfoText.gameObject.SetActive(true);
        }
        
        yield return new WaitForSeconds(3f);
        
        ReturnToMenu();
    }

    public void ShowGameOver()
    {
        gameInProgress = false;
        
        if (levelInfoText != null)
        {
            levelInfoText.text = $"Game Over!\nWaves Survived: {currentWave}\nLevel: {currentLevel.name}";
            levelInfoText.gameObject.SetActive(true);
        }
        
        StartCoroutine(GameOverDelay());
    }

    IEnumerator GameOverDelay()
    {
        yield return new WaitForSeconds(3f);
        ReturnToMenu();
    }

    void ReturnToMenu()
    {
        gameInProgress = false;
        waitingForContinue = false;
        currentWave = 0;
        currentLevel = null;
        
        GameManager.Instance.state = GameManager.GameState.PREGAME;
        
        // Hide info texts
        if (waveInfoText != null)
            waveInfoText.gameObject.SetActive(false);
        if (levelInfoText != null)
            levelInfoText.gameObject.SetActive(false);
        if (continueButton != null)
            continueButton.SetActive(false);
            
        // Show level selector
        level_selector.gameObject.SetActive(true);
    }

    // Called by continue button - this is what the UI calls
    public void NextWave()
    {
        Debug.Log("NextWave button pressed!");
        
        // Hide UI elements
        if (continueButton != null)
            continueButton.SetActive(false);
        if (waveInfoText != null)
            waveInfoText.gameObject.SetActive(false);
            
        // Signal that we can continue
        waitingForContinue = false;
    }
}
