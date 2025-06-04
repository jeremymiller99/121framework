using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WaveUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI waveStatusText;
    public TextMeshProUGUI levelNameText;
    public Button continueButton;
    public GameObject waveCompletePanel;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    private void Start()
    {
        // Initialize UI state
        if (waveCompletePanel != null)
            waveCompletePanel.SetActive(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    private void Update()
    {
        UpdateWaveStatus();
    }

    private void UpdateWaveStatus()
    {
        if (waveStatusText == null) return;

        switch (GameManager.Instance.state)
        {
            case GameManager.GameState.PREGAME:
                waveStatusText.text = "Select a Level";
                break;
            case GameManager.GameState.COUNTDOWN:
                waveStatusText.text = $"Starting in {GameManager.Instance.countdown}";
                break;
            case GameManager.GameState.INWAVE:
                waveStatusText.text = $"Enemies left: {GameManager.Instance.enemy_count}";
                break;
            case GameManager.GameState.WAVEEND:
                waveStatusText.text = "Wave Complete!";
                break;
            case GameManager.GameState.GAMEOVER:
                waveStatusText.text = "Game Over";
                break;
        }
    }

    public void ShowWaveComplete(int wave, int totalWaves)
    {
        if (waveCompletePanel != null)
        {
            waveCompletePanel.SetActive(true);
            
            var waveText = waveCompletePanel.GetComponentInChildren<TextMeshProUGUI>();
            if (waveText != null)
            {
                string text = $"Wave {wave} Complete!\n";
                if (totalWaves > 0)
                    text += $"Progress: {wave}/{totalWaves}";
                else
                    text += "Endless Mode";
                    
                waveText.text = text;
            }
        }
    }

    public void HideWaveComplete()
    {
        if (waveCompletePanel != null)
            waveCompletePanel.SetActive(false);
    }

    public void ShowGameOver(int wavesSurvived, string levelName)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            var gameOverText = gameOverPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (gameOverText != null)
            {
                gameOverText.text = $"Game Over!\nWaves Survived: {wavesSurvived}\nLevel: {levelName}";
            }
        }
    }

    public void ShowVictory(string levelName, int wavesSurvived)
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            
            var victoryText = victoryPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (victoryText != null)
            {
                victoryText.text = $"Victory!\nCompleted: {levelName}\nWaves: {wavesSurvived}";
            }
        }
    }

    public void HideAllPanels()
    {
        if (waveCompletePanel != null)
            waveCompletePanel.SetActive(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }
} 