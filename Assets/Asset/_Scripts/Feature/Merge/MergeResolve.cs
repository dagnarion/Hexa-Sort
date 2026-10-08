using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;


public class MergeResolve
{
    private Transform holder;
    private MergeVisual mergeVisual;
    private Grid<Slot> grid;
    private ComponentPoolSO<HexagonStack> hexagonStackPool;
    public MergeResolve(MergeVisual mergeVisual,Grid<Slot> grid,ComponentPoolSO<HexagonStack> hexagonStackPool,Transform holder)
    {
        this.mergeVisual = mergeVisual;
        this.holder = holder;
        this.grid = grid;
        this.hexagonStackPool = hexagonStackPool;
    }
    
    public async UniTask Resolve(Slot slot)
    {
        HexagonStack hexagonStack = slot.GetHexagonStack();
        if(hexagonStack == null || hexagonStack.IsEmpty) return;
        
        List<Hexagon> hexagons = new List<Hexagon>();
        Hexagon topHexagon = hexagonStack.GetTopElement();
        hexagons.Add(topHexagon);
        for (int i = hexagonStack.GetNumberOfElement() - 2; i >= 0; i--)
        {
            if (MergeRule.IsSameColor(hexagonStack.GetElement(i).ColorType, topHexagon.ColorType)) hexagons.Add(hexagonStack.GetElement(i));
            else break;
        }
        if(hexagonStack.IsEmpty) return;
        if(hexagons.Count < 10) return;
        foreach (var it in hexagons)
        {
            it.SetParent(holder);
            hexagonStack.RemoveElement(it);
        }
        await mergeVisual.ReleaseHexagon(hexagons);
        
        await TryDamageNeighbourLocksAsync(slot);
        
        if (hexagonStack.IsEmpty)
        {
            slot?.ReleaseSlot();
            hexagonStack.transform.SetParent(null);
            hexagonStackPool.Release(hexagonStack);
        }
    }


    private async UniTask TryDamageNeighbourLocksAsync(Slot slot)
    {
        Vector2Int currentPosition = slot.Position;
        List<UniTask> damageTasks = new List<UniTask>();
        foreach (Vector2Int direct in Direction.GetDirections(currentPosition))
        {
            Vector2Int nextPosition = currentPosition + direct;
            if (!grid.IsOnGrid(nextPosition)) continue;
            Slot nextSlot = grid.GetValue(nextPosition);
            if (nextSlot == null || !nextSlot.IsLocked) continue;
            damageTasks.Add(nextSlot.TryHitLockAsync());
        }
        if (damageTasks.Count > 0)
            await UniTask.WhenAll(damageTasks);
    }
    
}