using UnityEngine;
using UnityEngine.UI;

public class ExtinguisherPowderUI : MonoBehaviour
{
    public Extinguisher extinguisher;
    public Slider powderRemainingSlider;

    void Start()
    {
        extinguisher.OnPowderRemainingChanged += UpdatePowderRemainingSlider;
    }

    private void UpdatePowderRemainingSlider(float powderRemaining)
    {
        powderRemainingSlider.value = powderRemaining / 10.0f;
        if(powderRemainingSlider.value <= 0)
        {
            powderRemainingSlider.gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        extinguisher.OnPowderRemainingChanged -= UpdatePowderRemainingSlider;
    }
}
