using UnityEngine;

public enum TaskGoalKind
{
    CollectHexagons,
    BreakBlocks
}

public class TaskGoalIcon : MonoBehaviour
{
    public TaskGoalKind Kind { get; private set; }

    public void Initialize(TaskGoalKind kind)
    {
        Kind = kind;
    }
}
