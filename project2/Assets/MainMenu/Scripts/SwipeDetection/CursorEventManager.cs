using UnityEngine;
using UnityEngine.Events;

public static class CursorEventManager
{
    public static event System.Action OnReleaseCursor;
    
    public static void ReleaseCursor()
    {
        OnReleaseCursor?.Invoke();
    }
}