using UnityEngine;
using System;

public class FireBehavior : MonoBehaviour
{
    [SerializeField] private float fireLife = 1.0f;
    [SerializeField] private AudioSource fireAudio;
    private bool isBeingExtinguished = false;
    private bool isExtinguished = false; 
    private ParticleSystem fireParticleSystem;

    public event Action<float> OnFireLifeChanged;
    public event Action OnFireExtinguished;


    void Start()
    {
        fireParticleSystem = GetComponent<ParticleSystem>();
        UpdateFireVisual();
        OnFireLifeChanged?.Invoke(fireLife);
    }

    void Update()
    {
        if (isExtinguished) return;

        if (isBeingExtinguished)
        {
            fireLife -= Time.deltaTime / 6.0f;
        }
        else
        {
            fireLife += Time.deltaTime / 10.0f;
        }

        fireLife = Mathf.Clamp(fireLife, 0.0f, 1.0f);
        UpdateFireVisual();
        OnFireLifeChanged?.Invoke(fireLife);

        if (fireLife <= 0.0f && !isExtinguished)
        {
            ExtinguishFire();
        }
    }

    private void UpdateFireVisual()
    {
        var main = fireParticleSystem.main;
        main.startSize = Mathf.Lerp(3f, 17f, fireLife);
        if (fireLife <= 0.0f && fireParticleSystem.isPlaying)
        {
            fireParticleSystem.Stop();
            if (fireAudio != null && fireAudio.isPlaying)
            {
                fireAudio.Stop();
            }
        }
        else if (fireLife > 0.0f && !fireParticleSystem.isPlaying && !isExtinguished)
        {
            fireParticleSystem.Play();
        }
    }

    private void ExtinguishFire()
    {
        if (fireParticleSystem.isPlaying)
        {
            fireParticleSystem.Stop();
        }

        isExtinguished = true; 
        OnFireExtinguished?.Invoke();
    }

    public void StartExtinguishing()
    {
        if (!isExtinguished)
        {
            isBeingExtinguished = true;
        }
    }

    public void StopExtinguishing()
    {
        isBeingExtinguished = false;
    }
}
