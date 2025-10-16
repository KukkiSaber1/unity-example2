using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI")]
    [SerializeField] private Image background;
    [SerializeField] private Image thumb;

    [Header("Movement")]
    [Tooltip("Radius in pixels the thumb can move from the center")]
    [SerializeField] private float movementAreaRadius = 75f;
    [Tooltip("Deadzone radius in pixels; movement inside yields zero")]
    [SerializeField] private float deadzoneRadius = 10f;
    [SerializeField] private bool isDynamic = false;
    [SerializeField] private RectTransform dynamicMovementArea; // optional container for dynamic mode
    [SerializeField] private bool canFollowPointer = false;

    [Header("Behavior")]
    [Tooltip("If true the joystick will be invisible until touched (dynamic feel)")]
    [SerializeField] private bool hideWhenIdle = false;

    RectTransform rectTransform;
    RectTransform thumbTR;
    Vector2 pointerInitialLocalPos;
    Vector2 joystickInitialAnchoredPos;
    bool isHeld = false;

    // Public normalized value: x=left(-1)..right(1), y=down(-1)..up(1)
    public Vector2 Value { get; private set; } = Vector2.zero;

    void Awake()
    {
        rectTransform = transform as RectTransform;
        if (thumb != null) thumbTR = thumb.rectTransform;
        if (background == null)
            background = GetComponent<Image>();

        joystickInitialAnchoredPos = rectTransform.anchoredPosition;

        if (hideWhenIdle && background != null)
        {
            var c = background.color;
            c.a = isDynamic || isHeld ? 1f : 0f;
            background.color = c;
        }
    }

    void Update()
    {
        if (!isDynamic || !hideWhenIdle || background == null) return;

        float targetAlpha = isHeld ? 1f : 0f;
        var c = background.color;
        c.a = Mathf.MoveTowards(c.a, targetAlpha, Time.unscaledDeltaTime * 6f);
        background.color = c;
        if (thumb != null)
        {
            var tc = thumb.color;
            tc.a = c.a;
            thumb.color = tc;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHeld = true;

        if (isDynamic)
        {
            // Show joystick at pointer position (convert screen to local in canvas)
            if (dynamicMovementArea == null)
            {
                // try to use parent canvas rect transform
                var c = GetComponentInParent<Canvas>();
                if (c != null)
                {
                    dynamicMovementArea = c.transform as RectTransform;
                }
            }

            Vector2 worldPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(dynamicMovementArea ?? rectTransform, eventData.position, eventData.pressEventCamera, out worldPos);
            rectTransform.anchoredPosition = worldPos;
            pointerInitialLocalPos = Vector2.zero;
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out pointerInitialLocalPos);
        }

        if (thumbTR != null) thumbTR.localPosition = Vector3.zero;
        Value = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPointerPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localPointerPos);

        Vector2 direction = localPointerPos - pointerInitialLocalPos;

        // clamp by movement radius
        if (direction.sqrMagnitude > movementAreaRadius * movementAreaRadius)
        {
            Vector2 dirNorm = direction.normalized * movementAreaRadius;
            if (canFollowPointer)
            {
                // move joystick center so thumb can follow further
                rectTransform.anchoredPosition += (Vector2)(direction - dirNorm);
                // new pointer initial becomes pointer in local coords relative to new center
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out pointerInitialLocalPos);
                direction = dirNorm;
            }
            else
            {
                direction = dirNorm;
            }
        }

        // deadzone
        if (direction.sqrMagnitude <= deadzoneRadius * deadzoneRadius)
        {
            Value = Vector2.zero;
            if (thumbTR != null) thumbTR.localPosition = Vector3.zero;
        }
        else
        {
            // normalized value in -1..1 range per axis
            Vector2 normalized = direction / movementAreaRadius;
            normalized.x = Mathf.Clamp(normalized.x, -1f, 1f);
            normalized.y = Mathf.Clamp(normalized.y, -1f, 1f);
            Value = normalized;
            if (thumbTR != null) thumbTR.localPosition = new Vector3(direction.x, direction.y, 0f);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHeld = false;
        Value = Vector2.zero;
        if (thumbTR != null) thumbTR.localPosition = Vector3.zero;
        if (!isDynamic && canFollowPointer)
            rectTransform.anchoredPosition = joystickInitialAnchoredPos;
        if (isDynamic)
        {
            // optionally hide by moving back to initial pos
            rectTransform.anchoredPosition = joystickInitialAnchoredPos;
        }
    }

    // Optional helpers for external scripts:
    public bool IsHeld() => isHeld;
    public void ResetJoystick()
    {
        isHeld = false;
        Value = Vector2.zero;
        if (thumbTR != null) thumbTR.localPosition = Vector3.zero;
        rectTransform.anchoredPosition = joystickInitialAnchoredPos;
    }
}
