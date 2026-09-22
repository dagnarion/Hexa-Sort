using System.Collections.Generic;
using UnityEngine;

public class MergeHandler
{
    private ConnectedSlotFinder connectedSlotFinder;
    private MergeChainResolver chainResolver;

    public MergeHandler(ConnectedSlotFinder connectedSlotFinder,MergeChainResolver chainResolver)
    {
        this.connectedSlotFinder = connectedSlotFinder;
        this.chainResolver = chainResolver;
    }
        
        
    public void MergeSlot(Vector2Int pos)
    {
        List<Slot> path = connectedSlotFinder.FindConnectedSameColorSlots(pos);
        if(path.Count <= 1) return;
        List<MergeNode> chain = chainResolver.ResolveMergeOrder(path);
        Merge(chain);
    }

    private void Merge(List<MergeNode> chain)
    {
        foreach (var node in chain)
        {
            if (node.Parent != null)
            {
                MergeStack(node.Slot.GetHexagonStack(),node.Parent.Slot.GetHexagonStack());
            }
        }
    }

    private void MergeStack(HexagonStack currentStack,HexagonStack targetStack)
    {
        if (targetStack == null || currentStack == null) return;
        Hexagon topTargetCell = targetStack.GetTopElement();
        if (topTargetCell == null) return;
        for (int i = currentStack.GetNumberOfElement() - 1; i >= 0; i--)
        {
            if(currentStack.GetElement(i).ColorType != topTargetCell.ColorType) return;
            Hexagon hexa = currentStack.GetElement(i);
            currentStack.RemoveElement(i);
            targetStack.AddElement(hexa);
        }
    }
        
}