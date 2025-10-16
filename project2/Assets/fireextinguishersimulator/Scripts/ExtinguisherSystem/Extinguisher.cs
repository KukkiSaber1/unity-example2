using System;
using UnityEngine;

public class Extinguisher : MonoBehaviour
{
    [SerializeField] private FireBehavior targetFire;
    [SerializeField] private ParticleSystem extinguishingParticleSystem;
    [SerializeField] private AnimationController tipCameraAnimation;
    [SerializeField] private GameObject tipUI;
    [SerializeField] private GameObject heightSlider;
    [SerializeField] private GameObject cameraTipUI;
    [SerializeField] private AudioSource pinSound;
    [SerializeField] private AudioClip clip;
    [SerializeField] private AudioClip clipPowder;
    [HideInInspector] public bool isSound = false;
    [HideInInspector] public bool isSafetyPinRemoved = false;
    [HideInInspector] public bool isNozzleAvailable = false;
    private bool isExtinguishing = false;
    private float extinguisherPowderRemaining = 10.0f;

    public event Action<float> OnPowderRemainingChanged;

    void Update()
    {
        if (isExtinguishing)
        {
            if (pinSound != null && !pinSound.isPlaying)
            {
                pinSound.PlayOneShot(clipPowder);
            }
            extinguisherPowderRemaining -= Time.deltaTime;
            if (extinguisherPowderRemaining <= 0.0f)
            {
                extinguisherPowderRemaining = 0.0f;
                StopExtinguishing(); 
            }

            OnPowderRemainingChanged?.Invoke(extinguisherPowderRemaining); 

            if (Input.GetMouseButtonUp(0))
            {
                StopExtinguishing();
            }
        }
        if (isSafetyPinRemoved)
        {
            tipCameraAnimation.PlayAnimation("CameraTip2");
            if (pinSound != null && !pinSound.isPlaying && !isSound)
            {
                pinSound.PlayOneShot(clip);
                isSound = true;
            }
        }
        if (isNozzleAvailable)
        {
            tipUI.SetActive(true);
            heightSlider.SetActive(true);
            cameraTipUI.SetActive(false);
        }
    }

    public void HandleClicked()
    {
        if (!isExtinguishing && isSafetyPinRemoved && extinguisherPowderRemaining > 0.0f)
        {
            StartExtinguishing();
        }
    }

    private void StartExtinguishing()
    {
        isExtinguishing = true;
        targetFire.StartExtinguishing();
        extinguishingParticleSystem.Play();
    }

    private void StopExtinguishing()
    {
        pinSound.Stop();
        isExtinguishing = false;
        targetFire.StopExtinguishing();
        extinguishingParticleSystem.Stop();
    }

    public float GetExtinguisherPowderRemaining()
    {
        return extinguisherPowderRemaining;
    }
}
