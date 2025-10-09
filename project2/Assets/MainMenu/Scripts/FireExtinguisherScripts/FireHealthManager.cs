using UnityEngine;

public class FireHealthManager : MonoBehaviour
{
    [Header("Regeneration Settings")]
    [Tooltip("Health added to each fire every effectInterval seconds.")]
    public float regenPerSecond = 2f;

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

    // Per-fire activation tracking so we can apply regen when a fire becomes active
    private bool[] wasActive;
    private bool[] pendingImmediateRegen;

    void Start()
    {
        RegisterFires();

        // start timers at interval so first tick happens after effectInterval seconds
        regenTimer = effectInterval;
        damageTimer = effectInterval;
    }

    void RegisterFires()
    {
        if (autoRegisterFires)
            fires = FindObjectsOfType<FireHealth>();
        else
            fires = manualFires;

        int n = fires != null ? fires.Length : 0;
        wasActive = new bool[n];
        pendingImmediateRegen = new bool[n];

        for (int i = 0; i < n; i++)
        {
            bool active = IsFireActive(fires[i]);
            wasActive[i] = active;
            pendingImmediateRegen[i] = false;
        }
    }

    bool IsFireActive(FireHealth fh)
    {
        if (fh == null) return false;
        return fh.gameObject.activeInHierarchy && fh.enabled;
    }

    void Update()
    {
        // If fires were set to be found later or the scene changed, re-register
        if (fires == null || fires.Length == 0)
        {
            if (autoRegisterFires)
                RegisterFires();
            if (fires == null || fires.Length == 0 || effectInterval <= 0f)
                return;
        }

        // If the fires array has changed size (e.g., new/removed fires), re-register to keep arrays aligned
        if ((fires != null && wasActive != null && fires.Length != wasActive.Length) ||
            (fires != null && wasActive == null))
        {
            RegisterFires();
        }

        float dt = Time.deltaTime;
        regenTimer  += dt;
        damageTimer += dt;

        // Track activation changes and queue immediate regen for newly activated fires
        for (int i = 0; i < fires.Length; i++)
        {
            var fh = fires[i];
            bool activeNow = IsFireActive(fh);
            if (!wasActive[i] && activeNow)
            {
                // Fire was inactive and now active -> queue an immediate regen tick for this fire
                pendingImmediateRegen[i] = true;
            }
            wasActive[i] = activeNow;
        }

        // Regeneration tick (only for active fires)
        if (regenTimer >= effectInterval)
        {
            float amount = regenPerSecond * regenTimer;
            for (int i = 0; i < fires.Length; i++)
            {
                var fh = fires[i];
                if (fh == null) continue;

                if (IsFireActive(fh))
                {
                    if (fh.currentHealth < fh.maxHealth)
                        fh.ModifyHealth(amount);
                }
            }

            regenTimer = 0f;
        }

        // Apply pending immediate regen for fires that just activated
        // We apply the same magnitude as a single effect tick (regenPerSecond * effectInterval)
        for (int i = 0; i < fires.Length; i++)
        {
            if (!pendingImmediateRegen[i]) continue;

            var fh = fires[i];
            if (fh != null && IsFireActive(fh) && fh.currentHealth < fh.maxHealth)
            {
                float immediateAmount = regenPerSecond * effectInterval;
                fh.ModifyHealth(immediateAmount);
            }

            pendingImmediateRegen[i] = false;
        }

        // Damage tick (only if player zone overlaps fire collider)
        if (playerDamageZone != null && damageTimer >= effectInterval)
        {
            for (int i = 0; i < fires.Length; i++)
            {
                var fh = fires[i];
                if (fh == null) continue;

                var col = fh.GetComponent<Collider>();
                if (col != null && col.bounds.Intersects(playerDamageZone.bounds))
                {
                    fh.ModifyHealth(-damagePerSecond * damageTimer);
                }
            }

            damageTimer = 0f;
        }
    }
}
