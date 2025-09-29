using UnityEngine;
using UnityEngine.Playables;

// Simple component to trigger tutorial from inspector events
public class TutorialTrigger : MonoBehaviour
{
    [Header("Tutorial References")]
    public TutorialManager tutorialManager;
    public PlayableDirector timelineToPause;

    [Header("Trigger Settings")]
    public bool triggerOnStart = false;
    public float delay = 0f;

    void Start()
    {
        if (triggerOnStart)
        {
            if (delay > 0)
            {
                Invoke(nameof(TriggerTutorial), delay);
            }
            else
            {
                TriggerTutorial();
            }
        }
    }

    // Call this method from timeline events, button clicks, etc.
    public void TriggerTutorial()
    {
        if (tutorialManager != null)
        {
            if (timelineToPause != null)
            {
                tutorialManager.StartTutorialWithTimeline(timelineToPause);
            }
            else
            {
                tutorialManager.StartTutorial();
            }
        }
        else
        {
            Debug.LogWarning("TutorialManager reference is missing!", this);
        }
    }

    // Editor method to quickly setup references
    [ContextMenu("Auto-Find Tutorial Manager")]
    void AutoFindTutorialManager()
    {
        if (tutorialManager == null)
        {
            tutorialManager = FindObjectOfType<TutorialManager>();
            if (tutorialManager != null)
            {
                Debug.Log("Found TutorialManager: " + tutorialManager.name);
            }
            else
            {
                Debug.LogWarning("No TutorialManager found in scene!");
            }
        }
    }
}