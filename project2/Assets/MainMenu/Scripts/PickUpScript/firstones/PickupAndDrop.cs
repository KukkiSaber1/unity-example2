using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupAndDrop : MonoBehaviour
{
    public Transform holdPoint; // Assign this to an empty child object (e.g., attach to camera)
    public float pickupRange = 3f; // Max distance to pick up objects
    private Rigidbody heldObjectRb = null;
    private GameObject heldObject = null;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null)
            {
                TryPickup();
            }
            else
            {
                DropObject();
            }
        }

        // If holding an object, make it follow the holdPoint
        if (heldObject != null)
        {
            heldObject.transform.position = holdPoint.position;
        }
    }

    void TryPickup()
    {
        RaycastHit hit;
        // Raycast from camera forward
        if (Physics.Raycast(transform.position, transform.forward, out hit, pickupRange))
        {
            if (hit.collider.gameObject.CompareTag("Pickup"))
            {
                heldObject = hit.collider.gameObject;
                heldObjectRb = heldObject.GetComponent<Rigidbody>();
                if (heldObjectRb != null)
                {
                    heldObjectRb.isKinematic = true; // Disable physics while holding
                }
                // Optional: Disable gravity
                // heldObjectRb.useGravity = false;

                // Parent the object to the hold point for smoother movement
                heldObject.transform.parent = holdPoint;
            }
        }
    }

    void DropObject()
    {
        if (heldObject != null)
        {
            // Unparent the object
            heldObject.transform.parent = null;
            if (heldObjectRb != null)
            {
                heldObjectRb.isKinematic = false; // Re-enable physics
            }

            heldObject = null;
            heldObjectRb = null;
        }
    }
}