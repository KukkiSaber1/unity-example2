using UnityEngine;
using UnityEngine.Playables;

public class TimelineController : MonoBehaviour
{
    public PlayableDirector director;

    public void SkipTimeline()
    {
        if (director != null)
        {
            director.time = director.duration;
            director.Evaluate();
            director.Stop();
            Debug.Log("Timeline skipped!");
        }
    }
}