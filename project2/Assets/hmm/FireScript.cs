using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireScript : MonoBehaviour
{
    public ParticleSystem smoke;
    public ParticleSystem fire;
    public ParticleSystem dissolve;

    public bool SmokeOnStart = false;
    public bool Dissolvable = true;

    public float SmokeDuration = 3;
    public float FireDuration = 5;

    public bool Smoking { get; private set; } = false;
    public bool Burning { get; private set; } = false;

    private Vector3 oldPosition;
    private float smokeStart;
    private float fireStart;

    // References to re-enable if you plan to reset/reuse this object
    private MeshRenderer mr;
    private SkinnedMeshRenderer smr;
    private Collider[] colliders;

    void Start()
    {
        oldPosition = transform.position;
        mr = GetComponent<MeshRenderer>();
        smr = GetComponentInChildren<SkinnedMeshRenderer>();
        colliders = GetComponentsInChildren<Collider>();

        if (SmokeOnStart) StartSmoke();
    }

    void Update()
    {
        if (transform.position != oldPosition)
        {
            float d = Vector3.Magnitude(transform.position - oldPosition);
            FireManager.Instance.OnBurnableMoved(this.gameObject, d);
            oldPosition = transform.position;
        }
    }

    // NEW: Extinguish fire when hit by water particles
    // Note: The water Particle System must have 'Collision' module enabled
    // Add this inside your FireScript class
private void OnParticleCollision(GameObject other)
{
    // You can check if the particle belongs to a "Water" object
    if (other.CompareTag("Water") || other.name.Contains("Extinguisher"))
    {
        StopBurning(); // Call the stop function we defined earlier
    }
}


    public void StartSmoke()
    {
        if (this.Burning) return;
        smokeStart = Time.time;
        this.StartCoroutine(Smoke(smokeStart));
    }

    public void StartFire()
    {
        fireStart = Time.time;
        this.StartCoroutine(Fire(fireStart));
    }

    public void StopBurning()
    {
        StopAllCoroutines(); // Stop the countdown to dissolve

        if (this.Smoking)
        {
            this.Smoking = false;
            if (smoke) smoke.Stop();
        }

        if (this.Burning)
        {
            this.Burning = false;
            FireManager.Instance.OnBurnStopped(this.gameObject);
            if (fire) fire.Stop();
        }
    }

    IEnumerator Smoke(float t)
    {
        this.Smoking = true;
        this.smoke.Play();
        yield return new WaitForSeconds(SmokeDuration);

        if (this.Smoking && t == smokeStart)
        {
            fireStart = Time.time;
            StartCoroutine(Fire(fireStart));
        }
    }

    IEnumerator Fire(float t)
    {
        this.Burning = true;
        this.fire.Play();
        FireManager.Instance.OnBurnStarted(this.gameObject);

        yield return new WaitForSeconds(0.5f);

        if (this.Smoking)
        {
            this.Smoking = false;
            this.smoke.Stop();
        }

        yield return new WaitForSeconds(FireDuration);

        if (this.Burning && t == fireStart && Dissolvable)
        {
            StartCoroutine(Dissolve());
        }
    }

    IEnumerator Dissolve()
    {
        if (!Dissolvable || !this.Burning) yield break;

        this.Burning = false;
        FireManager.Instance.OnBurnStopped(this.gameObject);
        this.fire.Stop();
        
        if(dissolve) dissolve.Play();

        // Disable visuals and physics
        if (mr) mr.enabled = false;
        if (smr) smr.enabled = false;
        foreach (Collider c in colliders) c.enabled = false;

        // Wait for dissolve effect to finish
        yield return new WaitForSeconds(3f);

        // MODIFIED: Deactivate instead of Destroy to prevent mobile lag
        gameObject.SetActive(false);
    }
}
