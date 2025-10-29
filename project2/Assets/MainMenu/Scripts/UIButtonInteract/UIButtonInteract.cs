using UnityEngine;

public class UIButtonInteract : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            DoInteract();
    }

    public void DoInteract() // public so UI can call it
    {
        // interaction code
    }
}
