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

    public void SetPool(ComponentPoolSO<BreakLock> lockPool) => pool = lockPool;

    public void Init(int remain, Vector3 position, Quaternion rotation = default)
    {
        OnUnlocked = null;
        isUnlocking = false;
        transform.DOKill();
        transform.SetPositionAndRotation(position, rotation);
        transform.localScale = Vector3.one;
        this.remain = remain;
        lockRender.Init();
    }

    public async UniTask<bool> TakeHitAsync(int damage = 1)
    {
        if (remain <= 0 || isUnlocking) return true;
        remain = Mathf.Max(0, remain - damage);
        if (remain <= 0)
        {
            isUnlocking = true;
            await lockRender.OnHit(remain);
            await UnlockAsync();
            return true;
        }
        await lockRender.OnHit(remain);
        return false;
    }

    public async UniTask UnlockAsync()
    {
        isUnlocking = true;
        await lockRender.PlayUnlockEffect();
        if (this == null) return;
        OnUnlocked?.Invoke(this);
        ReleaseLock();
    }

    public void ReleaseLock()
    {
        OnUnlocked = null;
        isUnlocking = false;
        if (lockRender != null) lockRender.Init();
        transform.DOKill();
        if (pool != null) pool.Release(this);
        else Destroy(gameObject);
    }
}