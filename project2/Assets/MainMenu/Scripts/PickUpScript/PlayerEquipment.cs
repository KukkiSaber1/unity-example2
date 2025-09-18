using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [Header("Pickup Settings")]
    public Camera playerCamera;
    public float pickupDistance = 2f;
    public LayerMask pickupLayer;
    public string triggerTag = "Player";

    private GameObject equippedItem;
    private Transform itemSocket;
    private int pickedUpLayer;
    private int defaultLayer;

    void Start()
    {
        pickedUpLayer = LayerMask.NameToLayer("Ignore Raycast");

        // Create a socket under the camera for all items
        itemSocket = new GameObject("ItemSocket").transform;
        itemSocket.SetParent(playerCamera.transform, false);
        itemSocket.localPosition = new Vector3(0.2f, -2f, 0.6f);
        itemSocket.localRotation = Quaternion.Euler(0f, 90f, 0f);
    }

    void Update()
    {
        HandlePickup();
        HandleUse();
    }

    void HandlePickup()
    {
        if (!Input.GetKeyDown(KeyCode.F)) return;

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

        if (Input.GetMouseButtonDown(0))
            usable.OnUseStart();
        else if (Input.GetMouseButtonUp(0))
            usable.OnUseStop();
    }

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
    }
}
