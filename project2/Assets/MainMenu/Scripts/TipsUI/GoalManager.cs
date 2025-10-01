using UnityEngine;

public class GoalManager : MonoBehaviour
{
    [Header("Tip Popup Controller")]
    public TipPopupController tipPopup;  // Reference to your TipPopupController

    [Header("Goal Items")]
    [Tooltip("Drag and drop all required GameObjects here")]
    public GameObject[] requiredItems;  // Items to track

    private bool[] itemsCollected;      // Track collected state per item
    private int toolLayer;              // Layer index for "Tool"

    void Start()
    {
        toolLayer = LayerMask.NameToLayer("Tool");
        if (toolLayer == -1)
        {
            Debug.LogError("Layer 'Tool' not found! Please add it in Project Settings > Tags and Layers.");
        }

        itemsCollected = new bool[requiredItems.Length];
        for (int i = 0; i < itemsCollected.Length; i++)
            itemsCollected[i] = false;
    }

    void Update()
    {
        // Check each required item every frame
        for (int i = 0; i < requiredItems.Length; i++)
        {
            if (!itemsCollected[i])
            {
                GameObject item = requiredItems[i];

                // If item is destroyed or null, count as collected
                if (item == null)
                {
                    itemsCollected[i] = true;
                    Debug.Log($"Item {requiredItems[i]?.name ?? "Unknown"} destroyed or missing, counted as collected.");
                    CheckGoalCompletion();
                    continue;
                }

                // If item layer changed from "Tool", count as collected
                if (item.layer != toolLayer)
                {
                    itemsCollected[i] = true;
                    Debug.Log($"Item {item.name} layer changed from 'Tool' to '{LayerMask.LayerToName(item.layer)}', counted as collected.");
                    CheckGoalCompletion();
                }
            }
        }
    }

    /// <summary>
    /// Checks if all required items are collected and closes the tip popup if yes.
    /// </summary>
    private void CheckGoalCompletion()
    {
        foreach (bool collected in itemsCollected)
        {
            if (!collected)
                return; // Not all collected yet
        }

        // All items collected - complete the goal
        tipPopup.goalCompleted = true;
        tipPopup.TryCloseTip();
        Debug.Log("All goal items collected! Tip popup closed.");
    }
}
