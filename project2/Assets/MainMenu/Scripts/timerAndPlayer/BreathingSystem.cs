using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BreathingSystem : MonoBehaviour
{
    [Header("Breath Settings")]
    public int maxBreath = 100;
    public int currentBreath = 100;

    [Tooltip("Breath lost per second while in smoke (used by internal tick).")]
    public int breathLossRate = 2;

    [Tooltip("Breath regained per second when safe.")]
    public int regenRate = 1;

    [Tooltip("Health damage per second when breath is fully depleted.")]
    public int damageRate = 1;

    [Header("References")]
    public Slider breathBar;
    public PlayerHealth playerHealth;

    [Header("Events")]
    public UnityEvent<int> onBreathChanged;
    public UnityEvent onBreathDepleted;

    private bool inSmoke = false;
    private float tickTimer = 0f;

    void Start()
    {
        currentBreath = Mathf.Clamp(currentBreath, 0, maxBreath);
        UpdateUI();
    }

    void Update()
    {
        // Internal 1-second tick for regen / internal loss and health damage
        tickTimer += Time.deltaTime;
        if (tickTimer >= 1f)
        {
            if (inSmoke)
            {
                if (currentBreath > 0)
                {
                    ModifyBreath(-breathLossRate);
                }
                else
                {
                    onBreathDepleted?.Invoke();
                    if (playerHealth != null)
                        playerHealth.TakeDamage(damageRate);
                }
            }
            else
            {
                if (currentBreath < maxBreath)
                {
                    ModifyBreath(regenRate);
                }
            }
            tickTimer = 0f;
        }
    }

    private void UpdateUI()
    {
        if (breathBar != null)
            breathBar.value = (float)currentBreath / maxBreath;
    }

    /// <summary>
    /// Called by external systems (particle triggers, damage over time, etc.)
    /// Positive values restore breath, negative values reduce breath.
    /// Accepts fractional values; final breath is clamped and events fired.
    /// </summary>
    public void ModifyBreath(float amount)
    {
        // Convert to float then to int so we can accept fractional deltas
        float newBreathF = currentBreath + amount;
        int newBreath = Mathf.Clamp(Mathf.RoundToInt(newBreathF), 0, maxBreath);

        if (newBreath != currentBreath)
        {
            currentBreath = newBreath;
            onBreathChanged?.Invoke(currentBreath);
            UpdateUI();

            if (currentBreath <= 0)
            {
                onBreathDepleted?.Invoke();
            }
        }
    }

    /// <summary>
    /// Mark whether the player is currently inside smoke.
    /// Particle/trigger scripts should call this when particles start/stop overlapping the player.
    /// </summary>
    public void SetInSmoke(bool value)
    {
        inSmoke = value;
    }

    /// <summary>
    /// Optional helper to query smoke state.
    /// </summary>
    public bool IsInSmoke()
    {
        return inSmoke;
    }
}
