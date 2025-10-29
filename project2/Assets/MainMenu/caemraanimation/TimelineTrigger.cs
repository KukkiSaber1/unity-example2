using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.UI;

public class TimelineTrigger : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent OnTimelineTriggered;

    [Header("Settings")]
    public KeyCode interactionKey = KeyCode.E;
    public bool oneTimeUse = true;

    [Header("Inspector UI Button (optional)")]
    [Tooltip("Drag the UI Button here or assign it at runtime. If assigned, the script will auto-subscribe to its onClick.")]
    public Button uiButton;

    [Tooltip("If true, the UI button will only trigger when the player is in range. If false, UI button can trigger anytime.")]
    public bool uiRequiresPlayerInRange = false;

    private bool playerInRange = false;
    private bool hasBeenUsed = false;

    void OnEnable()
    {
        if (uiButton != null)
        {
            uiButton.onClick.AddListener(OnUIButtonClicked);
        }
    }

    void OnDisable()
    {
        if (uiButton != null)
        {
            uiButton.onClick.RemoveListener(OnUIButtonClicked);
        }
    }

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

    // Called by the UI Button (auto-subscribed) or you can call this from other scripts / inspector
    public void OnUIButtonClicked()
    {
        if (oneTimeUse && hasBeenUsed) return;
        if (uiRequiresPlayerInRange && !playerInRange) return;
        TriggerTimelineEvent();
    }

    public void TriggerTimelineEvent()
    {
        OnTimelineTriggered?.Invoke();
        hasBeenUsed = true;
        playerInRange = false;
    }
}
