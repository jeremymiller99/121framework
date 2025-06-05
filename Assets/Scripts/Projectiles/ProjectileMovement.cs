using UnityEngine;

public class ProjectileMovement
{
    public float speed;
    public float displaySpeed; // Speed used for visual movement (capped)
    public static float MAX_VISUAL_SPEED = 50f; // Maximum speed for visibility
    public static float SPEED_SCALING_FACTOR = 0.7f; // How much high speeds are scaled down
    
    static bool settingsInitialized = false;

    public ProjectileMovement(float speed)
    {
        this.speed = speed;
        
        // Initialize settings from ProjectileSettings if not already done
        if (!settingsInitialized)
        {
            var settings = ProjectileSettings.Instance;
            MAX_VISUAL_SPEED = settings.maxVisualSpeed;
            SPEED_SCALING_FACTOR = settings.speedScalingFactor;
            settingsInitialized = true;
        }
        
        // Use a more sophisticated speed cap that still provides some benefit from high speeds
        // but prevents complete invisibility
        if (speed <= MAX_VISUAL_SPEED)
        {
            this.displaySpeed = speed;
        }
        else
        {
            // For speeds above the cap, apply diminishing returns
            float excessSpeed = speed - MAX_VISUAL_SPEED;
            this.displaySpeed = MAX_VISUAL_SPEED + (excessSpeed * SPEED_SCALING_FACTOR);
        }
    }

    public virtual void Movement(Transform transform)
    {
        
    }
}
