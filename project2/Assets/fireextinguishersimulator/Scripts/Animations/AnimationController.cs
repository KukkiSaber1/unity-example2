using UnityEngine;
using System.Linq;

public class AnimationController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayAnimation(string animationName)
    {
        animator.Play(animationName);
    }

    public void OnUnpinSafetyPinAnimationEnd()
    {
        var safetyPins = GameObject.FindObjectsOfType<InteractableObject>()
            .Where(io => io.interactionType == InteractableObject.InteractionType.SafetyPin)
            .Select(io => io.gameObject)
            .ToArray();

        foreach (GameObject safetyPin in safetyPins)
        {
            safetyPin.GetComponent<InteractableObject>().extinguisher.isSafetyPinRemoved = true;
            GameObject clonedSafetyPin = Instantiate(safetyPin, safetyPin.transform.position, safetyPin.transform.rotation);
            clonedSafetyPin.transform.SetParent(null);  
            Destroy(clonedSafetyPin.GetComponent<InteractableObject>());

            Rigidbody rb = clonedSafetyPin.AddComponent<Rigidbody>();
            Collider collider = clonedSafetyPin.AddComponent<BoxCollider>();
            rb.useGravity = true;
            rb.isKinematic = false;
            collider.isTrigger = false;

            safetyPin.SetActive(false);
        }
    }
}
