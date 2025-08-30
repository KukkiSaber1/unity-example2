using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSystem : MonoBehaviour
{
    public GameObject heldItem; // Reference to the currently held item
    public LayerMask itemLayer; // Set this to your "Item" layer in Inspector
    public LayerMask holdItemLayer; // Set this to your "holdItem" layer in Inspector

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) // Or your pickup key
        {
            if (heldItem == null)
            {
                TryPickup();
            }
            else
            {
                DropItem();
            }
        }
    }

    void TryPickup()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 3f, itemLayer))
        {
            heldItem = hit.collider.gameObject;
            heldItem.layer = LayerMask.NameToLayer("holdItem");
            // Optional: Make it a child of the player or camera
            heldItem.transform.SetParent(Camera.main.transform); 
            heldItem.transform.localPosition = new Vector3(0, 0, 1); // Adjust as needed
        }
    }

    void DropItem()
    {
        heldItem.layer = LayerMask.NameToLayer("Item");
        heldItem.transform.SetParent(null);
        heldItem = null;
    }
}
