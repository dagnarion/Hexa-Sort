using UnityEngine;

[CreateAssetMenu(menuName = "Pool/Stack Release Burst")]
public sealed class StackReleaseBurstPool : ComponentPoolSO<StackReleaseBurst>
{
    public void Play(Vector3 position)
    {
        Get().Play(this, position);
    }
}
