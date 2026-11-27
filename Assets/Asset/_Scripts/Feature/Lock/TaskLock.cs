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
    private int generation;
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
        generation++;
        OnUnlocked = null;
        IsLocked = true;
        transform.DOKill();
        this.transform.SetPositionAndRotation(position, rotation);
        this.transform.localScale = Vector3.one;
        this.target = target;
        render.Lock(target);
    }

    public void SetStack(HexagonStack stack) => render.SetStack(stack);

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
        int unlockGeneration = generation;
        await render.PlayUnlockEffect();
        if (this == null || generation != unlockGeneration) return;
        OnUnlocked?.Invoke(this);
        ReleaseLock();
    }

    public void ReleaseLock()
    {
        generation++;
        IsLocked = false;
        OnUnlocked = null;
        if (render != null) render.ResetRender();
        transform.DOKill();
        if (pool != null) pool.Release(this);
        else Destroy(gameObject);
    }
}
