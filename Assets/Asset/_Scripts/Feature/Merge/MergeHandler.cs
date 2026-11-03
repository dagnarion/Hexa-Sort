using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MergeHandler
{
    private Transform holder;
    private MergeVisual mergeVisual;

    public MergeHandler(MergeVisual mergeVisual,Transform holder)
    {
        this.mergeVisual = mergeVisual; // test
        this.holder = holder;
    }
    
    public async UniTask<List<Slot>> Merge(List<MergeNode> chain)
    {
        List<Slot> potentialSlot = new List<Slot>();
        if (chain == null || chain.Count <= 1) return potentialSlot;
        
        foreach (var node in chain)
        {
            if (node == null || node.Slot == null) continue;
            if (node.Parent != null)
            {
                if (node.Parent.Slot == null) continue;
                await MergeStack(node.Slot.GetHexagonStack(), node.Parent.Slot.GetHexagonStack(), node.Slot);
                if (!node.Slot.IsEmpty && !potentialSlot.Contains(node.Slot)) potentialSlot.Add(node.Slot);
            }
            else if(!node.Slot.IsEmpty && !potentialSlot.Contains(node.Slot)) potentialSlot.Add(node.Slot);
        }

        return potentialSlot;
    }

    private async UniTask MergeStack(HexagonStack currentStack,HexagonStack targetStack,Slot currentSlot)
    {
        if (targetStack == null || currentStack == null || currentSlot == null ||
            !targetStack.gameObject.activeInHierarchy || !currentStack.gameObject.activeInHierarchy ||
            currentSlot.GetHexagonStack() != currentStack) return;
        Hexagon topTargetCell = targetStack.GetTopElement();
        
        if (topTargetCell == null) return;
        List<Hexagon> hexagons = new List<Hexagon>();
        Vector3 targetPos = targetStack.Render.GetTopPosition();
        
        for (int i = currentStack.GetNumberOfElement() - 1; i >= 0; i--)
        {
            if(!MergeRule.IsSameColor(currentStack.GetElement(i).ColorType, topTargetCell.ColorType)) break;
            Hexagon hexa = currentStack.GetElement(i);
            currentStack.RemoveElement(i);
            targetStack.AddElement(hexa);
            hexagons.Add(hexa);
        }
        
        if(hexagons.Count == 0) return;
        await mergeVisual.VisualHandler(hexagons,targetPos);
        
        for(int i = 0;i<hexagons.Count;i++)
        {
            if (hexagons[i] != null && hexagons[i].gameObject.activeInHierarchy && targetStack.gameObject.activeInHierarchy)
            {
                hexagons[i].SetParent(targetStack.transform);
            }
        }

        if (currentStack.gameObject.activeInHierarchy && currentStack.IsEmpty && currentSlot.GetHexagonStack() == currentStack)
        {
            currentSlot.ReleaseSlot();
            currentStack.Release();
        }
    }


}

