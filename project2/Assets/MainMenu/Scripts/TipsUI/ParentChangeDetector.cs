using UnityEngine;

public class ParentChangeDetector : MonoBehaviour
{
    private Transform lastParent;

    void Start()
    {
        lastParent = transform.parent;
    }

    void Update()
    {
        if (transform.parent != lastParent)
        {
            Debug.Log($"Parent changed from {lastParent} to {transform.parent}");
            lastParent = transform.parent;

            // Call your event or function here
            OnParentChanged();
        }
    }

    void OnParentChanged()
    {
        // Your logic here
    }
}
