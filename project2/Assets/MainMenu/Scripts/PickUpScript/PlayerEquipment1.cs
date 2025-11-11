using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

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
    private Transform itemSocket;
    private Transform particleSocket;

    [Header("Particle Effects")]
    public ParticleSystem equipParticle;
    public ParticleSystem useParticle;

    [Header("Socket Settings")]
    public Vector3 objectSocketPosition = new Vector3(0.9f, -0.4f, 1f);
    public Vector3 objectSocketRotation = new Vector3(30f, 180f, 0f);
    public Vector3 particleSocketPosition = new Vector3(0f, 0f, 1f);
    public Vector3 particleOffset = new Vector3(0f, 0f, 0f);

    [Header("UI Buttons (optional)")]
    [Tooltip("Drag a UI Button here to allow picking up / dropping via UI")]
    public Button pickupUIButton;
    [Tooltip("Drag a UI Button here to control using the equipped item")]
    public Button useUIButton;
    [Tooltip("Drag a UI Button here to trigger the Interact event (E key equivalent)")]
    public Button interactUIButton;

    [Header("Use Input Settings")]
    [Tooltip("If true, left mouse button will trigger use start/stop")]
    public bool useWithMouse = true;
    [Tooltip("If true, the UI Use button will only work when an item is equipped")]
    public bool useUIButtonRequiresEquipped = true;
    [Tooltip("If true the UI Use button will act as press/release, otherwise it toggles start/stop on click")]
    public bool useUIButtonPressRelease = false;

    [Header("Interaction")]
    [Tooltip("Key used for the Interact event (fires OnInteract)")]
    public KeyCode interactKey = KeyCode.E;
    public UnityEvent OnInteract;

    // internal state for UI toggle behavior
    private bool isUsing = false;

    void Start()
    {
        pickedUpLayer = LayerMask.NameToLayer(pickedUpLayerName);

        itemSocket = new GameObject("ItemSocket").transform;
        itemSocket.parent = playerCamera.transform;
        itemSocket.localPosition = objectSocketPosition;
        itemSocket.localRotation = Quaternion.Euler(objectSocketRotation);

        particleSocket = new GameObject("ParticleSocket").transform;
        particleSocket.parent = playerCamera.transform;
        particleSocket.localPosition = particleSocketPosition;
        particleSocket.localRotation = Quaternion.identity;

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

    void OnEnable()
    {
        if (pickupUIButton != null)
            pickupUIButton.onClick.AddListener(OnPickupUIButtonPressed);

        if (useUIButton != null)
            useUIButton.onClick.AddListener(OnUseUIButtonPressed);

        if (interactUIButton != null)
            interactUIButton.onClick.AddListener(OnInteractUIButtonPressed);
    }

    void OnDisable()
    {
        if (pickupUIButton != null)
            pickupUIButton.onClick.RemoveListener(OnPickupUIButtonPressed);

        if (useUIButton != null)
            useUIButton.onClick.RemoveListener(OnUseUIButtonPressed);

        if (interactUIButton != null)
            interactUIButton.onClick.RemoveListener(OnInteractUIButtonPressed);
    }

    void Update()
    {
        HandlePickup();
        HandleUseItem();
        HandleInteractInput();
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

    public void OnPickupUIButtonPressed()
    {
        if (equippedItem == null)
            TryPickupItem();
        else
            DropItem();
    }

    void HandleUseItem()
    {
        if (equippedItem != null)
        {
            if (useWithMouse)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    StartUse();
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    StopUse();
                }
            }
        }
    }

    public void OnUseUIButtonPressed()
    {
        if (useUIButtonRequiresEquipped && equippedItem == null) return;

        if (!useUIButtonPressRelease)
        {
            if (!isUsing) StartUse();
            else StopUse();
        }
        else
        {
            // fallback to toggle if press/release isn't wired via EventTrigger
            if (!isUsing) StartUse();
            else StopUse();
        }
    }

    public void StartUse()
    {
        if (equippedItem == null) return;

        if (useParticle != null && !useParticle.isPlaying)
            useParticle.Play();

        isUsing = true;
    }

    public void StopUse()
    {
        if (equippedItem == null) return;

        if (useParticle != null && useParticle.isPlaying)
            useParticle.Stop();

        isUsing = false;
    }

    // New: Interact input handling (E key or UI button)
    void HandleInteractInput()
    {
        if (Input.GetKeyDown(interactKey))
        {
            // Optionally check state here (e.g., only when equippedItem != null)
            OnInteract?.Invoke();
        }
    }

    public void OnInteractUIButtonPressed()
    {
        // Optionally check state here (e.g., only when equippedItem != null)
        OnInteract?.Invoke();
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
        defaultLayer = equippedItem.layer;

        equippedItem.tag = "PickedUp";
        equippedItem.layer = pickedUpLayer;

        playerCamera.tag = "HoldingItem";

        Rigidbody rb = equippedItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.None;
        }

        equippedItem.transform.SetParent(itemSocket);
        equippedItem.transform.localPosition = Vector3.zero;
        equippedItem.transform.localRotation = Quaternion.identity;
        equippedItem.transform.localRotation = Quaternion.Euler(objectSocketRotation);

        if (equipParticle != null)
        {
            equipParticle.Play();
        }
    }

    void DropItem()
    {
        if (equippedItem == null) return;

        if (useParticle != null && useParticle.isPlaying)
        {
            useParticle.Stop();
        }

        equippedItem.tag = "canPickUp";
        equippedItem.layer = defaultLayer;

        Rigidbody rb = equippedItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        equippedItem.transform.SetParent(null);
        equippedItem = null;
        isUsing = false;
    }
}
