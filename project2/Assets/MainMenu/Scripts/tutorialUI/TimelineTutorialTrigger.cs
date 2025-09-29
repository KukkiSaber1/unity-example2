// TimelineTutorialTrigger.cs
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineTutorialTrigger : MonoBehaviour
{
    [Header("References")]
    public PlayableDirector timelineDirector;
    public TutorialPopup tutorialPopup;
    public double triggerTime = 5.0f; // Time in seconds when tutorial should appear

    private bool tutorialTriggered = false;

    void Update()
    {
        if (timelineDirector != null && tutorialPopup != null && !tutorialTriggered)
        {
            // Check if we've reached the trigger time
            if (timelineDirector.time >= triggerTime)
            {
                TriggerTutorial();
                tutorialTriggered = true;
            }
        }
    }

    private void TriggerTutorial()
    {
        timelineDirector.Pause();
        tutorialPopup.StartTutorial();
    }

    // Call this method to resume timeline after tutorial
    public void ResumeTimeline()
    {
        if (timelineDirector != null)
        {
            timelineDirector.Play();
        }
    }
}