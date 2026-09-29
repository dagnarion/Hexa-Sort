using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TaskLock : MonoBehaviour, ILock
{
    [SerializeField] private EventChannel<int> OnCountChange;
    [SerializeField] private TaskLockRender render;
    public event Action<ILock> OnUnlocked;
    private int target;
    private bool IsLocked;
    private void OnEnable()
    {
        OnCountChange.OnEventRaise += OnProgressUpdate;
    }

    private void OnDisable()
    {
        OnCountChange.OnEventRaise -= OnProgressUpdate;
    }

    public void Init(int target,Vector3 position)
    {
        IsLocked = true;
        this.transform.position = position;
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
        IsLocked = false;
        await render.PlayUnlockEffect();
        OnUnlocked?.Invoke(this);
        Destroy(gameObject);
    }
}