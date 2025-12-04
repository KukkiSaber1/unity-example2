using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleDamageToBreath : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Breath points per second to subtract while particle overlaps the player.")]
    public float breathLossPerSecond = 2f;

    private ParticleSystem ps;
    private List<ParticleSystem.Particle> inside = new List<ParticleSystem.Particle>();

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void OnParticleTrigger()
    {
        // Get particles that are inside registered trigger colliders
        int count = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Inside, inside);

        int registered = ps.trigger.colliderCount;

        // If no particles are inside, clear smoke flag on registered colliders
        if (count == 0)
        {
            for (int i = 0; i < registered; i++)
            {
                // GetCollider returns Component, so cast to Collider safely
                Collider col = ps.trigger.GetCollider(i) as Collider;
                if (col == null) continue;

                var breath = col.GetComponentInParent<BreathingSystem>();
                if (breath != null)
                {
                    breath.SetInSmoke(false);
                }
            }
            return;
        }

        // There are particles inside. For each registered collider, apply damage if applicable.
        for (int i = 0; i < registered; i++)
        {
            Collider col = ps.trigger.GetCollider(i) as Collider;
            if (col == null) continue;

            var breath = col.GetComponentInParent<BreathingSystem>();
            if (breath != null)
            {
                // Mark the player as in smoke while particles are inside
                breath.SetInSmoke(true);

                // Apply continuous breath loss scaled by deltaTime
                float deltaLoss = breathLossPerSecond * Time.deltaTime;
                breath.ModifyBreath(-deltaLoss);
            }
        }
    }
}
