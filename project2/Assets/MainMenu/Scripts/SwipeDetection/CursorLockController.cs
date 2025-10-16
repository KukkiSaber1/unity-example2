using UnityEngine;
using System.Collections;

public class CursorLockController : MonoBehaviour
{
    [Header("Cursor Settings")]
    public bool lockCursorOnStart = true;
    public CursorLockMode lockMode = CursorLockMode.Locked;
    
    private bool isCursorLocked = false;
    private Coroutine lockCoroutine;

    void Start()
    {
        // Subscribe to release event
        CursorEventManager.OnReleaseCursor += ReleaseCursor;
        
        if (lockCursorOnStart)
        {
            LockCursor();
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from event
        CursorEventManager.OnReleaseCursor -= ReleaseCursor;
    }

    public void LockCursor()
    {
        if (isCursorLocked) return;
        
        isCursorLocked = true;
        Cursor.lockState = lockMode;
        Cursor.visible = false;
        
        // Start any coroutines related to cursor lock
        lockCoroutine = StartCoroutine(CursorLockRoutine());
    }

    public void ReleaseCursor()
    {
        if (!isCursorLocked) return;
        
        isCursorLocked = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Stop all coroutines
        if (lockCoroutine != null)
        {
            StopCoroutine(lockCoroutine);
            lockCoroutine = null;
        }
        
        StopAllCoroutines();
        CancelInvoke(); // If using Invoke
    }

    IEnumerator CursorLockRoutine()
    {
        while (isCursorLocked)
        {
            // Your cursor lock logic here
            yield return null;
        }
    }

    // For manual release
    public void ManualRelease()
    {
        ReleaseCursor();
    }
}