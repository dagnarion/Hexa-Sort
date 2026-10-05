using UnityEngine;

public class LockFactory : MonoBehaviour
{
    [SerializeField] private ComponentPoolSO<TaskLock> taskLockPool;
    [SerializeField] private ComponentPoolSO<BreakLock> breakLockPool;
    public ILock CreateLock(LockType lockType,int target,Vector3 position)
    {
        ILock locks = null;
        switch (lockType)
        {
            case LockType.BreakLock:
                locks = breakLockPool.Get();
                locks.Init(target,position.With(y: position.y + .2f));
                break;
            case LockType.TaskLock:
                locks = taskLockPool.Get();
                locks.Init(target,position.With(y: position.y + .2f));
                break;
        }
        return locks;
    }
    
}