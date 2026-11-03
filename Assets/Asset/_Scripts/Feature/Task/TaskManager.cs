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
    public event Action<int, int, int, int> ProgressChanged;

   private void OnEnable()
   {
       if (OnHexagonCollected != null) OnHexagonCollected.OnEventRaise += CollectHexagon;
       if (OnBlockBreaked != null) OnBlockBreaked.OnEventRaise += Break;
       if (LevelLoadEventChannel != null) LevelLoadEventChannel.OnEventRaise += SetValue;
   }

   private void OnDisable()
   {
       if (OnHexagonCollected != null) OnHexagonCollected.OnEventRaise -= CollectHexagon;
       if (OnBlockBreaked != null) OnBlockBreaked.OnEventRaise -= Break;
       if (LevelLoadEventChannel != null) LevelLoadEventChannel.OnEventRaise -= SetValue;
   }

    public void SetValue(LevelData levelData)
    {
        collected = 0;
        blockBreaked = 0;
        maxBreaked = Mathf.Max(0, levelData.BreakBlockTarget);
        maxCollected = Mathf.Max(0, levelData.CollectHexagonTarget);
        hasLevelData = true;
        TaskProcess?.Raise(collected);
        PublishProgress();
    }

    private void CollectHexagon(int amount)
    {
        if (amount <= 0) return;
        collected += amount;
        TaskProcess?.Raise(collected);
        PublishProgress();
    }

    private void Break(int amount)
    {
        if (amount <= 0) return;
        blockBreaked += amount;
        PublishProgress();
    }

    public bool IsTaskCompleted()
    {
        return hasLevelData && collected >= maxCollected && blockBreaked >= maxBreaked;
    }

    public bool TryGetProgress(out int collectedAmount, out int collectedTarget, out int brokenAmount, out int brokenTarget)
    {
        collectedAmount = collected;
        collectedTarget = maxCollected;
        brokenAmount = blockBreaked;
        brokenTarget = maxBreaked;
        return hasLevelData;
    }

    private void PublishProgress()
    {
        ProgressChanged?.Invoke(collected, maxCollected, blockBreaked, maxBreaked);
    }
}
