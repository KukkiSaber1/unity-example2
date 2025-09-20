using UnityEngine;

public class FireHealthManager : MonoBehaviour
{
    [Header("Regeneration Settings")]
    [Tooltip("Health added to each fire every effectInterval seconds.")]
    public float regenPerSecond = 2f;

    [Tooltip("Delay in seconds before regeneration begins.")]
    public float regenStartDelay = 5f;

    [Header("Damage Settings")]
    [Tooltip("Health subtracted from each fire every effectInterval seconds when overlapping player zone.")]
    public float damagePerSecond = 2f;

    [Header("Effect Interval (secs)")]
    [Tooltip("Time between each regen/damage tick. Higher values → slower, chunkier changes.")]
    public float effectInterval = 1f;

    [Header("Collision Zones")]
    [Tooltip("Assign the invisible collider in front of the camera here.")]
    public Collider playerDamageZone;

    [Tooltip("If true, auto-finds all FireHealth in scene on Start.")]
    public bool autoRegisterFires = true;
    [Tooltip("If autoRegisterFires is false, drag-drop fire instances here.")]
    public FireHealth[] manualFires;

    private FireHealth[] fires;
    private float regenTimer;
    private float damageTimer;

    // New fields for regen delay
    private float regenStartTimer;
    private bool canRegen;

    void Start()
    {
        if (autoRegisterFires)
            fires = FindObjectsOfType<FireHealth>();
        else
            fires = manualFires;

        regenTimer       = effectInterval;
        damageTimer      = effectInterval;
        regenStartTimer  = 0f;
        canRegen         = false;
    }

    void Update()
    {
        if (fires == null || fires.Length == 0 || effectInterval <= 0f)
            return;

        float dt = Time.deltaTime;
        regenStartTimer += dt;
        damageTimer     += dt;

        // Enable regeneration after the initial delay
        if (!canRegen && regenStartTimer >= regenStartDelay)
            canRegen = true;

        // Regeneration tick (only once canRegen is true)
        if (canRegen)
        {
            regenTimer += dt;
            if (regenTimer >= effectInterval)
            {
                float amount = regenPerSecond * regenTimer;
                foreach (var fh in fires)
                    if (fh.currentHealth < fh.maxHealth)
                        fh.ModifyHealth(amount);

                regenTimer = 0f;
            }
        }

        // Damage tick (only if player zone overlaps fire collider)
        if (playerDamageZone != null && damageTimer >= effectInterval)
        {
            foreach (var fh in fires)
            {
                var col = fh.GetComponent<Collider>();
                if (col != null && col.bounds.Intersects(playerDamageZone.bounds))
                {
                    fh.ModifyHealth(-damagePerSecond * damageTimer);

                    // Optional: reset regen delay on damage
                    // regenStartTimer = 0f;
                    // canRegen = false;
                }
            }

            damageTimer = 0f;
        }
    }
}
