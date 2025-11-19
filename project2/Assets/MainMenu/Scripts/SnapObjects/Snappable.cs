using UnityEngine;

public class Snappable : MonoBehaviour
{
    [Tooltip("If false this object will never snap")]
    public bool canSnap = true;

    [Tooltip("Optional: tags of SnapZones this object can snap to. Leave empty to accept any.")]
    public string[] allowedZoneTags;

    public bool IsAllowedForZone(SnapZone zone)
    {
        if (!canSnap || zone == null) return false;
        if (allowedZoneTags == null || allowedZoneTags.Length == 0) return true;
        foreach (var t in allowedZoneTags)
            if (t == zone.acceptedTag) return true;
        return false;
    }
}
