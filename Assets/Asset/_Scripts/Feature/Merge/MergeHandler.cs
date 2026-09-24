using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

public class MergeHandler
{
    private ConnectedSlotFinder connectedSlotFinder;
    private MergeChainResolver chainResolver;
    private MergeVisual mergeVisual;

    public MergeHandler(ConnectedSlotFinder connectedSlotFinder,MergeChainResolver chainResolver)
    {
        this.connectedSlotFinder = connectedSlotFinder;
        this.chainResolver = chainResolver;
        mergeVisual = new MergeVisual(); // test
    }
        
        
    public async UniTask MergeSlot(Vector2Int pos)
    {
        List<Slot> path = connectedSlotFinder.FindConnectedSameColorSlots(pos);
        if(path.Count <= 1) return;
        
        Slot originSlot = path.Find(s => s.Position == pos);
        List<MergeNode> chain = chainResolver.ResolveMergeOrder(path,originSlot);
        await Merge(chain);
        await MergeResolve(chainResolver.Root.GetHexagonStack(),chainResolver.Root);
    }

    private async UniTask Merge(List<MergeNode> chain)
    {
        foreach (var node in chain)
        {
            if (node.Parent != null)
            {
               await MergeStack(node.Slot.GetHexagonStack(),node.Parent.Slot.GetHexagonStack(),node.Slot); 
            }
        }
    }

    private async UniTask MergeStack(HexagonStack currentStack,HexagonStack targetStack,Slot currentSlot)
    {
        if (targetStack == null || currentStack == null) return;
        Hexagon topTargetCell = targetStack.GetTopElement();
        
        if (topTargetCell == null) return;
        List<Hexagon> hexagons = new List<Hexagon>();
        Vector3 targetPos = targetStack.GetTopPosition();
        
        for (int i = currentStack.GetNumberOfElement() - 1; i >= 0; i--)
        {
            if(currentStack.GetElement(i).ColorType != topTargetCell.ColorType) break;
            Hexagon hexa = currentStack.GetElement(i);
            currentStack.RemoveElement(i);
            targetStack.AddElement(hexa);
            hexagons.Add(hexa);
        }
        
        if(hexagons.Count == 0) return;
        await mergeVisual.VisualHandler(hexagons,targetPos);
        
        for(int i = 0;i<hexagons.Count;i++)
            hexagons[i].SetParent(targetStack.transform);

        if (currentStack.IsEmpty)
        {
            currentSlot?.ReleaseSlot();
            currentStack.transform.SetParent(null);
            currentStack.gameObject.SetActive(false);
        }
    }

    private async UniTask MergeResolve(HexagonStack hexagonStack,Slot slot)
    {
        if(hexagonStack == null || hexagonStack.IsEmpty) return;
        List<Hexagon> hexagons = new List<Hexagon>();
        Hexagon topHexagon = hexagonStack.GetTopElement();
        hexagons.Add(topHexagon);
        for (int i = hexagonStack.GetNumberOfElement() - 2; i >= 0; i--)
        {
            if (hexagonStack.GetElement(i).ColorType == topHexagon.ColorType) hexagons.Add(hexagonStack.GetElement(i));
            else break;
        }
        if(hexagonStack.IsEmpty) return;
        if(hexagons.Count < 10) return;
        foreach (var it in hexagons)
        {
            it.SetParent(null);
            hexagonStack.RemoveElement(it);
        }
        await mergeVisual.ReleaseHexagon(hexagons);
        if (hexagonStack.IsEmpty)
        {
            slot?.ReleaseSlot();
            hexagonStack.transform.SetParent(null);
            hexagonStack.gameObject.SetActive(false);
        }
    }
}