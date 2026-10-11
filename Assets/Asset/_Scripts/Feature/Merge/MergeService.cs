using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;

public class MergeService : MonoBehaviour
{
    [SerializeField] private Transform holder;
    [SerializeField] private EventChannel<Vector2Int> dropChannel;
    [SerializeField] private ComponentPoolSO<HexagonStack> hexagonStackPool;
    private SlotScoring slotScoring;

    private MergeChainResolver chainResolver;
    private ConnectedSlotFinder connectedSlotFinder;
    private MergeHandler mergeHandler;
    private MergeVisual mergeVisual;
    private MergeResolve mergeResolve;
    private Queue<Vector2Int> mergeQueue = new Queue<Vector2Int>();
    private bool isRunning;
    private bool canMerge;

    private void OnEnable()
    {
        dropChannel.OnEventRaise += PushMergeCommand;
    }

    private void OnDisable()
    {
        dropChannel.OnEventRaise -= PushMergeCommand;
        RemoveAllCommand();
    }

    public void Init(Grid<Slot> grid)
    {
        RemoveAllCommand();
        slotScoring = new SlotScoring(grid);
        chainResolver = new MergeChainResolver(slotScoring);
        connectedSlotFinder = new ConnectedSlotFinder(grid);
        mergeVisual = new MergeVisual();
        mergeResolve = new MergeResolve(mergeVisual, grid, hexagonStackPool, holder);
        mergeHandler = new MergeHandler(mergeVisual, holder);
    }

    public bool IsFinishAllMerge() => !isRunning && mergeQueue.Count == 0;

    public void RemoveAllCommand()
    {
        canMerge = false;
        mergeQueue.Clear();
    }

    private void PushMergeCommand(Vector2Int pos)
    {
        canMerge = true;
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
        try
        {
            while (mergeQueue.Count > 0)
            {
                MergeRoot.Clear();
                potentialSlot.Clear();
                while (mergeQueue.Count != 0)
                {
                    Vector2Int pos = mergeQueue.Dequeue();
                    List<MergeNode> mergeChain = GetMergeChain(pos, out Slot root);
                    if (root != null && !MergeRoot.Contains(root))
                        MergeRoot.Add(root);

                    List<Slot> mergedSlots = await mergeHandler.Merge(mergeChain);
                    foreach (Slot s in mergedSlots)
                    {
                        if (s != null && !potentialSlot.Contains(s))
                            potentialSlot.Add(s);
                    }
                }

                if (MergeRoot.Count > 0)
                {
                    List<UniTask> tasks = new List<UniTask>();
                    foreach (Slot slot in MergeRoot)
                        tasks.Add(mergeResolve.Resolve(slot));
                    await UniTask.WhenAll(tasks);

                    foreach (Slot slot in MergeRoot)
                    {
                        if (slot != null && !slot.IsEmpty && !potentialSlot.Contains(slot))
                            potentialSlot.Add(slot);
                    }
                }

                if (!canMerge) break;
                if (potentialSlot.Count != 0)
                {
                    foreach (Slot slot in potentialSlot)
                    {
                        if (slot != null && !slot.IsEmpty)
                            mergeQueue.Enqueue(slot.Position);
                    }
                }
            }
        }
        finally
        {
            isRunning = false;
        }
    }

    private List<MergeNode> GetMergeChain(Vector2Int pos, out Slot root)
    {
        List<Slot> path = connectedSlotFinder.FindConnectedSameColorSlots(pos);

        if (path.Count <= 1)
        {
            if (path.Count == 1 && MergeRule.CanClearSlot(path[0]))
            {
                root = path[0];
            }
            else
            {
                root = null;
            }
            return null;
        }

        Slot originSlot = path.Find(s => s.Position == pos);
        List<MergeNode> chain = chainResolver.ResolveMergeOrder(path, out root, originSlot);
        return chain;
    }
}