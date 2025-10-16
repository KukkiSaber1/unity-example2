using UnityEngine;
using UnityEngine.UI;

public class FireLifeUI : MonoBehaviour
{
    [SerializeField] private FireBehavior fireBehavior;
    [SerializeField] private Slider fireLifeSlider;
    [SerializeField] private GameObject winMessage;

    void Awake()
    {
        fireBehavior.OnFireLifeChanged += UpdateFireLifeSlider;
        fireBehavior.OnFireExtinguished += OnFireExtinguished;
    }

    private void UpdateFireLifeSlider(float fireLife)
    {
        fireLifeSlider.value = fireLife;
    }

    private void OnFireExtinguished()
    {
        fireLifeSlider.gameObject.SetActive(false);
        winMessage.SetActive(true);
    }

    void OnDestroy()
    {
        fireBehavior.OnFireLifeChanged -= UpdateFireLifeSlider;
        fireBehavior.OnFireExtinguished -= OnFireExtinguished;
    }
}
