using UnityEngine;
using UnityEngine.Events;

public class MiniTimer : MonoBehaviour
{
    [Tooltip("Duration of the timer in seconds")]
    public float duration = 5f;

    [Tooltip("Event to invoke when the timer finishes")]
    public UnityEvent onTimerFinished;

    private float timeRemaining;
    private bool isRunning;

    void OnEnable()
    {
        // Reset timer when the object becomes active
        ResetTimer();
        StartTimer();
    }

    void OnDisable()
    {
        // Stop timer when object is disabled
        isRunning = false;
    }

    void Update()
    {
        if (!isRunning) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            isRunning = false;
            onTimerFinished?.Invoke();
        }
    }

    public void StartTimer()
    {
        timeRemaining = duration;
        isRunning = true;
    }

    public void ResetTimer()
    {
        timeRemaining = duration;
        isRunning = false;
    }
}
