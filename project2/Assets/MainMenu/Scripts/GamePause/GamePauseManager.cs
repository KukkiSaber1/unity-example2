using UnityEngine;
using UnityEngine.Events;

public class GamePauseManager : MonoBehaviour
{
    public bool isPaused { get; private set; }

    [Header("Events")]
    public UnityEvent OnGamePaused;   // Drag your Timeline.Play here
    public UnityEvent OnGameResumed;  // Hook up your resume logic here

    void Update()
    {
        // Detects 'Escape' on PC and 'Back' button on Android
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f; // Freezes physics/gameplay
        OnGamePaused.Invoke(); // Triggers your Timeline/UI events
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f; // Restores normal speed
        OnGameResumed.Invoke();
    }
}
