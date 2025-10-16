using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SwipeDetection : MonoBehaviour
{
    [Header("Swipe Settings")]
    public float minSwipeDistance = 50f;
    public float maxSwipeTime = 1f;
    
    [Header("UI References")]
    public RectTransform swipeArea; // Changed to RectTransform for better area detection
    public TextMeshProUGUI feedbackText;
    public Image swipeTrail; // Optional: visual trail effect
    
    [Header("Events")]
    public UnityEvent OnSwipeSuccess; // Triggers immediately after successful swipe
    public UnityEvent OnSwipeDelayedEnd; // Triggers after the 1-second delay from "Nice Job!"
    
    [Header("Activation Settings")]
    public bool startEnabled = false;
    
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private float startTime;
    private bool isSwiping = false;
    private bool swipeCompleted = false;
    private bool isEnabled = false;
    private bool touchStartedInArea = false;

    void Start()
    {
        // Set initial enabled state
        isEnabled = startEnabled;
        
        // Ensure UI elements are in correct state
        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);
            
        if (swipeTrail != null)
            swipeTrail.gameObject.SetActive(false);
    }

    void Update()
    {
        // Only process input if this GameObject is active in hierarchy AND swipe detection is enabled
        if (!isEnabled || !gameObject.activeInHierarchy) return;
        
        HandleSwipeInput();
    }

    void HandleSwipeInput()
    {
        // Mouse input for testing in editor
        if (Input.GetMouseButtonDown(0))
        {
            if (IsTouchInSwipeArea(Input.mousePosition))
            {
                StartSwipe(Input.mousePosition);
            }
        }
        else if (Input.GetMouseButton(0) && isSwiping && touchStartedInArea)
        {
            UpdateSwipeTrail(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) && isSwiping && touchStartedInArea)
        {
            EndSwipe(Input.mousePosition);
        }

        // Touch input for mobile
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                if (IsTouchInSwipeArea(touch.position))
                {
                    StartSwipe(touch.position);
                }
            }
            else if (touch.phase == TouchPhase.Moved && isSwiping && touchStartedInArea)
            {
                UpdateSwipeTrail(touch.position);
            }
            else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && isSwiping && touchStartedInArea)
            {
                EndSwipe(touch.position);
            }
        }
    }

    void StartSwipe(Vector2 position)
    {
        if (!isEnabled || !gameObject.activeInHierarchy) return;
        
        startTouchPosition = position;
        startTime = Time.time;
        isSwiping = true;
        swipeCompleted = false;
        touchStartedInArea = true;
        
        // Reset UI
        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);
            
        // Show swipe trail
        if (swipeTrail != null)
        {
            swipeTrail.gameObject.SetActive(true);
            UpdateSwipeTrail(position);
        }
        
        Debug.Log("Swipe started in swipe area");
    }

    void UpdateSwipeTrail(Vector2 currentPosition)
    {
        if (!isEnabled || !gameObject.activeInHierarchy) return;
        
        // Optional: Check if still in area during swipe (comment out if you want to allow leaving area)
        /*
        if (!IsTouchInSwipeArea(currentPosition))
        {
            // Cancel swipe if left the area
            CancelSwipe();
            return;
        }
        */
        
        if (swipeTrail != null)
        {
            swipeTrail.rectTransform.position = currentPosition;
        }
    }

    void EndSwipe(Vector2 position)
    {
        if (!isEnabled || !gameObject.activeInHierarchy) return;
        
        endTouchPosition = position;
        
        // Optional: Check if ended in area (comment out if you don't care where it ends)
        /*
        if (!IsTouchInSwipeArea(position))
        {
            CancelSwipe();
            return;
        }
        */
        
        float swipeTime = Time.time - startTime;
        float swipeDistance = Vector2.Distance(startTouchPosition, endTouchPosition);

        // Check if swipe is valid
        if (swipeTime <= maxSwipeTime && swipeDistance >= minSwipeDistance)
        {
            SwipeSuccess();
        }
        else
        {
            SwipeFail();
        }
        
        ResetSwipeState();
    }

    void CancelSwipe()
    {
        Debug.Log("Swipe cancelled - outside swipe area");
        
        // Hide swipe trail
        if (swipeTrail != null)
            swipeTrail.gameObject.SetActive(false);
            
        ResetSwipeState();
    }

    void ResetSwipeState()
    {
        isSwiping = false;
        touchStartedInArea = false;
        
        // Hide swipe trail
        if (swipeTrail != null)
            swipeTrail.gameObject.SetActive(false);
    }

    void SwipeSuccess()
    {
        if (swipeCompleted || !isEnabled || !gameObject.activeInHierarchy) return;
        
        swipeCompleted = true;
        
        // Show success message
        if (feedbackText != null)
        {
            feedbackText.text = "Nice Job!";
            feedbackText.color = Color.green;
            feedbackText.gameObject.SetActive(true);
        }
        
        // 🔥 EVENT 1: Trigger SUCCESS event immediately
        OnSwipeSuccess?.Invoke();
        Debug.Log("Swipe Success Event Triggered!");
        
        // Start coroutine for delayed end event
        StartCoroutine(DelayedSwipeEnd());
    }

    void SwipeFail()
    {
        if (!isEnabled || !gameObject.activeInHierarchy) return;
        
        if (feedbackText != null)
        {
            feedbackText.text = "Swipe Faster/Longer!";
            feedbackText.color = Color.red;
            feedbackText.gameObject.SetActive(true);
        }
        
        // Hide fail message after delay
        StartCoroutine(HideFeedbackAfterDelay(1f));
    }

    IEnumerator DelayedSwipeEnd()
    {
        // Wait for 1 second delay after "Nice Job!" appears
        yield return new WaitForSeconds(1f);
        
        // Only proceed if we're still active
        if (isEnabled && gameObject.activeInHierarchy)
        {
            // Hide feedback text
            if (feedbackText != null)
                feedbackText.gameObject.SetActive(false);
            
            // 🔥 EVENT 2: Trigger DELAYED END event after the delay
            OnSwipeDelayedEnd?.Invoke();
            Debug.Log("Swipe Delayed End Event Triggered!");
        }
    }

    IEnumerator HideFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (feedbackText != null && gameObject.activeInHierarchy)
            feedbackText.gameObject.SetActive(false);
    }

    bool IsTouchInSwipeArea(Vector2 touchPosition)
    {
        if (swipeArea == null)
        {
            Debug.LogWarning("SwipeArea is not assigned!");
            return false;
        }
        
        // Convert screen position to local position within the swipe area
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            swipeArea, 
            touchPosition, 
            null, // No camera needed for UI
            out localPoint
        );
        
        // Check if the point is within the rectangle
        bool isInArea = swipeArea.rect.Contains(localPoint);
        
        // Optional: Visual debug
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Debug.Log($"Touch at {touchPosition} - In swipe area: {isInArea}");
        }
        
        return isInArea;
    }

    // ================= PUBLIC METHODS TO CONTROL ACTIVATION =================

    /// <summary>
    /// Enable swipe detection
    /// </summary>
    public void EnableSwipe()
    {
        isEnabled = true;
        Debug.Log("Swipe detection ENABLED");
    }

    /// <summary>
    /// Disable swipe detection
    /// </summary>
    public void DisableSwipe()
    {
        isEnabled = false;
        ResetSwipeState();
        Debug.Log("Swipe detection DISABLED");
    }

    /// <summary>
    /// Toggle swipe detection on/off
    /// </summary>
    public void ToggleSwipe()
    {
        isEnabled = !isEnabled;
        Debug.Log($"Swipe detection {(isEnabled ? "ENABLED" : "DISABLED")}");
    }

    /// <summary>
    /// Check if swipe detection is currently enabled
    /// </summary>
    public bool IsSwipeEnabled()
    {
        return isEnabled && gameObject.activeInHierarchy;
    }

    // Public method to manually trigger swipe success (for testing)
    [ContextMenu("Test Swipe Success")]
    public void TestSwipeSuccess()
    {
        if (isEnabled && gameObject.activeInHierarchy)
            SwipeSuccess();
    }

    // Public method to test the delayed end event
    [ContextMenu("Test Delayed End")]
    public void TestDelayedEnd()
    {
        if (isEnabled && gameObject.activeInHierarchy)
            StartCoroutine(DelayedSwipeEnd());
    }

    // Debug method to visualize swipe area in editor
    void OnDrawGizmosSelected()
    {
        if (swipeArea != null)
        {
            // Draw the swipe area boundaries in Scene view
            Vector3[] corners = new Vector3[4];
            swipeArea.GetWorldCorners(corners);
            
            Gizmos.color = Color.cyan;
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
            }
        }
    }
}