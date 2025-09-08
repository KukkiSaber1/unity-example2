using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ParticlePickupController : MonoBehaviour
{
    [SerializeField] private ParticleSystem particleSystemChild;

    void Awake()
    {
        // Auto-find the child ParticleSystem if none assigned
        if (particleSystemChild == null)
            particleSystemChild = GetComponentInChildren<ParticleSystem>();

        if (particleSystemChild != null)
            particleSystemChild.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        else
            Debug.LogWarning($"[{name}] No ParticleSystem found in children.");
    }

    void OnMouseDown()
    {
        particleSystemChild?.Play();
    }

    void OnMouseUp()
    {
        particleSystemChild?.Stop();
    }
}
