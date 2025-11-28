using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
#endif

public class MaxTimer : MonoBehaviour
{
    [Tooltip("Duration of the timer in seconds")]
    public float duration = 5f;

    [Tooltip("Event to invoke when the timer finishes")]
    public UnityEvent onTimerFinished;

    [Header("UI (drag components here)")]
    [Tooltip("Optional UnityEngine.UI.Text to show remaining time")]
    public Text uiText;

    #if TMP_PRESENT
    [Tooltip("Optional TextMeshProUGUI to show remaining time")]
    public TextMeshProUGUI tmpText;
    #endif

    [Tooltip("Optional Image used as a fill (set Image Type to Filled)")]
    public Image uiFillImage;

    [Tooltip("If true, UI will show time remaining; otherwise it shows progress 0-1")]
    public bool showTimeRemaining = true;

    [Tooltip("Time format used when showing remaining time (e.g. mm\\:ss or F1)")]
    public string timeFormat = "F1";

    private float timeRemaining;
    private bool isRunning;

    void OnEnable()
    {
        ResetTimer();
        StartTimer();
        UpdateUIImmediate();
    }

    void OnDisable()
    {
        isRunning = false;
    }

    void Update()
    {
        if (!isRunning) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isRunning = false;
            onTimerFinished?.Invoke();
        }

        UpdateUI();
    }

    public void StartTimer()
    {
        timeRemaining = duration;
        isRunning = true;
        UpdateUIImmediate();
    }

    public void ResetTimer()
    {
        timeRemaining = duration;
        isRunning = false;
        UpdateUIImmediate();
    }

    private void UpdateUI()
    {
        float progress = Mathf.Clamp01(1f - (timeRemaining / Mathf.Max(0.0001f, duration)));

        // Update fill image if assigned
        if (uiFillImage != null)
        {
            uiFillImage.fillAmount = showTimeRemaining ? (timeRemaining / Mathf.Max(0.0001f, duration)) : progress;
        }

        // Update Unity UI Text
        if (uiText != null)
        {
            uiText.text = showTimeRemaining ? FormatTime(timeRemaining) : progress.ToString("F2");
        }

        // Update TextMeshPro if present and assigned
        #if TMP_PRESENT
        if (tmpText != null)
        {
            tmpText.text = showTimeRemaining ? FormatTime(timeRemaining) : progress.ToString("F2");
        }
        #endif
    }

    // Force an immediate UI refresh (useful when starting/resetting)
    private void UpdateUIImmediate()
    {
        UpdateUI();
    }

    private string FormatTime(float t)
    {
        // If user provided a numeric format like "F1" use that, otherwise try mm:ss style
        if (timeFormat.StartsWith("F"))
            return t.ToString(timeFormat);
        else
        {
            int minutes = Mathf.FloorToInt(t / 60f);
            int seconds = Mathf.FloorToInt(t % 60f);
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}
