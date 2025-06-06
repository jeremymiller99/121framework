using UnityEngine;
using System;

public class ProjectileManager : MonoBehaviour
{
    public GameObject[] projectiles;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.projectileManager = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateProjectile(int which, string trajectory, Vector3 where, Vector3 direction, float speed, Action<Hittable,Vector3> onHit)
    {
        // Bounds checking to prevent IndexOutOfRangeException
        int spriteIndex = Mathf.Clamp(which, 0, projectiles.Length - 1);
        GameObject new_projectile = Instantiate(projectiles[spriteIndex], where + direction.normalized*1.1f, Quaternion.Euler(0,0,Mathf.Atan2(direction.y, direction.x)*Mathf.Rad2Deg));
        
        var controller = new_projectile.GetComponent<ProjectileController>();
        if (controller != null)
        {
            controller.movement = MakeMovement(trajectory, speed);
            controller.OnHit += onHit;
        }
        else
        {
            Debug.LogError($"Projectile prefab {projectiles[spriteIndex].name} is missing ProjectileController component!");
            Destroy(new_projectile);
        }
    }

    public void CreateProjectile(int which, string trajectory, Vector3 where, Vector3 direction, float speed, Action<Hittable, Vector3> onHit, float lifetime)
    {
        // Bounds checking to prevent IndexOutOfRangeException
        int spriteIndex = Mathf.Clamp(which, 0, projectiles.Length - 1);
        GameObject new_projectile = Instantiate(projectiles[spriteIndex], where + direction.normalized * 1.1f, Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg));
        
        var controller = new_projectile.GetComponent<ProjectileController>();
        if (controller != null)
        {
            controller.movement = MakeMovement(trajectory, speed);
            controller.OnHit += onHit;
            controller.SetLifetime(lifetime);
        }
        else
        {
            Debug.LogError($"Projectile prefab {projectiles[spriteIndex].name} is missing ProjectileController component!");
            Destroy(new_projectile);
        }
    }

    public ProjectileMovement MakeMovement(string name, float speed)
    {
        // Handle null or empty trajectory names
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("Trajectory name is null or empty. Using straight movement as fallback.");
            return new StraightProjectileMovement(speed);
        }
        
        string trajectory = name.ToLower().Trim();
        
        if (trajectory == "straight")
        {
            return new StraightProjectileMovement(speed);
        }
        if (trajectory == "homing")
        {
            return new HomingProjectileMovement(speed);
        }
        if (trajectory == "spiraling")
        {
            return new SpiralingProjectileMovement(speed);
        }
        
        // Unknown trajectory type - log error and return default straight movement
        Debug.LogError($"Unknown projectile trajectory type: '{name}'. Using straight movement as fallback.");
        return new StraightProjectileMovement(speed);
    }

}
