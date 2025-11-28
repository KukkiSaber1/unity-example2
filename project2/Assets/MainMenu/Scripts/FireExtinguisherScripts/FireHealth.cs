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
    // This event is invoked only when health decreases and cooldown has elapsed.
    public UnityEvent<float> onHealthChanged;    // passes new health when decreased (subject to cooldown)
    public UnityEvent onFireExtinguished;        // health == 0
    public UnityEvent onFireMaxed;               // health == max
    public UnityEvent onFireHalfHealth;          // health crosses half

    [Header("Cooldown")]
    [Tooltip("Minimum seconds between consecutive onHealthChanged invocations when health decreases.")]
    public float healthChangeCooldown = 3f;

    private ParticleSystem.MainModule mainModule;
    private bool wasAboveHalf = true;            // track last state
    private float lastHealthDecreaseEventTime = -Mathf.Infinity;

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
    }
}
