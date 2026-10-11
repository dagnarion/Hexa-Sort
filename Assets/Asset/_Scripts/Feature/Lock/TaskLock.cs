using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class TaskLock : MonoBehaviour, ILock
{
    [SerializeField] private EventChannel<int> OnCountChange;
    [SerializeField] private TaskLockRender render;
    public event Action<ILock> OnUnlocked;
    private int target;
    private bool IsLocked;
    private ComponentPoolSO<TaskLock> pool;

    public void SetPool(ComponentPoolSO<TaskLock> lockPool) => pool = lockPool;

    private void OnEnable()
    {
        if (OnCountChange != null)
            OnCountChange.OnEventRaise += OnProgressUpdate;
    }

    private void OnDisable()
    {
        if (OnCountChange != null)
            OnCountChange.OnEventRaise -= OnProgressUpdate;
    }

    public void Init(int target, Vector3 position, Quaternion rotation = default)
    {
        OnUnlocked = null;
        IsLocked = true;
        transform.DOKill();
        this.transform.SetPositionAndRotation(position, rotation);
        this.transform.localScale = Vector3.one;
        this.target = target;
        render.Lock();
    }

    private void OnProgressUpdate(int currentAmount)
    {
        if (!IsLocked) return;
        if (currentAmount >= target)
        {
            UnlockAsync().Forget();
        }
    }

    public async UniTask UnlockAsync()
    {
        if (!IsLocked) return;
        IsLocked = false;
        await render.PlayUnlockEffect();
        if (this == null) return;
        OnUnlocked?.Invoke(this);
        ReleaseLock();
    }

    public void ReleaseLock()
    {
        IsLocked = false;
        OnUnlocked = null;
        if (render != null) render.ResetRender();
        transform.DOKill();
        if (pool != null) pool.Release(this);
        else Destroy(gameObject);
    }
}