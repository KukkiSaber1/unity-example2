using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class FireCollisionReducer : MonoBehaviour
{
    [Header("Collision Source")]
    [Tooltip("ParticleSystem whose particles will collide with this fire")]
    public ParticleSystem otherParticleSystem;

    [Header("Reduction Settings")]
    [Tooltip("How much to shrink the fire per collision event")]
    public float decayAmount = 0.05f;

    private ParticleSystem fireParticle;
    private ParticleSystem.MainModule fireMain;
    private List<ParticleCollisionEvent> collisionEvents;

    void Start()
    {
        // Cache references
        fireParticle = GetComponent<ParticleSystem>();
        fireMain = fireParticle.main;
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    // This is called whenever 'otherParticleSystem' particles hit this GameObject's collider
    void OnParticleCollision(GameObject hitObject)
    {
        // Only respond if the collision comes from the designated system
        if (otherParticleSystem != null && hitObject == otherParticleSystem.gameObject)
        {
            int events = otherParticleSystem.GetCollisionEvents(gameObject, collisionEvents);
            if (events > 0)
            {
                // Compute new size, clamped at zero
                float currentSize = fireMain.startSize.constant;
                float newSize = Mathf.Max(0f, currentSize - decayAmount * events);

                // Apply the reduced size immediately
                fireMain.startSize = newSize;
            }
        }
    }
}
