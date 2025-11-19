using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class ZoneTask
{
    public SnapZone zone;
    public string taskName;
    public Text taskLabel; // UI text element for this task
}

public class SnapZoneManager : MonoBehaviour
{
    [Tooltip("List of zones and their associated tasks")]
    public List<ZoneTask> zoneTasks = new List<ZoneTask>();

    [Header("Events")]
    public UnityEvent OnAllZonesOccupied;
    public UnityEvent OnAnyZoneReleased;

    private int occupiedCount = 0;

    void Start()
    {
        foreach (var task in zoneTasks)
        {
            if (task.zone == null) continue;
            task.zone.OnOccupied.AddListener(obj => OnZoneOccupied(task));
            task.zone.OnReleased.AddListener(obj => OnZoneReleased(task));
            UpdateTaskLabel(task, false);
        }
    }

    void OnZoneOccupied(ZoneTask task)
    {
        occupiedCount++;
        UpdateTaskLabel(task, true);

        if (occupiedCount == zoneTasks.Count)
        {
            Debug.Log("All tasks complete!");
            OnAllZonesOccupied?.Invoke();
        }
    }

    void OnZoneReleased(ZoneTask task)
    {
        occupiedCount = Mathf.Max(0, occupiedCount - 1);
        UpdateTaskLabel(task, false);

        Debug.Log($"Task released: {task.taskName}");
        OnAnyZoneReleased?.Invoke();
    }

    void UpdateTaskLabel(ZoneTask task, bool isComplete)
    {
        if (task.taskLabel != null)
        {
            task.taskLabel.text = isComplete
                ? $"✔ {task.taskName}"
                : $"✘ {task.taskName}";
            task.taskLabel.color = isComplete ? Color.green : Color.red;
        }
    }
}
