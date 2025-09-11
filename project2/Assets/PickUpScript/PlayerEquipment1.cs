using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipment1 : MonoBehaviour
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
    private Transform itemSocket; // For holding the object
    private Transform particleSocket; // For particle effects
    
    [Header("Particle Effects")]
    public ParticleSystem equipParticle; // Assign via Inspector
    public ParticleSystem useParticle;   // Assign via Inspector

    [Header("Socket Settings")]
    public Vector3 objectSocketPosition = new Vector3(0.9f, -0.4f, 1f); // Original object position
    public Vector3 particleSocketPosition = new Vector3(0f, 0f, 1f); // Center for particles
    public Vector3 particleOffset = new Vector3(0f, 0f, 0f); // Adjust particle position

    void Start()
    {
        // Initialize the layer index
        pickedUpLayer = LayerMask.NameToLayer(pickedUpLayerName);
        
        // Create item socket for holding objects (original position)
        itemSocket = new GameObject("ItemSocket").transform;
        itemSocket.parent = playerCamera.transform;
        itemSocket.localPosition = objectSocketPosition;// new Vector3(0.9f, -0.4f, 1f);
        itemSocket.localRotation = Quaternion.identity;

        // Create separate socket for particles (center position)
        particleSocket = new GameObject("ParticleSocket").transform;
        particleSocket.parent = playerCamera.transform;
        particleSocket.localPosition = particleSocketPosition;
        particleSocket.localRotation = Quaternion.identity;
        
        // Parent particles to particle socket (center)
        if (equipParticle != null)
        {
            equipParticle.transform.SetParent(particleSocket);
            equipParticle.transform.localPosition = particleOffset;
            equipParticle.transform.localRotation = Quaternion.identity;
        }
        
        if (useParticle != null)
        {
            useParticle.transform.SetParent(particleSocket);
            useParticle.transform.localPosition = particleOffset;
            useParticle.transform.localRotation = Quaternion.identity;
            useParticle.Stop();
        }
    }
    
    void Update()
    {
        HandlePickup();
        HandleUseItem();
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
    
    void HandleUseItem()
    {
        if (equippedItem != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Start particle effect when mouse button is pressed
                if (useParticle != null)
                {
                    useParticle.Play();
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                // Stop particle effect when mouse button is released
                if (useParticle != null)
                {
                    useParticle.Stop();
                }
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

        // Change the camera's tag when picking up
        playerCamera.tag = "HoldingItem";

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

        // Play equip particle effect if assigned
        if (equipParticle != null)
        {
            equipParticle.Play();
        }
    }
    
    void DropItem()
    {
        if (equippedItem == null) return;
        
        // Stop use particle if it's playing
        if (useParticle != null && useParticle.isPlaying)
        {
            useParticle.Stop();
        }
        
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