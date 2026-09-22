using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MergeChainResolver
{
    private SlotScoring slotScoring;

    public MergeChainResolver(SlotScoring slotScoring)
    {
        this.slotScoring = slotScoring;
    }

    public List<MergeNode> ResolveMergeOrder(List<Slot> path)
    {
        if (path == null || path.Count == 0) return new List<MergeNode>();
        Slot root = GetRoot(path);
        MergeNode rootNode = BuildTree(root, path);
        List<MergeNode> executionOrder = new List<MergeNode>();
        TraversePostOrder(rootNode, executionOrder);
        return executionOrder;
    }

    private void TraversePostOrder(MergeNode node, List<MergeNode> result)
    {
        if (node == null) return;
        foreach (var child in node.Child)
        {
            TraversePostOrder(child, result);
        }

        result.Add(node);
    }


    private MergeNode BuildTree(Slot root, List<Slot> path)
    {
        MergeNode rootNode = new MergeNode(root);
        Dictionary<Vector2Int, Slot> slotLookup = new Dictionary<Vector2Int, Slot>();
        foreach (var it in path)
        {
            slotLookup[it.Position] = it;
        }

        Queue<MergeNode> queue = new Queue<MergeNode>();
        HashSet<Slot> visited = new HashSet<Slot>();
        queue.Enqueue(rootNode);
        visited.Add(root);
        while (queue.Count > 0)
        {
            MergeNode currentNode = queue.Dequeue();
            Vector2Int currentPos = currentNode.Slot.Position;
            foreach (var dir in Direction.GetDirections(currentPos))
            {
                Vector2Int neighborPos = new Vector2Int(currentPos.x + dir.x, currentPos.y + dir.y);
                if (slotLookup.TryGetValue(neighborPos, out Slot neighborSlot))
                {
                    if (!visited.Contains(neighborSlot))
                    {
                        visited.Add(neighborSlot);
                        MergeNode childNode = new MergeNode(neighborSlot);
                        currentNode.AddChild(childNode);
                        queue.Enqueue(childNode);
                    }
                }
            }
        }

        return rootNode;
    }


    private Slot GetRoot(List<Slot> path)
    {
        List<(Slot, int)> slot = new List<(Slot, int)>();
        List<Slot> tmp = new List<Slot>();
        int minn = int.MaxValue;
        foreach (var it in path)
        {
            int score = slotScoring.CalculateMergeCost(it.Position);
            slot.Add((it, score));
            minn = Math.Min(minn, score);
        }

        foreach (var it in slot)
        {
            if (it.Item2 == minn) tmp.Add(it.Item1);
        }

        return tmp[Random.Range(0, tmp.Count)];
    }
}