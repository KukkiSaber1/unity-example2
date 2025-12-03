using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SwipeDetectionCircle : MonoBehaviour
{
    [Header("Swipe Settings")]
    public float minSwipeDistance = 50f;
    public float maxSwipeTime = 1f;

    [Header("Circle Detection Settings")]
    [Tooltip("Minimum total swept angle in degrees to consider a circle (e.g. 270 = large arc, 360 = full)")]
    public float requiredSweepDegrees = 300f;
    [Tooltip("Max allowed normalized radius deviation (stdDev / meanRadius)")]
    public float maxRadiusNormalizedStdDev = 0.35f;
    [Tooltip("Minimum number of samples to validate a circle")]
    public int minSamples = 8;
    [Tooltip("Minimum distance between consecutive stored samples (screen pixels)")]
    public float sampleSpacing = 10f;

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

    // New: store sampled points during a swipe (screen space)
    private List<Vector2> samples = new List<Vector2>();
    private Vector2 lastSamplePos;

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
            SamplePosition(Input.mousePosition);
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
            else if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) && isSwiping)
            {
                UpdateSwipeTrail(touch.position);
                SamplePosition(touch.position);
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

        samples.Clear();
        lastSamplePos = position;
        samples.Add(position);

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

    void SamplePosition(Vector2 pos)
    {
        if (samples.Count == 0)
        {
            samples.Add(pos);
            lastSamplePos = pos;
            return;
        }

        if (Vector2.Distance(pos, lastSamplePos) >= sampleSpacing)
        {
            samples.Add(pos);
            lastSamplePos = pos;
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

        // Ensure final sample recorded
        SamplePosition(position);

        // Basic time/distance filter first
        if (swipeTime <= maxSwipeTime && swipeDistance >= minSwipeDistance)
        {
            // Check if the gesture is circular
            if (IsCircularSwipe(samples))
            {
                SwipeSuccess();
                return;
            }
            else
            {
                // Not circular but still a valid swipe: you can choose to treat it as success or fail
                SwipeFail();
                return;
            }
        }
        else
        {
            SwipeFail();
        }
    }

    bool IsCircularSwipe(List<Vector2> points)
    {
        if (points == null || points.Count < minSamples) return false;

        // Compute center as mean of points
        Vector2 center = Vector2.zero;
        foreach (var p in points) center += p;
        center /= points.Count;

        // Compute angles and radii
        int n = points.Count;
        float[] angles = new float[n];
        float[] radii = new float[n];
        for (int i = 0; i < n; i++)
        {
            Vector2 d = points[i] - center;
            angles[i] = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg; // -180..180
            radii[i] = d.magnitude;
        }

        // Unwrap angles and compute total swept angle magnitude
        float totalSweep = 0f;
        float prevAngle = angles[0];
        float accum = 0f;
        for (int i = 1; i < n; i++)
        {
            float a = angles[i];
            float delta = Mathf.DeltaAngle(prevAngle, a); // signed shortest delta
            accum += delta;
            prevAngle = a;
        }
        totalSweep = Mathf.Abs(accum);

        // Evaluate radius consistency (std dev / mean)
        float meanRadius = radii.Average();
        float variance = radii.Select(r => (r - meanRadius) * (r - meanRadius)).Average();
        float stdDev = Mathf.Sqrt(variance);
        float normalizedStdDev = meanRadius > 0f ? stdDev / meanRadius : float.MaxValue;

        // Require fairly continuous sweep and reasonable radius consistency
        bool sweepOk = totalSweep >= requiredSweepDegrees;
        bool radiusOk = normalizedStdDev <= maxRadiusNormalizedStdDev;

        // Extra: ensure not just a tight back-and-forth by checking angle monotonicity ratio
        // Count angle sign changes of deltas (large flips reduce monotonicity)
        int signChanges = 0;
        float prevDelta = Mathf.DeltaAngle(angles[0], angles[1]);
        for (int i = 2; i < n; i++)
        {
            float d = Mathf.DeltaAngle(angles[i - 1], angles[i]);
            if (Mathf.Sign(d) != Mathf.Sign(prevDelta) && Mathf.Abs(d) > 10f && Mathf.Abs(prevDelta) > 10f)
                signChanges++;
            prevDelta = d;
        }
        bool monotonicEnough = signChanges <= Mathf.Max(1, n / 6);

        // Return true only if all checks pass
        return sweepOk && radiusOk && monotonicEnough;
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
