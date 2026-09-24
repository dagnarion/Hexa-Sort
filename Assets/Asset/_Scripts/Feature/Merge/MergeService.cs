using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MergeService : MonoBehaviour
{
    [SerializeField] private GridController gridController;
    [SerializeField] private EventChannel<Vector2Int> dropChannel;
    private SlotScoring slotScoring;
    private MergeChainResolver mergeChainResolver;
    private ConnectedSlotFinder connectedSlotFinder;
    private MergeHandler mergeHandler;
    private Queue<Vector2Int> mergeQueue = new Queue<Vector2Int>();
    private bool isRunning;
    private void OnEnable()
    {
        dropChannel.OnEventRaise += PushMergeCommand;
    }
    private void OnDisable()
    {
        dropChannel.OnEventRaise -= PushMergeCommand;
    }
    private void Start()
    {
        Init();
    }

    public void Init()
    {
        slotScoring = new SlotScoring(gridController.grid);
        mergeChainResolver = new MergeChainResolver(slotScoring);
        connectedSlotFinder = new ConnectedSlotFinder(gridController.grid);
        mergeHandler = new MergeHandler(connectedSlotFinder, mergeChainResolver);
    }



    private void PushMergeCommand(Vector2Int pos)
    {
        mergeQueue.Enqueue(pos);
        if (!isRunning)
        {
            Merge().Forget();
        }
    }

    private async UniTask Merge()
    {
        isRunning = true;
        while (mergeQueue.Count != 0)
        {
           Vector2Int pos = mergeQueue.Dequeue();
           await mergeHandler.MergeSlot(pos);
        }
        isRunning = false;
    }
    
}