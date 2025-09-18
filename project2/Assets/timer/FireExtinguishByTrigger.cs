using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(ParticleSystem))]
public class FireExtinguishByTrigger : MonoBehaviour
{
    [Header("Settings")]
    public float damagePerSecond = 2f;

    private ParticleSystem ps;
    private List<ParticleSystem.Particle> inside = new List<ParticleSystem.Particle>();

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void OnParticleTrigger()
    {
        // get particles that entered/stayed inside your registered colliders
        int count = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Inside, inside);

        if (count == 0)
            return;

        // iterate through all registered colliders
        foreach (var col in ps.trigger.GetCollider(0).GetComponentsInParent<Collider>())
        {
            var fire = col.GetComponentInParent<FireHealth>();
            if (fire != null)
            {
                fire.ModifyHealth(-damagePerSecond * Time.deltaTime);
            }
        }
    }
}
