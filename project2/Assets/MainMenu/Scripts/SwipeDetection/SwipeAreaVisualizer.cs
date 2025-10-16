using UnityEngine;
using UnityEngine.UI;

public class SwipeAreaVisualizer : MonoBehaviour
{
    [Header("Swipe Area Visual Settings")]
    public bool showVisualArea = true;
    public Color areaColor = new Color(0.2f, 0.8f, 1f, 0.3f);
    public Color borderColor = Color.cyan;
    
    private Image areaImage;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        areaImage = GetComponent<Image>();
        
        if (areaImage != null)
        {
            areaImage.color = showVisualArea ? areaColor : new Color(0, 0, 0, 0);
            
            // Add outline if needed
            if (showVisualArea)
            {
                var outline = gameObject.AddComponent<Outline>();
                outline.effectColor = borderColor;
                outline.effectDistance = new Vector2(2, 2);
            }
        }
    }

    // Call this to hide the visual area but keep functionality
    public void HideVisuals()
    {
        if (areaImage != null)
            areaImage.color = new Color(0, 0, 0, 0);
    }

    // Call this to show the visual area
    public void ShowVisuals()
    {
        if (areaImage != null)
            areaImage.color = areaColor;
    }
}