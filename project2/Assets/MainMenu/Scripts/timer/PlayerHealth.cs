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

    [Header("Audio")]
    public AudioClip deathSound;
    private AudioSource _audioSource;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        _audioSource = GetComponent<AudioSource>(); // Moved here
    }

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
            healthSlider.value = currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player died!");

        // Disable controls
        GetComponent<FirstPersonMovement>().enabled = false;
        GetComponent<Jump>().enabled = false;
        GetComponent<Crouch>().enabled = false;

        // Play death sound (if assigned)
        if (deathSound != null && _audioSource != null)
            _audioSource.PlayOneShot(deathSound);

        // Trigger death animation (if Animator exists)
        if (TryGetComponent<Animator>(out var animator))
            animator.SetTrigger("Die");

        // Respawn
        if (respawnPoint != null)
            Invoke(nameof(Respawn), respawnDelay);
    }

    private void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        UpdateHealthUI();
        transform.position = respawnPoint.position;

        // Re-enable controls
        GetComponent<FirstPersonMovement>().enabled = true;
        GetComponent<Jump>().enabled = true;
        GetComponent<Crouch>().enabled = true;
    }
}