using UnityEngine;

public class LockFactory : MonoBehaviour
{
    [SerializeField] private ComponentPoolSO<TaskLock> taskLockPool;
    [SerializeField] private ComponentPoolSO<BreakLock> breakLockPool;
    public ILock CreateLock(LockType lockType, int target, Vector3 position, Quaternion rotation = default, Transform parent = null)
    {
        ILock locks = null;
        switch (lockType)
        {
            case LockType.BreakLock:
                var breakLock = breakLockPool.Get();
                if (parent != null) breakLock.transform.SetParent(parent);
                breakLock.Init(target, position, rotation);
                locks = breakLock;
                break;
            case LockType.TaskLock:
                var taskLock = taskLockPool.Get();
                if (parent != null) taskLock.transform.SetParent(parent);
                taskLock.Init(target, position, rotation);
                locks = taskLock;
                break;
        }
        return locks;
    }
    
}