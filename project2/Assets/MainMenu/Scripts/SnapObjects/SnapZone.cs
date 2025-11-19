using System;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

[RequireComponent(typeof(Collider))]
public class SnapZone : MonoBehaviour
{
    [System.Serializable]
    public class SnapEvent : UnityEvent<GameObject> {}

    [Header("Snap Settings")]
    public Transform snapPoint;
    public string acceptedTag = "Snappable";
    public bool isOccupied = false;
    public bool requireReleaseInside = true;
    public bool invokeOnlyOnce = false;

    [Header("Preview")]
    public GameObject phantomPrefab;
    public Material ghostMaterial;
    public TextMeshProUGUI promptText;

    [Header("Events")]
    public SnapEvent OnOccupied = new SnapEvent();
    public SnapEvent OnReleased = new SnapEvent();

    // Optional C# events for code subscribers
    public event Action<GameObject> OnOccupiedEvent;
    public event Action<GameObject> OnReleasedEvent;

    private Collider zoneCollider;
    private GameObject phantomInstance;

    void Reset()
    {
        if (snapPoint == null)
        {
            GameObject go = new GameObject("SnapPoint");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            snapPoint = go.transform;
        }
    }

    void Start()
    {
        zoneCollider = GetComponent<Collider>();
        if (zoneCollider == null)
            Debug.LogWarning($"SnapZone {name} needs a Collider.");
    }

    public void ShowPreview(GameObject heldItem)
    {
        if (isOccupied) return;

        if (phantomInstance == null)
        {
            if (phantomPrefab != null)
                phantomInstance = Instantiate(phantomPrefab, snapPoint.position, snapPoint.rotation, transform);
            else if (heldItem != null)
                phantomInstance = CreateGhostFrom(heldItem);
            else
                return;
        }

        phantomInstance.transform.position = snapPoint.position;
        phantomInstance.transform.rotation = snapPoint.rotation;
        phantomInstance.SetActive(true);

        if (promptText != null)
        {
            promptText.text = "E or Drop";
            promptText.enabled = true;
        }
    }

    public void HidePreview()
    {
        if (phantomInstance != null)
            phantomInstance.SetActive(false);

        if (promptText != null)
            promptText.enabled = false;
    }

    GameObject CreateGhostFrom(GameObject source)
    {
        GameObject ghost = Instantiate(source, snapPoint.position, snapPoint.rotation, transform);

        foreach (var rb in ghost.GetComponentsInChildren<Rigidbody>()) Destroy(rb);
        foreach (var c in ghost.GetComponentsInChildren<Collider>()) Destroy(c);

        foreach (var m in ghost.GetComponentsInChildren<MonoBehaviour>()) m.enabled = false;

        if (ghostMaterial != null)
        {
            var rends = ghost.GetComponentsInChildren<Renderer>();
            foreach (var r in rends) r.material = ghostMaterial;
        }
        else
        {
            var rends = ghost.GetComponentsInChildren<Renderer>();
            foreach (var r in rends)
            {
                foreach (var mat in r.materials)
                {
                    Color c = mat.color;
                    c.a = Mathf.Min(c.a, 0.5f);
                    mat.color = c;
                }
            }
        }

        return ghost;
    }

    public bool IsObjectInside(GameObject obj)
    {
        if (zoneCollider == null || obj == null) return false;
        Vector3 closest = zoneCollider.ClosestPoint(obj.transform.position);
        return Vector3.Distance(closest, obj.transform.position) < 0.05f;
    }

    public void SnapObject(GameObject obj)
    {
        if (isOccupied || obj == null) return;

        obj.transform.SetParent(snapPoint);
        obj.transform.position = snapPoint.position;
        obj.transform.rotation = snapPoint.rotation;

        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        isOccupied = true;
        HidePreview();

        // Invoke UnityEvent and C# event
        OnOccupied?.Invoke(obj);
        OnOccupiedEvent?.Invoke(obj);

        if (invokeOnlyOnce && zoneCollider != null)
            zoneCollider.enabled = false;
    }

    public void UnsnapObject(GameObject obj)
    {
        if (!isOccupied || obj == null) return;

        obj.transform.SetParent(null);
        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        isOccupied = false;

        OnReleased?.Invoke(obj);
        OnReleasedEvent?.Invoke(obj);

        if (invokeOnlyOnce && zoneCollider != null)
            zoneCollider.enabled = true;
    }
}
