using System;
using NaughtyAttributes;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [SerializeField] private EventChannel<int> OnHexagonCollected;
    [SerializeField] private EventChannel<int> OnBlockBreaked;
    [SerializeField] private EventChannel<int> TaskProcess;
    [SerializeField] private EventChannel<LevelData> LevelLoadEventChannel;
    [SerializeField] private GridController gridController;
    [SerializeField] private MergeService mergeService;
    [SerializeField] private int collected;
    [SerializeField] private int blockBreaked;
    [SerializeField] private int maxCollected;
    [SerializeField] private int maxBreaked;

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
   }

    [Button]
    private void Check()
    {
        Debug.Log("win: " + IsWin());
        Debug.Log("lose: " + IsLose());
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

    public bool IsWin()
    {
        return (collected >= maxCollected) && (blockBreaked >= maxBreaked);
    }

    public bool IsLose()
    {
        if (IsWin()) return false;
        return gridController.IsFull() && mergeService.IsFinishAllMerge();
    }
    
}