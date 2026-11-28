using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public sealed class StackReleaseBurst : MonoBehaviour
{
    private StackReleaseBurstPool owner;
    private ParticleSystem sparks;

    private void Awake() => sparks = GetComponent<ParticleSystem>();

    public void Play(StackReleaseBurstPool pool, Vector3 position)
    {
        owner = pool;
        transform.position = position;
        sparks.Play(withChildren: true);
    }

    private void OnParticleSystemStopped()
    {
        owner?.Release(this);
    }
}
