using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public Hittable hp;
    public HealthBar healthui;
    public ManaBar manaui;

    public SpellCaster spellcaster;
    public SpellUIContainer spellUIContainer;

    public int speed;
    public int currentWave = 1;

    public Unit unit;
    private bool isDead = false; // Prevent multiple death calls
    private Vector3 lastPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unit = GetComponent<Unit>();
        GameManager.Instance.player = gameObject;
        lastPosition = transform.position;
    }

    public void StartLevel()
    {
        Debug.Log("PlayerController StartLevel() called");
        
        isDead = false; // Reset death state when starting a new level
        
        // Calculate player stats based on selected character class
        CharacterClass selectedClass = CharacterClassManager.Instance?.GetSelectedClass();
        int playerHp, playerMana, playerManaReg, playerSpellPower;
        
        if (selectedClass == null)
        {
            Debug.LogWarning("No character class selected, using default mage stats");
            // Fall back to hardcoded mage stats
            var variables = new Dictionary<string, int> { ["wave"] = currentWave };
            playerHp = RPNEvaluator.Evaluate("95 wave 12 * + wave wave * 2 * +", variables);
            playerMana = RPNEvaluator.Evaluate("90 wave 15 * + wave wave * 3 * +", variables);
            playerManaReg = RPNEvaluator.Evaluate("10 wave 2 * + wave 10 / +", variables);
            playerSpellPower = RPNEvaluator.Evaluate("wave 12 *", variables);
            speed = RPNEvaluator.Evaluate("5 wave 15 / +", variables);
        }
        else
        {
            playerHp = selectedClass.CalculateHealth(currentWave);
            playerMana = selectedClass.CalculateMana(currentWave);
            playerManaReg = selectedClass.CalculateManaRegeneration(currentWave);
            playerSpellPower = selectedClass.CalculateSpellPower(currentWave);
            speed = selectedClass.CalculateSpeed(currentWave);
        }
        
        // Create or update spell caster
        if (spellcaster == null)
        {
            spellcaster = new SpellCaster(playerMana, playerManaReg, Hittable.Team.PLAYER);
            StartCoroutine(spellcaster.ManaRegeneration());
        }
        else
        {
            // Update existing spell caster with full mana
            spellcaster.max_mana = playerMana;
            spellcaster.mana = playerMana; // Reset to full mana instead of preserving current
            spellcaster.mana_reg = playerManaReg;
        }
        
        // Update spell power and wave for all spells
        spellcaster.UpdateSpellPowerAndWave(playerSpellPower, currentWave);
        
        // Create or update hittable
        if (hp == null)
        {
            hp = new Hittable(playerHp, Hittable.Team.PLAYER, gameObject);
            hp.OnDeath += Die;
            hp.team = Hittable.Team.PLAYER;
        }
        else
        {
            // Reset to full HP instead of preserving HP percentage
            hp.max_hp = playerHp;
            hp.hp = playerHp; // Set to full health
        }

        // tell UI elements what to show - ensure they're active first
        if (healthui != null)
        {
            healthui.gameObject.SetActive(true);
            healthui.SetHealth(hp);
        }
        
        if (manaui != null)
        {
            manaui.gameObject.SetActive(true);
            manaui.SetSpellCaster(spellcaster);
        }
        
        if (spellUIContainer != null)
        {
            spellUIContainer.gameObject.SetActive(true);
            spellUIContainer.SetSpellCaster(spellcaster);
        }
        
        Debug.Log($"PlayerController initialization complete - Wave {currentWave}, HP: {playerHp}, Mana: {playerMana}, Spell Power: {playerSpellPower}");
    }

    public void SetWave(int wave)
    {
        currentWave = wave;
        if (spellcaster != null)
        {
            // Recalculate stats for new wave
            StartLevel();
        }
    }

    public void ResetPlayer()
    {
        Debug.Log("PlayerController ResetPlayer() called - performing complete reset");
        
        // Reset death state
        isDead = false;
        
        // Reset wave to 1
        currentWave = 1;
        
        // Clear spell caster state
        if (spellcaster != null)
        {
            // Stop mana regeneration coroutine if it's running
            StopAllCoroutines(); // This will stop all coroutines on this MonoBehaviour
            spellcaster = null;
        }
        
        // Clear hittable state
        if (hp != null)
        {
            hp.OnDeath -= Die; // Remove event handler to prevent duplicate subscriptions
            hp = null;
        }
        
        // Clear all relics
        if (RelicManager.Instance != null)
        {
            RelicManager.Instance.ClearAllRelics();
        }
        
        // Reset position to starting position (0,0,0)
        transform.position = Vector3.zero;
        lastPosition = Vector3.zero;
        
        // Reset movement
        if (unit != null)
        {
            unit.movement = Vector2.zero;
        }
        
        Debug.Log("Player reset complete - ready for new game");
    }

    public void ShowSpellReward(Spell rewardSpell)
    {
        // This would show the reward UI - for now just add the spell
        if (spellcaster != null)
        {
            spellcaster.AddSpell(rewardSpell);
            Debug.Log($"Player received spell: {rewardSpell.GetName()}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Track player movement for relic system
        if (Vector3.Distance(transform.position, lastPosition) > 0.1f)
        {
            EventBus.Instance.FirePlayerMove(transform.position);
            lastPosition = transform.position;
        }
    }

    void OnAttack(InputValue value)
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME || GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;
        Vector2 mouseScreen = Mouse.current.position.value;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0;
        
        // SpellCaster.Cast() will handle firing the spell cast event after applying relic modifiers
        StartCoroutine(spellcaster.Cast(transform.position, mouseWorld));
    }

    void OnMove(InputValue value)
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME || GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;
        unit.movement = value.Get<Vector2>()*speed;
    }

    void OnSpell1(InputValue value)
    {
        if (spellcaster != null) 
        {
            spellcaster.SelectSpell(0);
            Debug.Log("Key 1 pressed - Selected spell slot 1");
        }
    }

    void OnSpell2(InputValue value)
    {
        if (spellcaster != null) 
        {
            spellcaster.SelectSpell(1);
            Debug.Log("Key 2 pressed - Selected spell slot 2");
        }
    }

    void OnSpell3(InputValue value)
    {
        if (spellcaster != null) 
        {
            spellcaster.SelectSpell(2);
            Debug.Log("Key 3 pressed - Selected spell slot 3");
        }
    }

    void OnSpell4(InputValue value)
    {
        if (spellcaster != null) 
        {
            spellcaster.SelectSpell(3);
            Debug.Log("Key 4 pressed - Selected spell slot 4");
        }
    }

    void Die()
    {
        // Prevent multiple death calls
        if (isDead) return;
        isDead = true;
        
        Debug.Log("Player Die() method called - You Lost");
        GameManager.Instance.state = GameManager.GameState.GAMEOVER;
        
        // Find and notify the EnemySpawner
        var enemySpawner = FindObjectOfType<EnemySpawner>();
        if (enemySpawner != null)
        {
            Debug.Log("EnemySpawner found, calling ShowGameOver()");
            enemySpawner.ShowGameOver();
        }
        else
        {
            Debug.LogError("EnemySpawner not found! Cannot show game over screen.");
        }
    }
}
