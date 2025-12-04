using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public MaxTimer timer;               // Drag your MaxTimer here
    public PlayerHealth playerHealth;    // Drag your PlayerHealth here

    [Header("Scoring weights")]
    public float timeWeight = 10f;
    public float healthWeight = 100f;

    [Header("UI (optional)")]
    public TextMeshProUGUI scoreText;

    [Header("Achievement")]
    public int achievementThreshold = 300;
    public UnityEvent<int> onScoreCalculated;
    public UnityEvent onAchievementUnlocked;

    public enum SceneOption { Scene1, Scene2, Scene3, Scene4 } // edit names as needed
    [Tooltip("Select which scene this score belongs to")]
    public SceneOption scene = SceneOption.Scene1;

    private int lastScore;

    // Call this to compute and save the score (hook to timer.onTimerFinished)
    public void CalculateAndPublishScore()
    {
        if (timer == null || playerHealth == null)
        {
            Debug.LogWarning("[ScoreManager] Missing references.");
            return;
        }

        float remainingSeconds = timer.TimeRemaining; // MaxTimer must expose TimeRemaining
        float healthPercent = playerHealth.currentHealth / playerHealth.maxHealth;

        float rawScore = (remainingSeconds * timeWeight) + (healthPercent * healthWeight);
        lastScore = Mathf.RoundToInt(rawScore);

        if (scoreText) scoreText.text = $"Score: {lastScore}";

        onScoreCalculated?.Invoke(lastScore);
        if (lastScore >= achievementThreshold) onAchievementUnlocked?.Invoke();

        // Save to persistent scoreboard using the selected scene name
        string sceneName = scene.ToString();
        PersistentScores.AddScore(sceneName, lastScore);
    }

    public int GetLastScore() => lastScore;
}
