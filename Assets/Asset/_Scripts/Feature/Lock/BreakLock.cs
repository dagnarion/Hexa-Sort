using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BreakLock : MonoBehaviour,IHittableLock
{
    [SerializeField] private BreakLockRender lockRender;
    public event Action<ILock> OnUnlocked;
    [SerializeField] private int remain;
    
    public void Init(int remain,Vector3 position)
    {
        transform.position = position;
        this.remain = remain;
    }
    

    public async UniTask<bool> TakeHitAsync(int damage = 1)
    {
        if (remain <= 0) return true;
        remain = Mathf.Max(0, remain - damage);
        await lockRender.OnHit(remain);
        if (remain <= 0)
        {
            await UnlockAsync();
            return true;
        }
        return false;
    }
    
    public async UniTask UnlockAsync()
    {
        await lockRender.PlayUnlockEffect();
        OnUnlocked?.Invoke(this);
        Destroy(gameObject);
    }
}