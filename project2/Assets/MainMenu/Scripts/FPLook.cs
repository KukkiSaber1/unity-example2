using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class FPLook : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag the player/character transform here (object that rotates horizontally)")]
    public Transform character;

    [Header("Look settings")]
    public float sensitivity = 2f;
    public float smoothing = 1.5f;
    public bool invertY = false;
    public bool startLocked = false;

    [Header("Touch pad (optional)")]
    [Tooltip("Optional: drag a RectTransform to restrict look to that UI area. Leave null to allow anywhere on screen.")]
    public RectTransform touchPadArea;

    // Internal state
    Vector2 velocity;
    Vector2 frameVelocity;

    // Input tracking
    int activeTouchId = -1;
    bool isPressing = false;
    Vector2 lastPointerPosition;

    void Start()
    {
        SetCursorLock(startLocked);
    }

    void Update()
    {
        // Toggle lock with L key for desktop convenience
        if (Input.GetKeyDown(KeyCode.L))
        {
            SetCursorLock(Cursor.lockState != CursorLockMode.Locked);
        }

        // Handle press start (touch or mouse)
        HandlePressStartAndEnd();

        // If cursor is unlocked and not pressing (desktop), do nothing
        if (Cursor.lockState != CursorLockMode.Locked && !isPressing)
            return;

        // Compute input delta
        Vector2 inputDelta = Vector2.zero;
        if (isPressing)
        {
            // Touch takes precedence
            if (activeTouchId != -1)
            {
                // Try to find that touch
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch t = Input.GetTouch(i);
                    if (t.fingerId == activeTouchId)
                    {
                        inputDelta = t.deltaPosition;
                        break;
                    }
                }
            }
            else if (Input.GetMouseButton(0))
            {
                Vector2 mousePos = (Vector2)Input.mousePosition;
                inputDelta = mousePos - lastPointerPosition;
                lastPointerPosition = mousePos;
            }
        }
        else
        {
            // No press. If cursor locked, still allow mouse look via Input axes
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * sensitivity;
                inputDelta = mouseDelta;
            }
        }

        // If inputDelta is pixels (touch/mouse delta), scale down to match mouse feel
        Vector2 scaled = inputDelta;
        if (activeTouchId != -1 || (isPressing && !Input.GetMouseButton(0) && Input.touchCount > 0))
        {
            // ensure touch uses a gentle scale; tweak multiplier if needed
            scaled = inputDelta * (sensitivity * 0.02f);
        }
        else if (Input.GetMouseButton(0) && isPressing)
        {
            // mouse dragging: scale similarly but a bit stronger
            scaled = inputDelta * (sensitivity * 0.02f);
        }
        // if using Input axes, scaled is already multiplied by sensitivity above

        // Smooth and accumulate
        Vector2 rawFrameVelocity = Vector2.Scale(scaled, Vector2.one);
        frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1f / Mathf.Max(0.0001f, smoothing));
        velocity += frameVelocity;

        // Clamp vertical rotation
        velocity.y = Mathf.Clamp(velocity.y, -90f, 90f);

        // Apply rotations
        float yRotation = invertY ? velocity.y : -velocity.y;
        transform.localRotation = Quaternion.AngleAxis(yRotation, Vector3.right);
        if (character != null)
            character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
    }

    void HandlePressStartAndEnd()
    {
        // Reset when nothing relevant
        bool pressedThisFrame = false;
        bool releasedThisFrame = false;

        // Touch handling: check touches first
        if (Input.touchCount > 0)
        {
            // If we already track a touch, follow its phases
            if (activeTouchId != -1)
            {
                bool stillFound = false;
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch t = Input.GetTouch(i);
                    if (t.fingerId == activeTouchId)
                    {
                        stillFound = true;
                        if (t.phase == TouchPhase.Began)
                        {
                            pressedThisFrame = true;
                            lastPointerPosition = t.position;
                        }
                        else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                        {
                            releasedThisFrame = true;
                        }
                        else
                        {
                            // Moved or Stationary -> keep pressing
                            isPressing = true;
                        }
                        break;
                    }
                }
                if (!stillFound)
                {
                    // Active touch disappeared, treat as released
                    releasedThisFrame = true;
                }
            }
            else
            {
                // No active touch tracked: consider any new touch that begins this frame
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch t = Input.GetTouch(i);
                    if (t.phase == TouchPhase.Began)
                    {
                        // If touchPadArea is set, only accept touches that started inside it
                        if (touchPadArea == null || IsScreenPointInRectTransform(t.position, touchPadArea))
                        {
                            activeTouchId = t.fingerId;
                            pressedThisFrame = true;
                            lastPointerPosition = t.position;
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            // No touches: clear activeTouchId
            activeTouchId = -1;
        }

        // Mouse handling (left button)
        if (Input.mousePresent)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // If touchPadArea is set, only accept mouse clicks inside it
                Vector2 mp = (Vector2)Input.mousePosition;
                if (touchPadArea == null || IsScreenPointInRectTransform(mp, touchPadArea))
                {
                    pressedThisFrame = true;
                    // cancel any active touch reservation
                    activeTouchId = -1;
                    lastPointerPosition = mp;
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                releasedThisFrame = true;
            }
        }

        // Apply state changes
        if (pressedThisFrame)
        {
            isPressing = true;
            // when pressing on desktop, lock cursor if desired
            if (Cursor.lockState != CursorLockMode.Locked && Input.mousePresent)
            {
                // don't force-lock; respect startLocked, but allow immediate look while mouse held
            }
        }

        if (releasedThisFrame)
        {
            isPressing = false;
            activeTouchId = -1;
            // don't zero velocity here; keep accumulated yaw so camera stays where it was
        }

        // If no input at all, ensure isPressing is false when not locked
        if (Cursor.lockState != CursorLockMode.Locked && Input.touchCount == 0 && !Input.GetMouseButton(0))
        {
            isPressing = false;
            activeTouchId = -1;
        }
    }

    private bool IsScreenPointInRectTransform(Vector2 screenPoint, RectTransform rect)
    {
        if (rect == null) return false;
        Camera cam = null;
        Canvas c = rect.GetComponentInParent<Canvas>();
        if (c != null && c.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = c.worldCamera;
        return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint, cam);
    }

    private void SetCursorLock(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
