using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleTimelineSync : MonoBehaviour
{
    public PlayableDirector director; // assign in inspector or find at Awake
    ParticleSystem ps;

    enum PSState { Playing, Paused, Stopped }
    PSState lastState = PSState.Stopped;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        if (director == null) director = GetComponent<PlayableDirector>();
    }

    void Update()
    {
        PSState current = GetParticleState();
        if (current == lastState) return;

        switch (current)
        {
            case PSState.Playing:
                // If director was stopped, start from 0; if paused, Play resumes from current time
                director.Play();
                break;
            case PSState.Paused:
                director.Pause();
                break;
            case PSState.Stopped:
                director.Stop();
                break;
        }

        lastState = current;
    }

    PSState GetParticleState()
    {
        if (ps.isPlaying) return PSState.Playing;
        if (ps.isPaused) return PSState.Paused;
        return PSState.Stopped;
    }
}
