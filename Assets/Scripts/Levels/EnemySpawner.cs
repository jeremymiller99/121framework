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
    public GameObject restartButton;
    public TextMeshProUGUI waveInfoText;
    public TextMeshProUGUI levelInfoText;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    
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
        InitializeUI();
    }

    void InitializeUI()
    {
        // Hide game over and victory panels initially
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
        if (restartButton != null)
            restartButton.SetActive(false);
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
        Debug.Log($"Starting level: {levelname}");
        
        currentLevel = levels.FirstOrDefault(l => l.name == levelname);
        if (currentLevel == null)
        {
            Debug.LogError($"Level '{levelname}' not found!");
            return;
        }

        // Reset all game state
        currentWave = 1;
        gameInProgress = true;
        waitingForContinue = false;
        
        // Hide level selector and all game UI
        level_selector.gameObject.SetActive(false);
        HideAllGameUI();
        
        // Reset GameManager state
        GameManager.Instance.state = GameManager.GameState.PREGAME;
        
        // Make sure wave UI is visible and reset
        InitializeGameUI();
        
        // Start the player
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
        
        Debug.Log($"Starting level coroutine for {levelname}, wave 1");
        StartCoroutine(RunLevel());
    }

    void InitializeGameUI()
    {
        Debug.Log("Initializing game UI for new run");
        
        // Make sure wave info text is active and ready
        if (waveInfoText != null)
        {
            waveInfoText.gameObject.SetActive(true);
            waveInfoText.text = "Preparing...";
        }
        
        // Hide other UI elements that shouldn't be shown at start
        if (levelInfoText != null)
            levelInfoText.gameObject.SetActive(false);
        if (continueButton != null)
            continueButton.SetActive(false);
        if (restartButton != null)
            restartButton.SetActive(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    IEnumerator RunLevel()
    {
        Debug.Log($"RunLevel started for {currentLevel.name}");
        
        while (gameInProgress)
        {
            Debug.Log($"Starting wave {currentWave}");
            yield return StartCoroutine(SpawnWave());
            
            // Wait for all enemies to be defeated
            yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
            
            GameManager.Instance.state = GameManager.GameState.WAVEEND;
            Debug.Log($"Wave {currentWave} completed");
            
            // Check win condition for Easy and Medium modes
            if (currentLevel.waves > 0 && currentWave >= currentLevel.waves)
            {
                Debug.Log($"Level {currentLevel.name} completed!");
                ShowVictoryScreen();
                yield break; // Exit the level loop
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
        
        // Enable continue button
        if (continueButton != null)
            continueButton.SetActive(true);
            
        // Show wave completion info in levelInfoText (NOT waveInfoText)
        if (levelInfoText != null)
        {
            string waveText = $"Wave {currentWave} Complete!\n";
            waveText += $"Enemies Defeated: {GetEnemiesDefeatedThisWave()}\n";
            waveText += $"Next Wave: {currentWave + 1}";
            
            if (currentLevel.waves > 0)
                waveText += $" / {currentLevel.waves}";
            else
                waveText += " (Endless)";
                
            levelInfoText.text = waveText;
            levelInfoText.gameObject.SetActive(true);
        }
        
        // Keep waveInfoText active but show wave end status
        if (waveInfoText != null)
        {
            waveInfoText.text = "Wave Complete - Click Continue";
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

    public void ShowGameOver()
    {
        Debug.Log("ShowGameOver() called!");
        
        gameInProgress = false;
        GameManager.Instance.state = GameManager.GameState.GAMEOVER;
        
        string gameOverMessage = GetGameOverMessage();
        Debug.Log($"Game Over Message: {gameOverMessage}");

        // Show game over message
        if (levelInfoText != null)
        {
            levelInfoText.text = gameOverMessage;
            levelInfoText.gameObject.SetActive(true);
        }

        // Show restart button
        if (restartButton != null)
        {
            restartButton.SetActive(true);
        }

        // Show game over panel if available
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    void ShowVictoryScreen()
    {
        gameInProgress = false;
        GameManager.Instance.state = GameManager.GameState.GAMEOVER;
        
        string victoryMessage = GetVictoryMessage();

        // Show victory message
        if (levelInfoText != null)
        {
            levelInfoText.text = victoryMessage;
            levelInfoText.gameObject.SetActive(true);
        }

        // Show restart button
        if (restartButton != null)
        {
            restartButton.SetActive(true);
        }

        // Show victory panel if available
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
    }

    string GetGameOverMessage()
    {
        if (currentLevel == null)
            return "💀 GAME OVER 💀\n\nTry again!";
            
        if (currentLevel.name == "Easy")
        {
            return $"💀 GAME OVER 💀\n\n" +
                   $"Waves Survived: {currentWave} / {currentLevel.waves}\n\n" +
                   $"Keep trying! You'll get better each time.";
        }
        else if (currentLevel.name == "Medium")
        {
            return $"💀 DEFEATED 💀\n\n" +
                   $"Waves Survived: {currentWave} / {currentLevel.waves}\n\n" +
                   $"Medium mode is tough! Try again?";
        }
        else // Endless
        {
            return $"💀 ENDLESS WARRIOR FALLEN 💀\n\n" +
                   $"Waves Survived: {currentWave}\n\n" +
                   $"Can you survive even longer?";
        }
    }

    string GetVictoryMessage()
    {
        if (currentLevel.name == "Easy")
        {
            return $"🎉 VICTORY! 🎉\n\n" +
                   $"You completed Easy Mode!\n" +
                   $"Waves: {currentWave} / {currentLevel.waves}\n\n" +
                   $"Try Medium or Endless mode!";
        }
        else if (currentLevel.name == "Medium")
        {
            return $"🏆 AMAZING! 🏆\n\n" +
                   $"You mastered Medium Mode!\n" +
                   $"Waves: {currentWave} / {currentLevel.waves}\n\n" +
                   $"Ready for Endless mode?";
        }
        return "🎉 VICTORY! 🎉";
    }

    public void RestartGame()
    {
        Debug.Log("RestartGame() called - performing complete reset");
        
        // Stop all coroutines to prevent any lingering processes
        StopAllCoroutines();
        
        // Clear all enemies from the game
        var allEnemies = FindObjectsOfType<EnemyController>();
        foreach (var enemy in allEnemies)
        {
            if (enemy != null)
            {
                GameManager.Instance.RemoveEnemy(enemy.gameObject);
                Destroy(enemy.gameObject);
            }
        }
        
        // Reset ALL game state
        gameInProgress = false;
        waitingForContinue = false;
        currentWave = 0;
        currentLevel = null;
        
        // Reset GameManager state properly
        GameManager.Instance.state = GameManager.GameState.PREGAME;
        GameManager.Instance.countdown = 0;
        
        // Hide ALL UI elements immediately
        HideAllGameUI();
        
        // Reset any UI controllers that might be cached
        ResetUIControllers();
        
        // Show level selector immediately
        level_selector.gameObject.SetActive(true);
        
        Debug.Log("Complete restart finished - ready for new game");
    }

    void ResetUIControllers()
    {
        // Find and reset any UI controllers that might have cached state
        var waveLabels = FindObjectsOfType<WaveLabelController>();
        foreach (var waveLabel in waveLabels)
        {
            if (waveLabel != null)
            {
                // Force the wave label to refresh by disabling and re-enabling
                waveLabel.gameObject.SetActive(false);
                waveLabel.gameObject.SetActive(true);
            }
        }
        
        Debug.Log($"Reset {waveLabels.Length} wave label controllers");
    }

    void HideAllGameUI()
    {
        Debug.Log("Hiding all game UI elements");
        
        // Only hide waveInfoText when completely exiting the game
        if (waveInfoText != null)
            waveInfoText.gameObject.SetActive(false);
        if (levelInfoText != null)
            levelInfoText.gameObject.SetActive(false);
        if (continueButton != null)
            continueButton.SetActive(false);
        if (restartButton != null)
            restartButton.SetActive(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    void ReturnToMenu()
    {
        // This method now just calls RestartGame for simplicity
        RestartGame();
    }

    // Called by continue button - this is what the UI calls
    public void NextWave()
    {
        Debug.Log("NextWave button pressed!");
        
        // Hide continue button and wave completion message
        if (continueButton != null)
            continueButton.SetActive(false);
        if (levelInfoText != null)
            levelInfoText.gameObject.SetActive(false);
            
        // Keep waveInfoText active and update it
        if (waveInfoText != null)
        {
            waveInfoText.text = "Preparing next wave...";
            // DON'T hide waveInfoText - it needs to stay active for countdown and enemy count
        }
            
        // Signal that we can continue
        waitingForContinue = false;
    }
}
