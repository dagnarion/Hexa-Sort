using UnityEngine;

[CreateAssetMenu(menuName = "Pool/BreakLock")]
public class BreakLockPool : ComponentPoolSO<BreakLock>
{
    protected override void OnTakeFromPool(BreakLock instance)
    {
        instance.SetPool(this);
        base.OnTakeFromPool(instance);
    }
}