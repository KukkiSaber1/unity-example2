using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountdownTimer : MonoBehaviour
{
    public float timeRemaining = 540.0f; // 9 minutes (540 sec)
    public bool timerIsRunning = false;
    public Text timerText;
    public ParticleSystem fireParticle; // Assign in Inspector
    public float triggerTime = 480.0f; // 8 minutes (480 sec)
    public float fireGrowthRate = 0.1f; // How fast fire grows per second
    public float maxFireSize = 3.0f; // Max size of fire
    
    private bool hasTriggeredFire = false;
    private float fireActiveTime = 0f;
    private ParticleSystem.MainModule fireMain;
    private ParticleSystem.EmissionModule fireEmission;

    private void Start()
    {
        timerIsRunning = true;
        
        if (fireParticle != null)
        {
            fireMain = fireParticle.main;
            fireEmission = fireParticle.emission;
            fireParticle.Stop();
        }
    }

    private void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);

                // Trigger fire at 8 minutes
                if (timeRemaining <= triggerTime && !hasTriggeredFire)
                {
                    TriggerFireEffect();
                    hasTriggeredFire = true;
                }

                // If fire is active, make it grow over time
                if (fireParticle != null && fireParticle.isPlaying)
                {
                    fireActiveTime += Time.deltaTime;
                    GrowFireEffect();
                }
            }
            else
            {
                timeRemaining = 0;
                timerIsRunning = false;
                Debug.Log("Time has run out!");
            }
        }
    }

    private void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1; // Prevents showing 0:00 before stopping
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void TriggerFireEffect()
    {
        if (fireParticle != null)
        {
            // Start with small fire
            fireMain.startSize = 0.5f;
            fireEmission.rateOverTime = 10f;
            fireMain.startSpeed = 1f;
            
            fireParticle.Play();
            Debug.Log("Fire started small!");
        }
    }

    private void GrowFireEffect()
    {
        if (fireParticle.isPlaying)
        {
            // Increase size (capped at maxFireSize)
            float newSize = Mathf.Min(fireMain.startSize.constant + fireGrowthRate * Time.deltaTime, maxFireSize);
            fireMain.startSize = newSize;

            // Increase emission (more particles)
            float newEmission = fireEmission.rateOverTime.constant + fireGrowthRate * 5f * Time.deltaTime;
            fireEmission.rateOverTime = newEmission;

            // Increase speed (more intense)
            float newSpeed = fireMain.startSpeed.constant + fireGrowthRate * 0.5f * Time.deltaTime;
            fireMain.startSpeed = newSpeed;
        }
    }
}