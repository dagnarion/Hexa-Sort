using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MergeService : MonoBehaviour
{
    [SerializeField] private GridController gridController;
    [SerializeField] private EventChannel<Vector2Int> dropChannel;
    private SlotScoring slotScoring;

    private MergeChainResolver chainResolver;
    private ConnectedSlotFinder connectedSlotFinder;
    private MergeHandler mergeHandler;
    private MergeVisual mergeVisual;
    private MergeResolve mergeResolve;

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
        chainResolver = new MergeChainResolver(slotScoring);
        connectedSlotFinder = new ConnectedSlotFinder(gridController.grid);
        mergeVisual = new MergeVisual();
        mergeResolve = new MergeResolve(mergeVisual);
        mergeHandler = new MergeHandler(mergeVisual);
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
        List<Slot> MergeRoot = new List<Slot>();
        List<Slot> potentialSlot = new List<Slot>();
        while (mergeQueue.Count > 0)
        {
            MergeRoot.Clear();
            potentialSlot.Clear();
            while (mergeQueue.Count != 0)
            {
                Vector2Int pos = mergeQueue.Dequeue();
                List<MergeNode> mergeChain = GetMergeChain(pos, out Slot root);
                if (root != null) MergeRoot.Add(root);
               potentialSlot.AddRange(await mergeHandler.Merge(mergeChain));
            }
            if (MergeRoot.Count > 0)
            {
                List<UniTask> tasks = new List<UniTask>();
                foreach (Slot slot in MergeRoot)
                    tasks.Add(mergeResolve.Resolve(slot));
                await UniTask.WhenAll(tasks);
            }

            if (potentialSlot != null && potentialSlot.Count != 0)
            {
                foreach (Slot slot in potentialSlot)
                {
                    mergeQueue.Enqueue(slot.Position);
                }
            }
            
        }

        isRunning = false;
    }

    private List<MergeNode> GetMergeChain(Vector2Int pos,out Slot root)
    {
        List<Slot> path = connectedSlotFinder.FindConnectedSameColorSlots(pos);
        
        if (path.Count <= 1)
        {
            root = null;
            return null;
        }
        Slot originSlot = path.Find(s => s.Position == pos);
        List<MergeNode> chain = chainResolver.ResolveMergeOrder(path,out root, originSlot);
        return chain;
    }
}
