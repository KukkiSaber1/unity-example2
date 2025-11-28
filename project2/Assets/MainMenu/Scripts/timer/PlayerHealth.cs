using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isDead = false;

    [Header("UI")]
    public Slider healthSlider;

    [Header("Respawn")]
    public Transform respawnPoint;
    public float respawnDelay = 3f;

    [Header("Audio")]
    public AudioClip deathSound;
    private AudioSource _audioSource;

    [Header("Events")]
    [Tooltip("Invoked whenever health decreases (fires repeatedly for a duration).")]
    public UnityEvent<float> onHealthDecreased;

    [Tooltip("Invoked once when the player dies.")]
    public UnityEvent onDeath;

    [Header("Event Settings")]
    public float eventDuration = 3f;
    public float eventInterval = 0.5f;

    private Coroutine damageEventRoutine;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        _audioSource = GetComponent<AudioSource>();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        float previousHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateHealthUI();

        if (currentHealth < previousHealth)
        {
            if (damageEventRoutine != null)
                StopCoroutine(damageEventRoutine);

            damageEventRoutine = StartCoroutine(RepeatDamageEvent());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator RepeatDamageEvent()
    {
        float elapsed = 0f;
        while (elapsed < eventDuration && !isDead)
        {
            onHealthDecreased?.Invoke(currentHealth);
            yield return new WaitForSeconds(eventInterval);
            elapsed += eventInterval;
        }
        damageEventRoutine = null;
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
        if (isDead) return;

        isDead = true;
        Debug.Log("Player died!");

        // Stop the repeating damage event if running
        if (damageEventRoutine != null)
        {
            StopCoroutine(damageEventRoutine);
            damageEventRoutine = null;
        }

        // Invoke death event
        onDeath?.Invoke();

        GetComponent<FirstPersonMovement>().enabled = false;
        GetComponent<Jump>().enabled = false;
        GetComponent<Crouch>().enabled = false;

        if (deathSound != null && _audioSource != null)
            _audioSource.PlayOneShot(deathSound);

        if (TryGetComponent<Animator>(out var animator))
            animator.SetTrigger("Die");

        if (respawnPoint != null)
            Invoke(nameof(Respawn), respawnDelay);
    }

    private void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        UpdateHealthUI();
        transform.position = respawnPoint.position;

        GetComponent<FirstPersonMovement>().enabled = true;
        GetComponent<Jump>().enabled = true;
        GetComponent<Crouch>().enabled = true;
    }
}
