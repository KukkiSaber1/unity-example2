using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class LongPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Settings")]
    public float holdTime = 2f; // Seconds to hold

    [Header("UI")]
    public Image progressImage; // Assign ProgressFill Image here

    [Header("Events")]
    public UnityEvent onLongPressComplete;

    private bool isHolding = false;
    private float holdTimer = 0f;

    void Update()
    {
        if (isHolding)
        {
            holdTimer += Time.deltaTime;

            if (progressImage != null)
                progressImage.fillAmount = holdTimer / holdTime;

            if (holdTimer >= holdTime)
            {
                isHolding = false;
                holdTimer = 0f;

                if (progressImage != null)
                    progressImage.fillAmount = 0f;

                onLongPressComplete?.Invoke();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        holdTimer = 0f;

        if (progressImage != null)
            progressImage.fillAmount = 0f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetHold();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetHold();
    }

    private void ResetHold()
    {
        isHolding = false;
        holdTimer = 0f;

        if (progressImage != null)
            progressImage.fillAmount = 0f;
    }
}