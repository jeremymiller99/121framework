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
    public SpellUI spellui;

    public int speed;

    public Unit unit;
    private bool isDead = false; // Prevent multiple death calls

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unit = GetComponent<Unit>();
        GameManager.Instance.player = gameObject;
    }

    public void StartLevel()
    {
        Debug.Log("PlayerController StartLevel() called");
        
        isDead = false; // Reset death state when starting a new level
        
        spellcaster = new SpellCaster(125, 8, Hittable.Team.PLAYER);
        StartCoroutine(spellcaster.ManaRegeneration());
        
        hp = new Hittable(100, Hittable.Team.PLAYER, gameObject);
        hp.OnDeath += Die;
        hp.team = Hittable.Team.PLAYER;

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
        
        if (spellui != null)
        {
            spellui.gameObject.SetActive(true);
            spellui.SetSpell(spellcaster.spell);
        }
        
        Debug.Log("PlayerController initialization complete");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnAttack(InputValue value)
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME || GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;
        Vector2 mouseScreen = Mouse.current.position.value;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0;
        StartCoroutine(spellcaster.Cast(transform.position, mouseWorld));
    }

    void OnMove(InputValue value)
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME || GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;
        unit.movement = value.Get<Vector2>()*speed;
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
