using System.Collections;
using UnityEngine;

public class FireCollisionHandler : MonoBehaviour
{
    [Header("Particle Systems")]
    [Tooltip("Drag your fire Particle System here")]
    public ParticleSystem fireSystem;
    [Tooltip("Drag your smoke Particle System here")]
    public ParticleSystem smokeSystem;
    [Tooltip("Drag the other Particle System (e.g., water) here")]
    public ParticleSystem otherSystem;

    [Header("Collision Settings")]
    [Tooltip("Minimum interval between collision reactions (seconds)")]
    public float collisionCooldown = 1f;

    [Header("Shrink Settings")]
    [Tooltip("Normal shrink speed (units per second)")]
    public float normalShrinkRate = 0.1f;
    [Tooltip("Fast shrink speed when hit (units per second)")]
    public float hitShrinkRate = 1f;
    [Tooltip("Minimum scale before the particle dies")]
    public float minScale = 0.05f;
    [Tooltip("Duration of fast shrink after hit (seconds)")]
    public float fastShrinkDuration = 0.5f;

    private float lastCollisionTime = -Mathf.Infinity;
    private bool isFastShrinking;
    private Transform fireTrans;
    private Transform smokeTrans;

    void Start()
    {
        if (fireSystem == null || smokeSystem == null || otherSystem == null)
            Debug.LogWarning("Assign all three Particle Systems in the Inspector");

        fireTrans = fireSystem.transform;
        smokeTrans = smokeSystem.transform;
    }

    void Update()
    {
        float rate = isFastShrinking ? hitShrinkRate : normalShrinkRate;
        float delta = rate * Time.deltaTime;
        Vector3 shrink = Vector3.one * delta;

        fireTrans.localScale  = Vector3.Max(Vector3.one * minScale, fireTrans.localScale  - shrink);
        smokeTrans.localScale = Vector3.Max(Vector3.one * minScale, smokeTrans.localScale - shrink);

        if (fireTrans.localScale.x <= minScale && smokeTrans.localScale.x <= minScale)
            Destroy(gameObject);
    }

    void OnParticleCollision(GameObject other)
    {
        // Only react if the colliding system matches the one you dragged in
        if (Time.time >= lastCollisionTime + collisionCooldown)
        {
            ParticleSystem ps = other.GetComponent<ParticleSystem>();
            if (ps == otherSystem)
            {
                lastCollisionTime = Time.time;
                StartCoroutine(DoFastShrink());
            }
        }
    }

    IEnumerator DoFastShrink()
    {
        isFastShrinking = true;
        yield return new WaitForSeconds(fastShrinkDuration);
        isFastShrinking = false;
    }
}
