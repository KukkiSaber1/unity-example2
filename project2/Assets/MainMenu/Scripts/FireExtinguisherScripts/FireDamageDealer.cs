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

    private void Update()
    {
        if (fireHealth == null || fireHealth.currentHealth <= 0)
            return;

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
                health.TakeDamage(damagePerSecond * Time.deltaTime);
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
