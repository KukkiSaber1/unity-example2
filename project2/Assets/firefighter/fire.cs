using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fire : MonoBehaviour
{
    [SerializeField, Range(0f,1f)] private float currentIntensity = 1.0f; //1.0f is max intensity
    private float[] startIntensities = new float[0];
float timeLastWatered = 0; //if fire is not hit and it will regen
[SerializeField] private float regenDelay = 2.5f; //fire regeneration
[SerializeField] private float regenRate = .1f;
    [SerializeField] private ParticleSystem [] fireParticleSystems = new ParticleSystem[0];

    private bool isLit = true; //fire is alive

    private void Start()
    {
        startIntensities = new float[fireParticleSystems.Length];

        for (int i = 0; i < fireParticleSystems.Length; i++)
        {
            startIntensities[i] = fireParticleSystems[i].emission.rateOverTime.constant; //changing particle emission of fire
        }
    }



//for fire regeneration
    private void Update()
    {
        if (isLit && currentIntensity < 1.0f && Time.time - timeLastWatered >= regenDelay)
        {
            currentIntensity += regenRate * Time.deltaTime;
            ChangeIntensity();
        }
    }

//if extinguished
public bool TryExtinguish (float amount)
{
    timeLastWatered = Time.time;

    currentIntensity -= amount;

ChangeIntensity();

    if( currentIntensity <= 0) 
    {
        isLit = false;
        return true;
    }


    return false; //fire is still lit
}

    private void ChangeIntensity()
    {
        for (int i = 0; i < fireParticleSystems.Length; i++)
        {
        var emission = fireParticleSystems[i].emission;
        emission.rateOverTime = currentIntensity * startIntensities[i];
        }
        
    }
}
