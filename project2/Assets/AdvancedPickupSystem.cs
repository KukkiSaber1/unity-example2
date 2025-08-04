using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdvancedPickupSystem : MonoBehaviour
{
    public Transform holdPoint;
    public float detectionRadius = 5f;
    public LayerMask pickupLayer;
    public Text promptText;
    public float moveSpeed = 10f;
    public float throwForce = 5f; // Added throw force parameter

    private GameObject heldObject = null;
    private GameObject targetObject = null;

    void Update()
    {
        DetectObjects();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null && targetObject != null)
            {
                PickupObject(targetObject);
            }
            else if (heldObject != null)
            {
                DropObject();
            }
        }

        if (Input.GetMouseButtonDown(0) && heldObject != null) // Added throw on left click
        {
            ThrowObject();
        }

        if (heldObject != null)
        {
            MoveHeldObject();
        }
    }

    void DetectObjects()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, pickupLayer);
        float closestDistance = Mathf.Infinity;
        GameObject closestObject = null;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Pickup"))
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObject = hit.gameObject;
                }
            }
        }

        targetObject = closestObject;
        UpdatePrompt();
    }

    void UpdatePrompt()
    {
        if (promptText != null)
        {
            bool shouldShowPrompt = targetObject != null && heldObject == null;
            promptText.enabled = shouldShowPrompt;
            
            if (shouldShowPrompt)
            {
                promptText.text = $"Press E to pick up {targetObject.name}";
            }
        }
    }

    void PickupObject(GameObject obj)
    {
        heldObject = obj;
        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        heldObject.transform.SetParent(holdPoint);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;
    }

    void MoveHeldObject()
    {
        if (Vector3.Distance(heldObject.transform.position, holdPoint.position) > 0.1f)
        {
            heldObject.transform.position = Vector3.Lerp(
                heldObject.transform.position, 
                holdPoint.position, 
                moveSpeed * Time.deltaTime
            );
        }
    }

    void DropObject()
    {
        if (heldObject == null) return;

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        heldObject.transform.SetParent(null);
        heldObject = null;
    }

    void ThrowObject()
    {
        if (heldObject == null) return;

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(Camera.main.transform.forward * throwForce, ForceMode.Impulse);
        }

        heldObject.transform.SetParent(null);
        heldObject = null;
    }

    // Visualize detection radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}