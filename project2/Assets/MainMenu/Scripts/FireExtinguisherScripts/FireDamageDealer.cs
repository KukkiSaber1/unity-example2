using UnityEngine;

public class FireDamageDealer : MonoBehaviour
{
    [Header("References")]
    public FireHealth fireHealth; // Link to the FireHealth script
    public LayerMask playerLayer;

    [Header("Damage Settings")]
    public float damagePerSecond = 10f;
    public float minDamageRadius = 1f;
    public float maxDamageRadius = 5f;

    [Header("Tick Settings")]
    [Tooltip("Seconds between damage ticks")]
    public float tickInterval = 1f; // 1-tick second

    private float tickTimer;

    private void Start()
    {
        tickTimer = 0f;
    }

    private void Update()
    {
        if (fireHealth == null || fireHealth.currentHealth <= 0)
            return;

        // accumulate time and apply damage only when a tick elapses
        tickTimer += Time.deltaTime;
        if (tickTimer < tickInterval)
            return;

        // consume ticks (handles cases where frame time > tickInterval)
        int ticksToProcess = Mathf.FloorToInt(tickTimer / tickInterval);
        tickTimer -= ticksToProcess * tickInterval;

        // Scale radius based on fire health percentage
        float healthPercent = fireHealth.currentHealth / fireHealth.maxHealth;
        float currentRadius = Mathf.Lerp(minDamageRadius, maxDamageRadius, healthPercent);

        // Detect players in range
        Collider[] hitPlayers = Physics.OverlapSphere(transform.position, currentRadius, playerLayer);
        foreach (var player in hitPlayers)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                // Apply damage for each tick processed
                float damagePerTick = damagePerSecond * tickInterval;
                health.TakeDamage(damagePerTick * ticksToProcess);
            }
        }
    }

    // Optional: Draw radius in Scene view for debugging
    private void OnDrawGizmosSelected()
    {
        if (fireHealth != null)
        {
            float healthPercent = fireHealth.currentHealth / fireHealth.maxHealth;
            float currentRadius = Mathf.Lerp(minDamageRadius, maxDamageRadius, healthPercent);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, currentRadius);
        }
    }
}
