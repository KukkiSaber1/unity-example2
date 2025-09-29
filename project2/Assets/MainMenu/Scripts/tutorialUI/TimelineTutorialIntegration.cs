using UnityEngine;
using UnityEngine.Playables;

// Attach this to the same GameObject as your PlayableDirector
public class TimelineTutorialIntegration : MonoBehaviour
{
    [Header("Drag & Drop References")]
    public TutorialManager tutorialManager;
    public PlayableDirector director;

    void Start()
    {
        // Auto-get director if not assigned
        if (director == null)
        {
            director = GetComponent<PlayableDirector>();
        }

        // You can connect this to timeline signals
        if (director != null)
        {
            // Example: Pause at specific time and show tutorial
            // You can use this with timeline markers
        }
    }

    // Call this method from timeline via Animation Event or Signal
    public void ShowTutorialAtMarker()
    {
        if (director != null)
        {
            director.Pause();
        }

        if (tutorialManager != null)
        {
            tutorialManager.StartTutorialWithTimeline(director);
        }
    }
}