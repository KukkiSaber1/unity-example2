using UnityEngine;

/// <summary>
/// Simple, robust Rigidbody-first-person movement that reads either:
///  - a reference to a Joystick component exposing public Vector2 Value, OR
///  - Unity Input axes ("Horizontal", "Vertical") when no joystick is assigned.
/// Attach this to the same GameObject that has the Rigidbody for the player body.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class FPMovementNew : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Assign your on-screen Joystick here (must expose public Vector2 Value). If empty, Unity Input axes will be used.")]
    public MonoBehaviour joystickComponent;

    [Header("Speeds")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;

    [Header("Running")]
    public bool canRun = true;
    public KeyCode runKey = KeyCode.LeftShift;
    public bool IsRunning { get; private set; }

    [Header("Smoothing")]
    [Tooltip("0 = instant velocity, >0 smooths velocity changes (seconds).")]
    [Range(0f, 0.5f)]
    public float velocitySmoothTime = 0.08f;

    [Header("Misc")]
    [Tooltip("If true attempts to find a Joystick in the scene at Awake when none assigned.")]
    public bool autoFindJoystick = true;
    public bool debugLogs = false;

    Rigidbody rb;

    // internal smoothing
    Vector3 currentVelocity;
    Vector3 velocitySmoothVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            Debug.LogError("FPMovementNew requires a Rigidbody on the same GameObject.");

        // If no joystick assigned, try to find one (type name "Joystick")
        if (joystickComponent == null && autoFindJoystick)
        {
            var all = FindObjectsOfType<MonoBehaviour>();
            foreach (var mb in all)
            {
                if (mb == null) continue;
                var tName = mb.GetType().Name;
                if (tName == "Joystick" || tName.EndsWith("Joystick"))
                {
                    joystickComponent = mb;
                    if (debugLogs) Debug.Log($"FPMovementNew: Auto-wired joystick [{tName}]");
                    break;
                }
            }
        }
    }

    void FixedUpdate()
    {
        // Running state
        IsRunning = canRun && Input.GetKey(runKey);
        float targetSpeed = IsRunning ? runSpeed : walkSpeed;

        // Read input: joystick preferred
        Vector2 rawInput = Vector2.zero;

        if (joystickComponent != null)
        {
            // Try property named "Value" first, then field named "Value"
            var t = joystickComponent.GetType();
            var prop = t.GetProperty("Value");
            if (prop != null && prop.PropertyType == typeof(Vector2))
            {
                rawInput = (Vector2)prop.GetValue(joystickComponent);
            }
            else
            {
                var field = t.GetField("Value");
                if (field != null && field.FieldType == typeof(Vector2))
                    rawInput = (Vector2)field.GetValue(joystickComponent);
                // else remain zero and fallback to Unity axes below
            }
        }

        if (rawInput == Vector2.zero)
        {
            rawInput.x = Input.GetAxis("Horizontal");
            rawInput.y = Input.GetAxis("Vertical");
        }

        // Normalize diagonal input to avoid faster diagonal movement
        Vector2 clampedInput = Vector2.ClampMagnitude(rawInput, 1f);

        // Build local movement vector (x = right, z = forward)
        Vector3 localMove = new Vector3(clampedInput.x, 0f, clampedInput.y) * targetSpeed;

        // Convert local (relative to player rotation) movement to world space
        Vector3 desiredVelocity = transform.rotation * localMove;

        // Preserve current vertical velocity (gravity/jumps)
        desiredVelocity.y = rb.velocity.y;

        // Smooth velocity if requested
        if (velocitySmoothTime > 0f)
        {
            // smooth damp works per-axis
            float smooth = Mathf.Max(0.0001f, velocitySmoothTime);
            Vector3 current = rb.velocity;
            Vector3 target = desiredVelocity;

            float t = Time.fixedDeltaTime / smooth;
            // simple exponential smoothing
            Vector3 newVel = Vector3.Lerp(current, target, t);

            rb.velocity = newVel;
        }
        else
        {
            rb.velocity = desiredVelocity;
        }

        if (debugLogs)
            Debug.Log($"FPMove Input:{rawInput} Speed:{targetSpeed:F2} Vel:{rb.velocity}");
    }
}
