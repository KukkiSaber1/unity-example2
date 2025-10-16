using UnityEngine;
using UnityEngine.UI;

public class ExtinguisherHeightController : MonoBehaviour
{
    public Transform extinguisher;
    public Slider heightSlider;   
    public float minHeight = 0.0f;
    public float maxHeight = 5.0f; 

    void Start()
    {
        if (heightSlider != null)
        {
            heightSlider.onValueChanged.AddListener(AdjustExtinguisherHeight);
        }
    }

    private void AdjustExtinguisherHeight(float sliderValue)
    {
        if (extinguisher != null)
        {
            float height = Mathf.Lerp(minHeight, maxHeight, sliderValue);
            extinguisher.position = new Vector3(extinguisher.position.x, height, extinguisher.position.z);
        }
    }

    void OnDestroy()
    {
        if (heightSlider != null)
        {
            heightSlider.onValueChanged.RemoveListener(AdjustExtinguisherHeight);
        }
    }
}
