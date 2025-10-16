using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class TimelineTrigger : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent OnTimelineTriggered;
    
    [Header("Settings")]
    public KeyCode interactionKey = KeyCode.E;
    public bool oneTimeUse = true;
    
    private bool playerInRange = false;
    private bool hasBeenUsed = false;
    
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey) && !(oneTimeUse && hasBeenUsed))
        {
            TriggerTimelineEvent();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !(oneTimeUse && hasBeenUsed))
        {
            playerInRange = true;
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
    
    void TriggerTimelineEvent()
    {
        OnTimelineTriggered?.Invoke();
        hasBeenUsed = true;
        playerInRange = false;
    }
}