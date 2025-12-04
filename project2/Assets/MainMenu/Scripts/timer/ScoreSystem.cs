using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class ScoreSystem : MonoBehaviour
{
    [Header("References")]
    public MaxTimer timer;           // Drag your MiniTimer here
    public PlayerHealth playerHealth; // Drag your PlayerHealth here

    [Header("Scoring")]
    [Tooltip("Multiplier for remaining seconds from the timer")]
    public float timeWeight = 10f;
    [Tooltip("Multiplier for health percentage (0..1)")]
    public float healthWeight = 100f;

    [Header("UI (optional)")]
    public TextMeshProUGUI scoreText;

    [Header("Events")]
    [Tooltip("Invoked when score is calculated (e.g., on finish)")]
    public UnityEvent<int> onScoreCalculated;
    [Tooltip("Invoked if score meets or exceeds achievementThreshold")]
    public UnityEvent onAchievementUnlocked;

    [Header("Achievement")]
    [Tooltip("Score required to unlock achievement")]
    public int achievementThreshold = 300;

    private int lastScore;

    // Call this whenever you want (e.g., timer finishes, level ends, button press)
    public void CalculateAndPublishScore()
    {
        if (timer == null || playerHealth == null)
        {
            Debug.LogWarning("[ScoreManager] Missing references.");
            return;
        }

        float remainingSeconds = timer.TimeRemaining;                // from MiniTimer
        float healthPercent = playerHealth.currentHealth / playerHealth.maxHealth;

        float rawScore = (remainingSeconds * timeWeight) + (healthPercent * healthWeight);
        lastScore = Mathf.RoundToInt(rawScore);

        if (scoreText) scoreText.text = $"Score: {lastScore}";

        onScoreCalculated?.Invoke(lastScore);
        if (lastScore >= achievementThreshold) onAchievementUnlocked?.Invoke();
    }

    // Optional: expose last score for other scripts/UI
    public int GetLastScore() => lastScore;
}
