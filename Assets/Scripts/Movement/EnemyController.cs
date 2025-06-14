using UnityEngine;

public class EnemyController : MonoBehaviour
{

    public Transform target;
    public int speed;
    public int damage = 5; // Configurable damage
    public Hittable hp;
    public HealthBar healthui;
    public bool dead;

    public float last_attack;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        target = GameManager.Instance.player.transform;
        hp.OnDeath += Die;
        healthui.SetHealth(hp);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = target.position - transform.position;
        if (direction.magnitude < 2f)
        {
            DoAttack();
        }
        else
        {
            GetComponent<Unit>().movement = direction.normalized * speed;
        }
    }
    
    void DoAttack()
    {
        if (last_attack + 2 < Time.time)
        {
            last_attack = Time.time;
            target.gameObject.GetComponent<PlayerController>().hp.Damage(new Damage(damage, Damage.Type.PHYSICAL));
        }
    }


    void Die()
    {
        if (!dead)
        {
            dead = true;
            
            // Fire enemy killed event for relic system
            var player = GameManager.Instance?.player?.GetComponent<PlayerController>();
            if (player?.hp != null)
            {
                EventBus.Instance.FireEnemyKilled(player.hp, hp);
            }
            
            GameManager.Instance.RemoveEnemy(gameObject);
            Destroy(gameObject);
        }
    }
}
