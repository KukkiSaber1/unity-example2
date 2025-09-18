using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class EquipableItem : MonoBehaviour, IUsable
{
    public ParticleSystem useParticle;  // drag your particle here

    void Awake()
    {
        if (useParticle == null)
            useParticle = GetComponent<ParticleSystem>();
        useParticle.Stop();
    }

    public void OnUseStart()
    {
        if (!useParticle.isPlaying)
            useParticle.Play();
        // e.g. FireRaycast(), PlaySound(), etc.
    }

    public void OnUseStop()
    {
        if (useParticle.isPlaying)
            useParticle.Stop();
    }
}
