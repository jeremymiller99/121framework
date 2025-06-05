using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileSettings", menuName = "Game/Projectile Settings")]
public class ProjectileSettings : ScriptableObject
{
    [Header("Speed Settings")]
    [Tooltip("Maximum speed before visual scaling kicks in")]
    public float maxVisualSpeed = 50f;
    
    [Tooltip("How much speeds above the cap are scaled down (0.0 = no scaling, 1.0 = full scaling)")]
    [Range(0f, 1f)]
    public float speedScalingFactor = 0.7f;
    
    [Header("Visual Enhancement Settings")]
    [Tooltip("Minimum speed to add trail effects")]
    public float trailSpeedThreshold = 30f;
    
    [Tooltip("Speed threshold for enhanced visibility effects")]
    public float enhancedVisibilityThreshold = 60f;
    
    [Header("Trail Settings")]
    public Color normalTrailColor = Color.cyan;
    public Color fastTrailColor = Color.yellow;
    public float normalTrailWidth = 0.3f;
    public float fastTrailWidth = 0.5f;
    public float normalTrailTime = 0.2f;
    public float fastTrailTime = 0.4f;
    
    private static ProjectileSettings _instance;
    public static ProjectileSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ProjectileSettings>("ProjectileSettings");
                if (_instance == null)
                {
                    // Create default settings if none exist
                    _instance = CreateInstance<ProjectileSettings>();
                }
            }
            return _instance;
        }
    }
    
    private void OnValidate()
    {
        // Update the static values when settings change in editor
        if (Application.isPlaying)
        {
            ProjectileMovement.MAX_VISUAL_SPEED = maxVisualSpeed;
            ProjectileMovement.SPEED_SCALING_FACTOR = speedScalingFactor;
        }
    }
} 