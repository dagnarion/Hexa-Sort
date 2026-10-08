using System;
using NaughtyAttributes;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [SerializeField] private EventChannel<int> OnHexagonCollected;
    [SerializeField] private EventChannel<int> TaskProcess;
    [SerializeField] private EventChannel<LevelData> LevelLoadEventChannel;
    [SerializeField] private GridController gridController;
    private int collected = 0;
    private int blockBreaked = 0;
    private int maxCollected = 0;
    private int maxBreaked = 0;

   private void OnEnable()
   {
       OnHexagonCollected.OnEventRaise += CollectHexagon;
       LevelLoadEventChannel.OnEventRaise += SetValue;
   }

   private void OnDisable()
   {
       OnHexagonCollected.OnEventRaise -= CollectHexagon;
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

    public bool IsWin()
    {
        return (collected >= maxCollected) && (blockBreaked >= maxBreaked);
    }

    public bool IsLose()
    {
        if (IsWin()) return false;
        return gridController.IsFull();
    }
    
}