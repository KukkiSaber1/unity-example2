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

    [Header("Snap Settings")]
    [Tooltip("Layer mask for snap zones")]
    public LayerMask snapZoneLayer;
    [Tooltip("Max distance to show snap preview / snap")]
    public float snapRange = 3f;
    [Tooltip("Optional screen-space UI Text to show 'Press E to Snap'")]
    public Text interactText;
    [Tooltip("Optional material used for ghost preview when SnapZone creates a ghost from the held item")]
    public Material ghostMaterial;

    [Header("Auto Snap On Drop")]
public bool autoSnapOnDrop = true;
public float autoSnapRadius = 1f; // radius to search for snap zones when dropping


    private SnapZone lookedSnapZone;
    private string cameraDefaultTag;

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

        if (playerCamera != null)
            cameraDefaultTag = playerCamera.tag;
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
        UpdateSnapPreview();
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
            // If holding an eligible item and looking at a snap zone, snap it
            if (equippedItem != null && lookedSnapZone != null)
            {
                SnapHeldItemToZone(lookedSnapZone);
                return;
            }

            OnInteract?.Invoke();
        }
    }

    public void OnInteractUIButtonPressed()
    {
        if (equippedItem != null && lookedSnapZone != null)
        {
            SnapHeldItemToZone(lookedSnapZone);
            return;
        }

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

    // Try auto-snap before releasing to world
    if (autoSnapOnDrop)
    {
        Collider[] hits = Physics.OverlapSphere(equippedItem.transform.position, autoSnapRadius, snapZoneLayer);
        foreach (var c in hits)
        {
            SnapZone zone = c.GetComponentInParent<SnapZone>();
            if (zone == null) continue;
            Snappable sn = equippedItem.GetComponent<Snappable>();
            if (sn == null || !sn.IsAllowedForZone(zone) || zone.isOccupied) continue;
            if (zone.requireReleaseInside && !zone.IsObjectInside(equippedItem)) continue;

            // Snap and return (SnapHeldItemToZone uses equippedItem)
            SnapHeldItemToZone(zone);
            return;
        }
    }

    // No auto-snap found — perform normal drop
    if (useParticle != null && useParticle.isPlaying) useParticle.Stop();

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

    if (lookedSnapZone != null)
    {
        lookedSnapZone.HidePreview();
        lookedSnapZone = null;
    }

    if (interactText != null) interactText.enabled = false;
}


    // Show/hide preview and prompt while looking at a SnapZone
    void UpdateSnapPreview()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, snapRange, snapZoneLayer))
        {
            SnapZone zone = hit.collider.GetComponentInParent<SnapZone>();
            if (zone != null && equippedItem != null)
            {
                Snappable sn = equippedItem.GetComponent<Snappable>();
                if (sn != null && sn.IsAllowedForZone(zone) && !zone.isOccupied)
                {
                    // pass ghost material if you want the zone to use it
                    if (ghostMaterial != null)
                        zone.ghostMaterial = ghostMaterial;

                    zone.ShowPreview(equippedItem);
                    lookedSnapZone = zone;

                    if (interactText != null)
                    {
                        interactText.text = "Press E to Snap";
                        interactText.enabled = true;
                    }

                    return;
                }
            }
        }

        // nothing valid under the crosshair
        if (lookedSnapZone != null)
        {
            lookedSnapZone.HidePreview();
            lookedSnapZone = null;
        }

        if (interactText != null)
            interactText.enabled = false;
    }

    void SnapHeldItemToZone(SnapZone zone)
    {
        if (equippedItem == null || zone == null) return;

        Snappable sn = equippedItem.GetComponent<Snappable>();
        if (sn == null || !sn.IsAllowedForZone(zone)) return;

        // restore layer/tag before snapping
        equippedItem.tag = "Snapped";
        equippedItem.layer = defaultLayer;

        zone.SnapObject(equippedItem);

        // clear equipped state
        equippedItem = null;
        isUsing = false;

        if (playerCamera != null)
            playerCamera.tag = cameraDefaultTag;

        if (equipParticle != null)
            equipParticle.Stop();

        if (interactText != null)
            interactText.enabled = false;

        lookedSnapZone = null;
    }
}
