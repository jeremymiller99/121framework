using UnityEngine;
using System;
using System.Collections;

public class ProjectileController : MonoBehaviour
{
    public float lifetime;
    public event Action<Hittable,Vector3> OnHit;
    public ProjectileMovement movement;
    
    // Visual enhancement for fast projectiles
    private TrailRenderer trail;
    private SpriteRenderer spriteRenderer;
    private float originalSpeed;
    
    [Header("Debug")]
    public bool showSpeedDebug = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Setup visual enhancements based on projectile speed
        if (movement != null)
        {
            originalSpeed = movement.speed;
            SetupVisualEnhancements();
            
            // Debug output for speed capping
            if (showSpeedDebug && originalSpeed != movement.displaySpeed)
            {
                Debug.Log($"Projectile speed capped: {originalSpeed:F1} -> {movement.displaySpeed:F1} (Reduction: {((originalSpeed - movement.displaySpeed) / originalSpeed * 100):F1}%)");
            }
        }
        else
        {
            Debug.LogError($"ProjectileController on {gameObject.name} has no ProjectileMovement component assigned! This projectile will be destroyed.");
            // Set a short lifetime to destroy this broken projectile
            SetLifetime(0.1f);
        }
    }

    void SetupVisualEnhancements()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        var settings = ProjectileSettings.Instance;
        
        // Add trail for fast projectiles
        if (originalSpeed > settings.trailSpeedThreshold)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = settings.normalTrailColor;
            trail.endColor = new Color(settings.normalTrailColor.r, settings.normalTrailColor.g, settings.normalTrailColor.b, 0f);
            trail.startWidth = settings.normalTrailWidth;
            trail.endWidth = settings.normalTrailWidth * 0.3f;
            trail.time = settings.normalTrailTime;
            trail.minVertexDistance = 0.1f;
            
            // Make very fast projectiles more visible
            if (originalSpeed > settings.enhancedVisibilityThreshold)
            {
                // Increase sprite brightness/alpha for extremely fast projectiles
                if (spriteRenderer != null)
                {
                    Color spriteColor = spriteRenderer.color;
                    spriteColor.a = Mathf.Min(1.5f, spriteColor.a * 1.5f);
                    spriteRenderer.color = spriteColor;
                }
                
                // Longer, brighter trail
                trail.startWidth = settings.fastTrailWidth;
                trail.endWidth = settings.fastTrailWidth * 0.4f;
                trail.time = settings.fastTrailTime;
                trail.startColor = settings.fastTrailColor;
                trail.endColor = new Color(settings.fastTrailColor.r, settings.fastTrailColor.g, settings.fastTrailColor.b, 0f);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (movement != null)
        {
            movement.Movement(transform);
        }
        else
        {
            // If no movement component, projectile should be destroyed or given default behavior
            Debug.LogWarning($"ProjectileController on {gameObject.name} has null movement component!");
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("projectile")) return;
        if (collision.gameObject.CompareTag("unit"))
        {
            var ec = collision.gameObject.GetComponent<EnemyController>();
            if (ec != null)
            {
                OnHit(ec.hp, transform.position);
            }
            else
            {
                var pc = collision.gameObject.GetComponent<PlayerController>();
                if (pc != null)
                {
                    OnHit(pc.hp, transform.position);
                }
            }

        }
        Destroy(gameObject);
    }

    public void SetLifetime(float lifetime)
    {
        StartCoroutine(Expire(lifetime));
    }

    IEnumerator Expire(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
}
