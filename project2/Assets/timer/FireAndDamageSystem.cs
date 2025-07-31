using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FireAndDamageSystem : MonoBehaviour
{
    [Header("Timer Settings")]
    public float timeRemaining = 540.0f; // 9 minutes
    public Text timerText;
    public float triggerTime = 480.0f; // 8 minutes

    [Header("Fire Settings")]
    public ParticleSystem fireParticle;
    public ParticleSystem smokeParticle;
    public float fireGrowthRate = 0.1f;
    public float maxFireSize = 3.0f;

    [Header("Damage Settings")]
    public float damagePerSecond = 10f;
    public LayerMask playerLayer;
    public float damageRadius = 2f;

    [Header("Fire Spread Settings")]
    public float spreadInterval = 5f;
    public float spreadRadius = 3f;
    public GameObject[] flammableObjects; // Assign in Inspector

    private bool timerIsRunning = true;
    private bool hasTriggeredFire = false;
    private float fireActiveTime = 0f;
    private float nextSpreadTime = 0f;
    private ParticleSystem.MainModule fireMain, smokeMain;
    private ParticleSystem.EmissionModule fireEmission, smokeEmission;
    private List<GameObject> activeFires = new List<GameObject>();

    private void Start()
    {
        if (fireParticle != null)
        {
            fireMain = fireParticle.main;
            fireEmission = fireParticle.emission;
            fireParticle.Stop();
        }
        if (smokeParticle != null)
        {
            smokeMain = smokeParticle.main;
            smokeEmission = smokeParticle.emission;
            smokeParticle.Stop();
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
                    ApplyDamageToPlayer();
                    TrySpreadFire();
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
        timeToDisplay += 1;
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void TriggerFireEffect()
    {
        if (fireParticle != null)
        {
            fireMain.startSize = 0.5f;
            fireEmission.rateOverTime = 10f;
            fireMain.startSpeed = 1f;
            fireParticle.Play();
            activeFires.Add(fireParticle.gameObject);
        }
        if (smokeParticle != null)
        {
            smokeMain.startSize = 0.3f;
            smokeEmission.rateOverTime = 5f;
            smokeParticle.Play();
        }
    }

    private void GrowFireEffect()
{
    // Grow fire size
    float newFireSize = Mathf.Min(fireMain.startSize.constant + fireGrowthRate * Time.deltaTime, maxFireSize);
    fireMain.startSize = newFireSize;

    // Increase emission rate (requires handling MinMaxCurve)
    ParticleSystem.MinMaxCurve emissionRate = fireEmission.rateOverTime;
    emissionRate.constant += fireGrowthRate * 5f * Time.deltaTime;
    fireEmission.rateOverTime = emissionRate;

    // Increase speed
    fireMain.startSpeed = fireMain.startSpeed.constant + fireGrowthRate * 0.5f * Time.deltaTime;

    // Grow smoke proportionally (same fix for smoke)
    if (smokeParticle != null)
    {
        smokeMain.startSize = newFireSize * 0.8f;
        ParticleSystem.MinMaxCurve smokeEmissionRate = smokeEmission.rateOverTime;
        smokeEmissionRate.constant = fireEmission.rateOverTime.constant * 0.7f;
        smokeEmission.rateOverTime = smokeEmissionRate;
    }
}

    private void ApplyDamageToPlayer()
    {
        Collider[] hitPlayers = Physics.OverlapSphere(fireParticle.transform.position, damageRadius, playerLayer);
        foreach (var player in hitPlayers)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damagePerSecond * Time.deltaTime);
            }
        }
    }

    private void TrySpreadFire()
    {
        if (Time.time >= nextSpreadTime && flammableObjects.Length > 0)
        {
            nextSpreadTime = Time.time + spreadInterval;
            GameObject randomObject = flammableObjects[Random.Range(0, flammableObjects.Length)];
            
            if (!activeFires.Contains(randomObject))
            {
                ParticleSystem newFire = Instantiate(fireParticle, randomObject.transform.position, Quaternion.identity);
                newFire.Play();
                activeFires.Add(newFire.gameObject);
            }
        }
    }
}