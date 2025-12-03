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
    public GameObject swipeArea;
    public TextMeshProUGUI feedbackText;
    public Image swipeTrail; // Optional: visual trail effect
    
    [Header("Events")]
    public UnityEvent OnSwipeSuccess;
    public UnityEvent OnSwipeEnd;
    
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private float startTime;
    private bool isSwiping = false;
    private bool swipeCompleted = false;

    void Update()
    {
        HandleSwipeInput();
    }

    void HandleSwipeInput()
    {
        // Mouse input for testing in editor
        if (Input.GetMouseButtonDown(0) && IsTouchInSwipeArea(Input.mousePosition))
        {
            StartSwipe(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && isSwiping)
        {
            UpdateSwipeTrail(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            EndSwipe(Input.mousePosition);
        }

        // Touch input for mobile
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began && IsTouchInSwipeArea(touch.position))
            {
                StartSwipe(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved && isSwiping)
            {
                UpdateSwipeTrail(touch.position);
            }
            else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && isSwiping)
            {
                EndSwipe(touch.position);
            }
        }
    }

    void StartSwipe(Vector2 position)
    {
        startTouchPosition = position;
        startTime = Time.time;
        isSwiping = true;
        swipeCompleted = false;
        
        // Reset UI
        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);
            
        // Show swipe trail
        if (swipeTrail != null)
        {
            swipeTrail.gameObject.SetActive(true);
            UpdateSwipeTrail(position);
        }
    }

    void UpdateSwipeTrail(Vector2 currentPosition)
    {
        if (swipeTrail != null)
        {
            swipeTrail.rectTransform.position = currentPosition;
        }
    }

    void EndSwipe(Vector2 position)
    {
        endTouchPosition = position;
        isSwiping = false;
        
        // Hide swipe trail
        if (swipeTrail != null)
            swipeTrail.gameObject.SetActive(false);

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
    }

    void SwipeSuccess()
    {
        if (swipeCompleted) return;
        
        swipeCompleted = true;
        
        // Show success message
        if (feedbackText != null)
        {
            feedbackText.text = "<b>Nice Job!</b>";
            feedbackText.color = Color.green;
            feedbackText.gameObject.SetActive(true);
        }
        
        // Trigger success event
        OnSwipeSuccess?.Invoke();
        
        // Start coroutine for delayed end event
        StartCoroutine(DelayedSwipeEnd());
    }

    void SwipeFail()
    {
        if (feedbackText != null)
        {
            feedbackText.text = "<b>Swipe Faster/Longer!</b>";
            feedbackText.color = Color.red;
            feedbackText.gameObject.SetActive(true);
        }
        
        // Hide fail message after delay
        StartCoroutine(HideFeedbackAfterDelay(1f));
    }

    IEnumerator DelayedSwipeEnd()
    {
        // Wait for 1 second delay
        yield return new WaitForSeconds(1f);
        
        // Hide feedback text
        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);
        
        // Trigger end event
        OnSwipeEnd?.Invoke();
        
        Debug.Log("Swipe completed successfully!");
    }

    IEnumerator HideFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);
    }

    bool IsTouchInSwipeArea(Vector2 touchPosition)
    {
        if (swipeArea == null) return true;
        
        RectTransform rectTransform = swipeArea.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, touchPosition);
    }

    // Public method to manually trigger swipe success (for testing)
    [ContextMenu("Test Swipe Success")]
    public void TestSwipeSuccess()
    {
        SwipeSuccess();
    }
}