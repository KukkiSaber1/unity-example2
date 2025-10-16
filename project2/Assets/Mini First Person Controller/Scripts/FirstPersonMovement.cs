using System.Collections.Generic;
using UnityEngine;

// If your joystick class lives in a namespace (for example SimpleInputNamespace),
// set the field in the inspector by dragging the Joystick component onto the 'joystick' slot.
// This script does not depend on SimpleInput or any static axis system.
public class FirstPersonMovement : MonoBehaviour
{
    [Header("Speeds")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;

    [Header("Running")]
    public bool canRun = true;
    public KeyCode runningKey = KeyCode.LeftShift;
    public bool IsRunning { get; private set; }

    [Header("Input")]
    [Tooltip("Optional: assign your Joystick component here (must expose a public Vector2 Value). If left empty the script falls back to Unity Input axes.")]
    public MonoBehaviour joystick; // assign your Joystick component here (e.g., the on-screen joystick script)

    [Header("Misc")]
    public bool autoFindJoystick = true;

    Rigidbody rb;
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            Debug.LogError("FirstPersonMovement requires a Rigidbody on the same GameObject.");

        if (joystick == null && autoFindJoystick)
        {
            // Try to find a component named "Joystick" anywhere in the scene
            var found = FindObjectOfType<MonoBehaviour>();
            // Search more specifically: try types named "Joystick" or in known namespace
            var all = FindObjectsOfType<MonoBehaviour>();
            foreach (var mb in all)
            {
                if (mb == null) continue;
                var tName = mb.GetType().Name;
                if (tName == "Joystick" || tName.EndsWith("Joystick"))
                {
                    joystick = mb;
                    break;
                }
            }
        }
    }

    void FixedUpdate()
    {
        // Update running flag
        IsRunning = canRun && Input.GetKey(runningKey);

        // Determine target speed (allows overrides)
        float targetSpeed = IsRunning ? runSpeed : walkSpeed;
        if (speedOverrides.Count > 0)
            targetSpeed = speedOverrides[speedOverrides.Count - 1]();

        // Read input: prefer joystick.Value if the assigned joystick exposes it, else use Unity axes.
        Vector2 input = Vector2.zero;

        if (joystick != null)
        {
            // Try to read a public Vector2 property/field named "Value" on the assigned component
            var jt = joystick.GetType();
            var prop = jt.GetProperty("Value");
            if (prop != null && prop.PropertyType == typeof(Vector2))
            {
                input = (Vector2)prop.GetValue(joystick);
            }
            else
            {
                var field = jt.GetField("Value");
                if (field != null && field.FieldType == typeof(Vector2))
                    input = (Vector2)field.GetValue(joystick);
            }
        }

        // Fallback to Unity Input if joystick not present or provided no value
        if (input == Vector2.zero)
        {
            input.x = Input.GetAxis("Horizontal");
            input.y = Input.GetAxis("Vertical");
        }

        // Apply speed
        Vector3 localMovement = new Vector3(input.x * targetSpeed, 0f, input.y * targetSpeed);

        // Rotate local velocity into world space using transform rotation
        Vector3 newVelocity = transform.rotation * localMovement;
        // Preserve current y velocity (gravity / jumping)
        newVelocity.y = rb.velocity.y;

        rb.velocity = newVelocity;
    }
}
