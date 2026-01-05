using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ParticleSystem))]
public class FireHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth = 50f;

    [Header("Size Scaling")]
    public float minSize = 0.2f;
    public float maxSize = 3f;
    [Tooltip("Particle system whose startSize will be driven by health.")]
    public ParticleSystem fireParticle;

    [Header("Events")]
    public UnityEvent<float> onHealthChanged;    // passes new health when decreased (subject to cooldown)
    public UnityEvent onFireExtinguished;        // health == 0
    public UnityEvent onFireMaxed;               // health == max
    public UnityEvent onFireHalfHealth;          // health crosses half

    [Header("Cooldown")]
    [Tooltip("Minimum seconds between consecutive onHealthChanged invocations when health decreases.")]
    public float healthChangeCooldown = 3f;

    [Header("Damage From Objects / Particles")]
    [Tooltip("Tag of objects that should damage the fire when they hit or enter the fire collider.")]
    public string damagingTag = "Water";
    [Tooltip("Amount of health to subtract when a damaging object/particle hits the fire.")]
    public float damageAmount = 20f;
    [Tooltip("Minimum seconds between particle damage applications to avoid spam.")]
    public float damageCooldown = 0.5f;

    private ParticleSystem.MainModule mainModule;
    private bool wasAboveHalf = true;            // track last state
    private float lastHealthDecreaseEventTime = -Mathf.Infinity;

    // For preventing repeated damage from the same object while it stays inside
    private HashSet<int> objectsInside = new HashSet<int>();
    private float lastParticleDamageTime = -Mathf.Infinity;

    void Awake()
    {
        if (fireParticle == null)
            fireParticle = GetComponent<ParticleSystem>();

        mainModule = fireParticle.main;
    }

    void Start()
    {
        // Clamp and immediately apply visual scale
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        ApplyScale();

        // Initialize half-health state
        wasAboveHalf = currentHealth >= maxHealth * 0.5f;
    }

    /// <summary>
    /// Change fire health by amount (can be negative). 
    /// Triggers scaling and events.
    /// </summary>
    public void ModifyHealth(float amount)
    {
        float previous = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);

        // Only react if value actually changed
        if (Mathf.Approximately(previous, currentHealth))
            return;

        // Invoke onHealthChanged only when health decreased and cooldown elapsed
        if (currentHealth < previous)
        {
            if (Time.time - lastHealthDecreaseEventTime >= healthChangeCooldown)
            {
                onHealthChanged?.Invoke(currentHealth);
                lastHealthDecreaseEventTime = Time.time;
            }
        }

        ApplyScale();

        if (currentHealth <= 0f)
            onFireExtinguished?.Invoke();
        else if (currentHealth >= maxHealth)
            onFireMaxed?.Invoke();

        CheckHalfHealth(previous, currentHealth);
    }

    /// <summary>
    /// Scales the particle startSize based on health ratio.
    /// </summary>
    private void ApplyScale()
    {
        float ratio = (maxHealth > 0f) ? currentHealth / maxHealth : 0f;
        float size = Mathf.Lerp(minSize, maxSize, ratio);
        mainModule.startSize = size;
    }

    /// <summary>
    /// Fires event whenever health crosses the half threshold.
    /// </summary>
    private void CheckHalfHealth(float previous, float current)
    {
        float half = maxHealth * 0.5f;
        bool isNowAboveHalf = current >= half;

        // Detect crossing the threshold
        if (isNowAboveHalf != wasAboveHalf)
        {
            onFireHalfHealth?.Invoke();
            wasAboveHalf = isNowAboveHalf;
        }
    }

    void OnValidate()
    {
        // Keep sensible ranges in the editor
        maxHealth = Mathf.Max(0.0001f, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        healthChangeCooldown = Mathf.Max(0f, healthChangeCooldown);
        damageAmount = Mathf.Max(0f, damageAmount);
        damageCooldown = Mathf.Max(0f, damageCooldown);
    }

    // -------------------------
    // Collision / Trigger logic
    // -------------------------

    // If your fire collider is a trigger, this will catch objects entering it.
    void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        if (!string.IsNullOrEmpty(damagingTag) && other.CompareTag(damagingTag))
        {
            int id = other.GetInstanceID();
            if (!objectsInside.Contains(id))
            {
                objectsInside.Add(id);
                ModifyHealth(-damageAmount);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other == null) return;

        if (!string.IsNullOrEmpty(damagingTag) && other.CompareTag(damagingTag))
        {
            int id = other.GetInstanceID();
            if (objectsInside.Contains(id))
                objectsInside.Remove(id);
        }
    }

    // If your fire collider is NOT a trigger and you want to use physics collisions:
    void OnCollisionEnter(Collision collision)
    {
        if (collision == null || collision.gameObject == null) return;

        if (!string.IsNullOrEmpty(damagingTag) && collision.gameObject.CompareTag(damagingTag))
        {
            int id = collision.gameObject.GetInstanceID();
            if (!objectsInside.Contains(id))
            {
                objectsInside.Add(id);
                ModifyHealth(-damageAmount);
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision == null || collision.gameObject == null) return;

        if (!string.IsNullOrEmpty(damagingTag) && collision.gameObject.CompareTag(damagingTag))
        {
            int id = collision.gameObject.GetInstanceID();
            if (objectsInside.Contains(id))
                objectsInside.Remove(id);
        }
    }

    // Particle collisions: requires the particle system's Collision module to be enabled
    // and "Send Collision Messages" checked so Unity calls OnParticleCollision.
    void OnParticleCollision(GameObject other)
    {
        // 'other' is the GameObject that owns the particle system that hit this object.
        // We check its tag (or you can check other properties).
        if (other == null) return;

        if (!string.IsNullOrEmpty(damagingTag) && other.CompareTag(damagingTag))
        {
            if (Time.time - lastParticleDamageTime >= damageCooldown)
            {
                ModifyHealth(-damageAmount);
                lastParticleDamageTime = Time.time;
            }
        }
    }
}
