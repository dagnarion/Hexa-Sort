using UnityEngine;

[CreateAssetMenu(menuName = "Pool/TaskLock")]
public class TaskLockPool : ComponentPoolSO<TaskLock>
{
    protected override void OnTakeFromPool(TaskLock instance)
    {
        instance.SetPool(this);
        base.OnTakeFromPool(instance);
    }
}