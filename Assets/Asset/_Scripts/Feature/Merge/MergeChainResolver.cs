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

    public List<MergeNode> ResolveMergeOrder(List<Slot> path, Slot droppedSlot = null)
    {
        if (path == null || path.Count == 0) return new List<MergeNode>();
        Slot root = GetRoot(path,droppedSlot);
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


    private Slot GetRoot(List<Slot> path,Slot originSlot)
    {
        int minScore = int.MaxValue;
        List<Slot> lowestScoreSlots = new List<Slot>();
        foreach (var slot in path)
        {
            int score = slotScoring.CalculateMergeCost(slot.Position);
            if (score < minScore)
            {
                minScore = score;
                lowestScoreSlots.Clear();
                lowestScoreSlots.Add(slot);
            }
            else if (score == minScore)
            {
                lowestScoreSlots.Add(slot);
            }
        }
        if (originSlot != null && lowestScoreSlots.Contains(originSlot))
        {
            return originSlot;
        }
        return lowestScoreSlots[0];
    }
}