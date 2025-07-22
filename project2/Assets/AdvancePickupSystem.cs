using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdvancedPickupSystem : MonoBehaviour
{
    public Transform holdPoint;
    public float detectionRadius = 5f; // Larger detection radius
    public LayerMask pickupLayer; // Assign layer for pickup objects
    public Text promptText; // Assign your UI prompt text
    public float moveSpeed = 10f; // Speed to move held object

    private Rigidbody heldObjectRb = null;
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

        // Move the held object smoothly
        if (heldObject != null)
        {
            Vector3 targetPosition = holdPoint.position;
            heldObject.transform.position = Vector3.Lerp(heldObject.transform.position, targetPosition, moveSpeed * Time.deltaTime);
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

        // Show prompt if there's a target object
        if (promptText != null)
        {
            promptText.enabled = (targetObject != null && heldObject == null);
            if (promptText.enabled)
            {
                promptText.text = "Press E to pick up " + targetObject.name;
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
        }

        // Optional: Add outline highlighting here
        // e.g., Outline outline = heldObject.GetComponent<Outline>();
        // if (outline != null) outline.enabled = false;

        // Parent to hold point
        heldObject.transform.parent = holdPoint;
        // Reset position to hold point
        heldObject.transform.position = holdPoint.position;
    }

    void DropObject()
    {
        if (heldObject != null)
        {
            // Unparent
            heldObject.transform.parent = null;

            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                // Optional: Add force for drop effect
                // rb.AddForce(transform.forward * 2f, ForceMode.VelocityChange);
            }

            // Optional: Disable outline highlight
            // e.g., Outline outline = heldObject.GetComponent<Outline>();
            // if (outline != null) outline.enabled = true;

            heldObject = null;
        }
    }
}