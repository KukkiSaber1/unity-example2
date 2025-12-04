using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardUI : MonoBehaviour
{
    [Header("Prefabs and container")]
    public GameObject groupPrefab;      // ScoreGroup prefab (see instructions)
    public GameObject rowPrefab;        // ScoreRow prefab (single row)
    public RectTransform contentParent; // ScrollView Content

    [Header("Formats")]
    public string headerFormat = "{0} ({1})";
    public string rowFormat = "{0}. {1} - {2} pts - {3}";

    [Header("Optional visuals")]
    public Color evenColor = new Color(0,0,0,0);
    public Color oddColor  = new Color(0,0,0,0);

    private void OnEnable() => Refresh();

    public void Refresh()
    {
        if (groupPrefab == null || rowPrefab == null || contentParent == null)
        {
            Debug.LogWarning("[ScoreboardUI] Assign groupPrefab, rowPrefab and contentParent.");
            return;
        }

        // Clear existing children
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        // Load and group scores
        List<ScoreEntry> all = PersistentScores.Load();
        var groups = all
            .GroupBy(e => e.sceneName)
            .OrderBy(g => g.Key)
            .Select(g => new { SceneName = g.Key, Entries = g.OrderByDescending(e => e.score).ToList() })
            .ToList();

        // If no scores, show a single header
        if (groups.Count == 0)
        {
            GameObject emptyGroup = Instantiate(groupPrefab, contentParent);
            emptyGroup.transform.localScale = Vector3.one;
            var headerTmp = emptyGroup.GetComponentInChildren<TextMeshProUGUI>();
            if (headerTmp != null) headerTmp.text = "No scores yet";
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
            return;
        }

        // Create a group for each scene
        foreach (var g in groups)
        {
            GameObject groupObj = Instantiate(groupPrefab, contentParent);
            groupObj.transform.localScale = Vector3.one;

            // Header text (first TextMeshProUGUI found is used)
            var headerTmp = groupObj.GetComponentInChildren<TextMeshProUGUI>();
            if (headerTmp != null)
                headerTmp.text = string.Format(headerFormat, g.SceneName, g.Entries.Count);

            // Find the GroupContent transform inside the group prefab
            // Expectation: groupPrefab has a child named "GroupContent" (or any child tagged)
            Transform groupContent = groupObj.transform.Find("GroupContent");
            if (groupContent == null)
            {
                // fallback: use groupObj as parent for rows (works if root VerticalLayoutGroup stacks header + rows)
                groupContent = groupObj.transform;
            }

            // Instantiate rows inside this group's content
            for (int i = 0; i < g.Entries.Count; i++)
            {
                var entry = g.Entries[i];
                GameObject row = Instantiate(rowPrefab, groupContent);
                row.transform.localScale = Vector3.one;

                var tmp = row.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = string.Format(rowFormat, i + 1, entry.sceneName, entry.score, entry.timestamp);
                }

                var img = row.GetComponent<Image>();
                if (img != null)
                {
                    bool useColors = (evenColor.a > 0f || oddColor.a > 0f) || (evenColor != Color.clear || oddColor != Color.clear);
                    if (useColors) img.color = (i % 2 == 0) ? evenColor : oddColor;
                }
            }
        }

        // Force layout rebuild so everything sizes and positions correctly
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
        Canvas.ForceUpdateCanvases();
    }

    public void ClearScores()
    {
        PersistentScores.Clear();
        Refresh();
    }
}
