using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleShooter : MonoBehaviour
{
    public ParticleSystem particleSystem;
    public float emissionDuration = 0.5f; // How long particles emit after click

    private bool isEmitting = false;
    private float emissionTimer = 0f;

    void Update()
    {
        // Check for mouse click
        if (Input.GetMouseButtonDown(0)) // 0 = left mouse button
        {
            StartEmission();
        }

        // Handle emission duration
        if (isEmitting)
        {
            emissionTimer += Time.deltaTime;
            if (emissionTimer >= emissionDuration)
            {
                StopEmission();
            }
        }
    }

    void StartEmission()
    {
        particleSystem.Play();
        isEmitting = true;
        emissionTimer = 0f;
    }

    void StopEmission()
    {
        particleSystem.Stop();
        isEmitting = false;
    }
    
    void ShootTowardsMouse()
{
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    particleSystem.transform.rotation = Quaternion.LookRotation(ray.direction);
    particleSystem.Play();
}
}
