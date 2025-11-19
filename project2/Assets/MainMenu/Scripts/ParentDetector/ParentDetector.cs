using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Managers/Targeted Parent Change Detector")]
public class ParentDetector : MonoBehaviour
{
    [Tooltip("Drop specific objects to monitor for parent changes.")]
    public List<Transform> targets = new List<Transform>();

    [Tooltip("Event invoked when a target's parent changes. Sends the target that changed.")]
    public UnityEvent<Transform> onTargetParentChanged;

    // Internal: last known parent per target
    private readonly Dictionary<Transform, Transform> _lastParents = new Dictionary<Transform, Transform>();
    private bool _isActive;

    void OnEnable()
    {
        _isActive = true;
        InitializeTracking();
    }

    void OnDisable()
    {
        _isActive = false;
        _lastParents.Clear();
    }

    void Update()
    {
        if (!_isActive) return;
        if (targets == null || targets.Count == 0) return;

        // Guard against nulls or removed items
        for (int i = 0; i < targets.Count; i++)
        {
            var t = targets[i];
            if (t == null) continue;

            // If not tracked yet (e.g., added at runtime), seed parent
            if (!_lastParents.ContainsKey(t))
                _lastParents[t] = t.parent;

            var currentParent = t.parent;
            var previousParent = _lastParents[t];

            if (currentParent != previousParent)
            {
                _lastParents[t] = currentParent;
                onTargetParentChanged?.Invoke(t);
            }
        }
    }

    // Rebuild tracking state based on current list
    private void InitializeTracking()
    {
        _lastParents.Clear();
        if (targets == null) return;

        foreach (var t in targets)
        {
            if (t == null) continue;
            _lastParents[t] = t.parent;
        }
    }

    // Optional: call if you change the targets list at runtime
    public void RefreshTargets()
    {
        if (!_isActive) return;
        InitializeTracking();
    }
}
