using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    public Camera playerCamera;
    public float pickupDistance = 2f;
    public LayerMask pickupLayer;
    
    public enum EquipmentType { Weapon, Tool, Gadget }
    public EquipmentType currentEquipmentType;
    
    [Header("Layer Settings")]
    [Tooltip("Name of the layer to switch to when item is picked up (e.g., 'Ignore Raycast').")]
    public string pickedUpLayerName = "Ignore Raycast";
    private int pickedUpLayer;
    private int defaultLayer;

    private GameObject equippedItem;
    private Transform itemSocket;
    
    void Start()
    {
        // Initialize the layer index
        pickedUpLayer = LayerMask.NameToLayer(pickedUpLayerName);
        
        // Create item socket
        itemSocket = new GameObject("ItemSocket").transform;
        itemSocket.parent = playerCamera.transform;
        itemSocket.localPosition = new Vector3(0.9f, -0.4f, 1f);
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
        
        // Save the object's original layer
        defaultLayer = equippedItem.layer;
        
        // Change tag and layer
        equippedItem.tag = "PickedUp";
        equippedItem.layer = pickedUpLayer;
        
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
        
        // Revert tag and layer
        equippedItem.tag = "canPickUp";
        equippedItem.layer = defaultLayer;
        
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