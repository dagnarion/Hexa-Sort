using System;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [SerializeField] private EventChannel<int> OnHexagonCollected;
    [SerializeField] private EventChannel<int> OnBlockBreaked;
    [SerializeField] private EventChannel<int> TaskProcess;
    [SerializeField] private EventChannel<LevelData> LevelLoadEventChannel;
    [SerializeField] private int collected;
    [SerializeField] private int blockBreaked;
    [SerializeField] private int maxCollected;
    [SerializeField] private int maxBreaked;
    private bool hasLevelData;
    public bool HasLevelData => hasLevelData;

   private void OnEnable()
   {
       OnHexagonCollected.OnEventRaise += CollectHexagon;
       OnBlockBreaked.OnEventRaise += Break;
       LevelLoadEventChannel.OnEventRaise += SetValue;
   }

   private void OnDisable()
   {
       OnHexagonCollected.OnEventRaise -= CollectHexagon;
       OnBlockBreaked.OnEventRaise -= Break;
       LevelLoadEventChannel.OnEventRaise -= SetValue;
   }

    public void SetValue(LevelData levelData)
    {
        collected = 0;
        blockBreaked = 0;
        maxBreaked = levelData.BreakBlockTarget;
        maxCollected = levelData.CollectHexagonTarget;
        hasLevelData = true;
        TaskProcess?.Raise(collected);
    }

    private void CollectHexagon(int amount)
    {
        collected += amount;
        TaskProcess?.Raise(collected);
    }

    private void Break(int amount)
    {
        blockBreaked += amount;
    }

    public bool IsTaskCompleted()
    {
        return collected >= Mathf.Max(0, maxCollected) && blockBreaked >= Mathf.Max(0, maxBreaked);
    }
}
