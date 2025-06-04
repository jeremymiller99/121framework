using UnityEngine;
using TMPro;

public class WaveLabelController : MonoBehaviour
{
    TextMeshProUGUI tmp;
    
    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        UpdateDisplay(); // Initialize display immediately
    }

    void OnEnable()
    {
        // Reset display when re-enabled (important for restart functionality)
        if (tmp != null)
            UpdateDisplay();
    }

    void Update()
    {
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (tmp == null) return;

        switch (GameManager.Instance.state)
        {
            case GameManager.GameState.PREGAME:
                tmp.text = "Select a Level";
                break;
                
            case GameManager.GameState.COUNTDOWN:
                tmp.text = "Starting in " + GameManager.Instance.countdown;
                break;
                
            case GameManager.GameState.INWAVE:
                tmp.text = "Enemies left: " + GameManager.Instance.enemy_count;
                break;
                
            case GameManager.GameState.WAVEEND:
                tmp.text = "Wave Complete!";
                break;
                
            case GameManager.GameState.GAMEOVER:
                tmp.text = "Game Over";
                break;
                
            default:
                tmp.text = "";
                break;
        }
    }
}
