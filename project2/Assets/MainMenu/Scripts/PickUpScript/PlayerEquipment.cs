using UnityEngine;
using UnityEngine.UI;

public class PlayerEquipment : MonoBehaviour
{
    [Header("Pickup Settings")]
    public Camera playerCamera;
    public float pickupDistance = 2f;
    public LayerMask pickupLayer;
    public string triggerTag = "Player";

    [Header("UI Buttons (optional)")]
    [Tooltip("Drag a UI Button here to allow picking up / dropping via UI")]
    public Button pickupUIButton;
    [Tooltip("Drag a UI Button here to control using the equipped item")]
    public Button useUIButton;

    [Header("Use Input Settings")]
    [Tooltip("If true, left mouse button will trigger use events")]
    public bool useWithMouse = true;
    [Tooltip("If true, the UI Use button will only work when an item is equipped")]
    public bool useUIButtonRequiresEquipped = true;

    private GameObject equippedItem;
    private Transform itemSocket;
    private int pickedUpLayer;
    private int defaultLayer;

    void Start()
    {
        pickedUpLayer = LayerMask.NameToLayer("Ignore Raycast");

        itemSocket = new GameObject("ItemSocket").transform;
        itemSocket.SetParent(playerCamera.transform, false);
        itemSocket.localPosition = new Vector3(0.2f, -2f, 0.6f);
        itemSocket.localRotation = Quaternion.Euler(0f, 90f, 0f);
    }

    void OnEnable()
    {
        if (pickupUIButton != null)
            pickupUIButton.onClick.AddListener(OnPickupUIButtonPressed);
        if (useUIButton != null)
            useUIButton.onClick.AddListener(OnUseUIButtonPressed);
    }

    void OnDisable()
    {
        if (pickupUIButton != null)
            pickupUIButton.onClick.RemoveListener(OnPickupUIButtonPressed);
        if (useUIButton != null)
            useUIButton.onClick.RemoveListener(OnUseUIButtonPressed);
    }

    void Update()
    {
        HandlePickup();
        HandleUse();
    }

    void HandlePickup()
    {
        // Keyboard fallback
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (equippedItem == null)
                TryPickup();
            else
                DropItem();
        }
    }

    // Public method for UI Button (auto-subscribed or can be wired manually)
    public void OnPickupUIButtonPressed()
    {
        if (equippedItem == null)
            TryPickup();
        else
            DropItem();
    }

    void HandleUse()
    {
        if (equippedItem == null) return;

        var usable = equippedItem.GetComponent<IUsable>();
        if (usable == null) return;

        // Mouse control
        if (useWithMouse)
        {
            if (Input.GetMouseButtonDown(0))
                usable.OnUseStart();
            else if (Input.GetMouseButtonUp(0))
                usable.OnUseStop();
        }
    }

    // Public method for UI Use Button (auto-subscribed or can be wired manually)
    public void OnUseUIButtonPressed()
    {
        if (useUIButtonRequiresEquipped && equippedItem == null) return;

        var usable = equippedItem != null ? equippedItem.GetComponent<IUsable>() : null;
        if (usable == null) return;

        // Toggle behavior: press triggers OnUseStart then OnUseStop on a subsequent press.
        // If you prefer press/release semantics, call OnUseStart on pointer down and OnUseStop on pointer up from the UI.
        if (!IsUsing)
        {
            usable.OnUseStart();
            IsUsing = true;
        }
        else
        {
            usable.OnUseStop();
            IsUsing = false;
        }
    }

    // Simple state so UI button toggles between start/stop
    private bool IsUsing = false;

    void TryPickup()
    {
        var ray = playerCamera.ViewportPointToRay(Vector3.one * 0.5f);
        if (Physics.Raycast(ray, out var hit, pickupDistance, pickupLayer)
            && hit.collider.CompareTag("canPickUp"))
        {
            Equip(hit.collider.gameObject);
        }
    }

    void Equip(GameObject item)
    {
        equippedItem = item;
        defaultLayer    = item.layer;
        item.tag        = "PickedUp";
        item.layer      = pickedUpLayer;

        var rb = item.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        item.transform.SetParent(itemSocket, false);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
    }

    void DropItem()
    {
        if (equippedItem == null) return;

        var usable = equippedItem.GetComponent<IUsable>();
        usable?.OnUseStop();

        equippedItem.tag   = "canPickUp";
        equippedItem.layer = defaultLayer;

        var rb = equippedItem.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        equippedItem.transform.SetParent(null);
        equippedItem = null;
        IsUsing = false;
    }
}
