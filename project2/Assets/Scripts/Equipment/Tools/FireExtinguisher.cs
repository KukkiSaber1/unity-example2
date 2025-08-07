using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// FireExtinguisher.cs
public class FireExtinguisher : BaseTool
{
    [Header("Extinguisher Settings")]
    public ParticleSystem foamParticles;
    public float foamDistance = 5f;
    public float foamRate = 0.5f;
    public Transform nozzleExit;
    
    private bool isSpraying;
    private float nextFoamTime;
    
    private void Awake()
    {
        toolName = "Fire Extinguisher";
        if (foamParticles != null) foamParticles.Stop();
    }
    
    public override void OnPrimaryAction(bool isPressed)
    {
        isSpraying = isPressed;
        
        if (isPressed)
        {
            StartSpraying();
        }
        else
        {
            StopSpraying();
        }
    }
    
    public override void OnSecondaryAction(bool isPressed)
    {
        // Could implement secondary functions like nozzle adjustment
    }
    
    private void Update()
    {
        if (isSpraying)
        {
            SprayFoam();
        }
    }
    
    private void StartSpraying()
    {
        if (foamParticles != null)
        {
            foamParticles.Play();
        }
    }
    
    private void StopSpraying()
    {
        if (foamParticles != null)
        {
            foamParticles.Stop();
        }
    }
    
    private void SprayFoam()
    {
        if (Time.time >= nextFoamTime)
        {
            // Raycast to detect what we're spraying
            RaycastHit hit;
            if (Physics.Raycast(nozzleExit.position, nozzleExit.forward, out hit, foamDistance))
            {
                // Handle foam interaction with objects
                Debug.Log("Spraying " + hit.collider.gameObject.name);
                
                // Example: Extinguish fire if hit object is flammable
                IFflammable flammable = hit.collider.GetComponent<IFflammable>();
                if (flammable != null)
                {
                    flammable.Extinguish(foamRate);
                }
            }
            
            nextFoamTime = Time.time + (1f / foamRate);
        }
    }
}
