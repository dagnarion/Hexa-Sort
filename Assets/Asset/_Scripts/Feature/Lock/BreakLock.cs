using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class BreakLock : MonoBehaviour, IHittableLock
{
    [SerializeField] private BreakLockRender lockRender;
    public event Action<ILock> OnUnlocked;
    [SerializeField] private int remain;
    private ComponentPoolSO<BreakLock> pool;
    private bool isUnlocking;
    private bool unlockEffectStarted;
    private int poolVersion;

    public void SetPool(ComponentPoolSO<BreakLock> lockPool) => pool = lockPool;

    public void Init(int remain, Vector3 position, Quaternion rotation = default)
    {
        poolVersion++;
        OnUnlocked = null;
        isUnlocking = false;
        unlockEffectStarted = false;
        transform.DOKill();
        transform.SetPositionAndRotation(position, rotation);
        transform.localScale = Vector3.one;
        int initialStage = Mathf.Clamp(remain, 1, 3);
        this.remain = initialStage + 1;
        lockRender.Init(initialStage - 1);
    }

    public async UniTask<bool> TakeHitAsync(int damage = 1)
    {
        if (remain <= 0 || isUnlocking) return true;
        if (damage <= 0) return false;
        remain = Mathf.Max(0, remain - damage);
        if (remain <= 0)
        {
            isUnlocking = true;
            await UnlockAsync();
            return true;
        }
        await lockRender.OnHit(Mathf.Max(0, remain - 2));
        return false;
    }

    public async UniTask UnlockAsync()
    {
        if (unlockEffectStarted) return;
        unlockEffectStarted = true;
        isUnlocking = true;
        int version = poolVersion;
        await lockRender.PlayUnlockEffect();
        if (this == null || version != poolVersion) return;
        await TaskGoalSpawner.FlyPickups(TaskGoalKind.BreakBlocks, transform.position, Color.white, 1);
        if (this == null || version != poolVersion) return;
        OnUnlocked?.Invoke(this);
        ReleaseLock();
    }

    public void ReleaseLock()
    {
        poolVersion++;
        OnUnlocked = null;
        isUnlocking = false;
        unlockEffectStarted = false;
        if (lockRender != null) lockRender.Init();
        transform.DOKill();
        if (pool != null) pool.Release(this);
        else Destroy(gameObject);
    }
}
