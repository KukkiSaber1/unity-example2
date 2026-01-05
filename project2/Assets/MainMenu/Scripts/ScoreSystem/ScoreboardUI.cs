using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardUI : MonoBehaviour
{
    [Header("Prefabs and container")]
    public GameObject rowPrefab;        
    public RectTransform contentParent; 

    [Header("Formats")]
    public string rowFormat = "{0}. {1} - {2} PTS - {3}";

    [Header("Optional visuals")]
    public Color evenColor = new Color(0.2f, 0.2f, 0.2f, 0.5f); // Semi-transparent grey
    public Color oddColor = new Color(0, 0, 0, 0);             // Transparent

    private void OnEnable() => Refresh();

    public void Refresh()
    {
        if (rowPrefab == null || contentParent == null) return;

        // 1. Clear existing children
        foreach (Transform child in contentParent) {
            Destroy(child.gameObject);
        }

        // 2. Load and Sort
        List<ScoreEntry> allScores = PersistentScores.Load()
            .OrderByDescending(e => e.score)
            .ToList();

        // 3. Create Rows
        for (int i = 0; i < allScores.Count; i++)
        {
            var entry = allScores[i];
            GameObject row = Instantiate(rowPrefab, contentParent);
            
            // Ensure scale is correct and it's active
            row.transform.localScale = Vector3.one;

            // Set Text
            var tmp = row.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
                tmp.text = string.Format(rowFormat, i + 1, entry.sceneName, entry.score, entry.timestamp);

            // Set Background Color
            var img = row.GetComponent<Image>();
            if (img != null)
                img.color = (i % 2 == 0) ? evenColor : oddColor;
        }

        // 4. THE FIX FOR OVERLAPPING: Force Unity to recalculate the layout immediately
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
    }

    public void ClearScores()
    {
        PersistentScores.Clear();
        Refresh();
    }
}
