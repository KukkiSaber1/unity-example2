using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Cursor Control")]
    public CursorLockController cursorLockController;
    
    void Update()
    {
        // Example: Press Escape to release cursor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReleaseCursorFromAnywhere();
        }
        
        // Example: Some game condition
        if (PlayerDied())
        {
            ReleaseCursorFromAnywhere();
        }
    }
    
    public void ReleaseCursorFromAnywhere()
    {
        // Method 1: Using Event System (Best - no reference needed)
        CursorEventManager.ReleaseCursor();
        
        // Method 2: Using direct reference
        // cursorLockController?.ManualRelease();
        
        // Method 3: Using FindObject (if you don't have reference)
        // FindObjectOfType<CursorLockController>()?.ManualRelease();
    }
    
    bool PlayerDied()
    {
        // Your game logic
        return false;
    }
}