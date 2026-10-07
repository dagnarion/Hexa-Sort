using System;
using NaughtyAttributes;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [SerializeField] private EventChannel<int> OnHexagonCollected;
    [SerializeField] private EventChannel<int> TaskProcess;
    [SerializeField] private GridController gridController;
    private int collected = 0;
    private int blockBreaked = 0;
   [SerializeField] private int maxCollected = 0;
   [SerializeField] private int maxBreaked = 0;

   private void OnEnable()
   {
       OnHexagonCollected.OnEventRaise += CollectHexagon;
   }

   private void OnDisable()
   {
       OnHexagonCollected.OnEventRaise -= CollectHexagon;
   }

   [Button]
    public void ResetValue()
    {
        collected = 0;
        blockBreaked = 0;
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