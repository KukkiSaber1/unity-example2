using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

public class MaxTimer : MonoBehaviour
{
    [Tooltip("Duration of the timer in seconds")]
    public float duration = 5f;

    [Header("Events")]
    [Tooltip("Invoked when the timer finishes")]
    public UnityEvent onTimerFinished;
    [Tooltip("Invoked when the timer is paused")]
    public UnityEvent onTimerPaused;
    [Tooltip("Invoked when the timer is resumed")]
    public UnityEvent onTimerResumed;

    [Header("UI (drag here)")]
    public TextMeshProUGUI tmpText;
    public Text uiText;
    [Tooltip("Optional Image used as a fill (set Image Type to Filled)")]
    public Image uiFillImage;

    [Header("Behavior")]
    [Tooltip("Start the timer automatically when the GameObject is enabled")]
    public bool startOnEnable = true;
    [Tooltip("Use unscaled time (ignores Time.timeScale)")]
    public bool useUnscaledTime = false;
    [Tooltip("Show numeric time remaining instead of progress 0-1")]
    public bool showTimeRemaining = true;
    [Tooltip("Numeric format when showing remaining time, e.g. F1")]
    public string timeFormat = "F1";
    [Tooltip("Enable debug logs")]
    public bool debugLogs = false;

    private float timeRemaining;
    private bool isRunning;
    private bool isPaused;

    public bool IsRunning => isRunning;
    public bool IsPaused => isPaused;
    public float TimeRemaining => Mathf.Max(0f, timeRemaining);
    public float NormalizedRemaining => duration > 0f ? Mathf.Clamp01(timeRemaining / duration) : 0f;
    public float Progress => 1f - NormalizedRemaining;

    void OnEnable()
    {
        if (duration <= 0f) duration = 1f;
        ResetTimer();
        if (startOnEnable) StartTimer();
        UpdateUIImmediate();
        if (debugLogs) Debug.Log($"[MiniTimer] Enabled. startOnEnable={startOnEnable}, duration={duration}");
    }

    void OnDisable()
    {
        isRunning = false;
        isPaused = false;
        if (debugLogs) Debug.Log("[MiniTimer] Disabled");
    }

    void Update()
    {
        if (!isRunning || isPaused) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        timeRemaining -= dt;
        if (debugLogs) Debug.Log($"[MiniTimer] timeRemaining={timeRemaining:F2} dt={dt:F4}");

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isRunning = false;
            if (debugLogs) Debug.Log("[MiniTimer] Finished");
            onTimerFinished?.Invoke();
        }

        UpdateUI();
    }

    [ContextMenu("Start Timer")]
    public void StartTimer()
    {
        timeRemaining = duration;
        isRunning = true;
        isPaused = false;
        if (debugLogs) Debug.Log("[MiniTimer] StartTimer");
        UpdateUIImmediate();
    }

    [ContextMenu("Reset Timer")]
    public void ResetTimer()
    {
        timeRemaining = duration;
        isRunning = false;
        isPaused = false;
        if (debugLogs) Debug.Log("[MiniTimer] ResetTimer");
        UpdateUIImmediate();
    }

    [ContextMenu("Pause Timer")]
    public void PauseTimer()
    {
        if (!isRunning || isPaused) return;
        isPaused = true;
        if (debugLogs) Debug.Log("[MiniTimer] Paused");
        onTimerPaused?.Invoke();
    }

    [ContextMenu("Resume Timer")]
    public void ResumeTimer()
    {
        if (!isRunning || !isPaused) return;
        isPaused = false;
        if (debugLogs) Debug.Log("[MiniTimer] Resumed");
        onTimerResumed?.Invoke();
    }

    [ContextMenu("Toggle Pause")]
    public void TogglePause()
    {
        if (!isRunning) return;
        if (isPaused) ResumeTimer();
        else PauseTimer();
    }

    [ContextMenu("Restart Timer")]
    public void RestartTimer()
    {
        ResetTimer();
        StartTimer();
    }

    private void UpdateUI()
    {
        float normalized = NormalizedRemaining;
        float progress = Progress;

        if (uiFillImage != null)
            uiFillImage.fillAmount = showTimeRemaining ? normalized : progress;

        string display = showTimeRemaining ? FormatTime(TimeRemaining) : progress.ToString("F2");

        if (tmpText != null) tmpText.text = display;
        if (uiText != null) uiText.text = display;
    }

    private void UpdateUIImmediate() => UpdateUI();

    private string FormatTime(float t)
    {
        if (t < 0f) t = 0f;
        if (!string.IsNullOrEmpty(timeFormat) && timeFormat.StartsWith("F"))
            return t.ToString(timeFormat);
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}
