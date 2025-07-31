using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isDead = false;

    [Header("UI (Optional)")]
    public Slider healthSlider; // Assign a UI Slider in Inspector

    [Header("Respawn (Optional)")]
    public Transform respawnPoint;
    public float respawnDelay = 3f;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    // Call this when the player takes damage (e.g., from fire)
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth / maxHealth; // Normalized (0-1)
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player died!");

        // Optional: Disable player controls
        GetComponent<FirstPersonMovement>().enabled = false;
        GetComponent<Jump>().enabled = false;
        GetComponent<Crouch>().enabled = false;

        // Optional: Trigger death animation
        GetComponent<Animator>().SetTrigger("Die");

        // Respawn after delay (if respawnPoint is set)
        if (respawnPoint != null)
        {
            Invoke(nameof(Respawn), respawnDelay);
        }
    }

    private void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        UpdateHealthUI();

        // Teleport to respawn point
        transform.position = respawnPoint.position;

        // Re-enable controls
        GetComponent<FirstPersonMovement>().enabled = true;
        GetComponent<Jump>().enabled = true;
        GetComponent<Crouch>().enabled = true;
    }
}