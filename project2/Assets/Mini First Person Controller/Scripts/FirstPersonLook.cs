using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField]
    Transform character;
    public float sensitivity = 2;
    public float smoothing = 1.5f;

    [Header("Cursor")]
    [Tooltip("If true the cursor will start locked; otherwise it starts unlocked")]
    public bool startLocked = false;

    Vector2 velocity;
    Vector2 frameVelocity;

    void Reset()
    {
        // Get the character from the FirstPersonMovement in parents.
        character = GetComponentInParent<FirstPersonMovement>().transform;
    }

    void Start()
    {
        // Start cursor lock state according to startLocked
        SetCursorLock(startLocked);
    }

    void Update()
    {
        // Toggle cursor lock with hidden key L
        if (Input.GetKeyDown(KeyCode.L))
        {
            bool nowLocked = Cursor.lockState == CursorLockMode.Locked ? false : true;
            SetCursorLock(nowLocked);
        }

        // If the cursor is unlocked, skip looking around
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        // Get smooth velocity.
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity);
        frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1 / smoothing);
        velocity += frameVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -90, 90);

        // Rotate camera up-down and controller left-right from velocity.
        transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
        character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
    }

    private void SetCursorLock(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
