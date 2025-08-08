using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    public Camera playerCamera;
    public float pickupDistance = 3f;
    public LayerMask pickupLayer;
    
    public enum EquipmentType { Weapon, Tool, Gadget }
    public EquipmentType currentEquipmentType;
    
    private GameObject equippedItem;
    private Transform itemSocket;
    
    void Start()
    {
        itemSocket = new GameObject("ItemSocket").transform;
        itemSocket.parent = playerCamera.transform;
        itemSocket.localPosition = new Vector3(0.9f, -0.4f, 1.5f);
        itemSocket.localRotation = Quaternion.identity;
    }
    
    void Update()
    {
        HandlePickup();
    }
    
    void HandlePickup()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (equippedItem == null)
            {
                TryPickupItem();
            }
            else
            {
                DropItem();
            }
        }
    }
    
    void TryPickupItem()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, pickupDistance, pickupLayer))
        {
            if (hit.collider.CompareTag("canPickUp"))
            {
                EquipItem(hit.collider.gameObject);
            }
        }
    }
    
    void EquipItem(GameObject item)
    {
        equippedItem = item;
        
        // Change tag to "PickedUp"
        equippedItem.tag = "PickedUp";
        
        // Disable physics
        Rigidbody rb = equippedItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.None;
        }
        
        // Parent to socket
        equippedItem.transform.SetParent(itemSocket);
        equippedItem.transform.localPosition = Vector3.zero;
        equippedItem.transform.localRotation = Quaternion.identity;
    }
    
    void DropItem()
    {
        if (equippedItem == null) return;
        
        // Revert tag back to "canPickUp"
        equippedItem.tag = "canPickUp";
        
        // Re-enable physics
        Rigidbody rb = equippedItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        
        // Unparent and drop
        equippedItem.transform.SetParent(null);
        equippedItem = null;
    }
}