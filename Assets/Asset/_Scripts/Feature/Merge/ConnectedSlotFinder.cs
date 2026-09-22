using System.Collections.Generic;
using UnityEngine;

public class ConnectedSlotFinder
{
    private Grid<Slot> grid;

    public ConnectedSlotFinder(Grid<Slot> grid)
    {
        this.grid = grid;
    }

    public List<Slot> FindConnectedSameColorSlots(Vector2Int startPos)
    {
        List<Slot> results = new List<Slot>();
        Slot originSlot = grid.GetValue(startPos);
        if (originSlot == null || originSlot.IsEmpty) return results;
            
        FloodFill(startPos,originSlot,results);
        return results;
    }
        
        
    private void FloodFill(Vector2Int currentPosition, Slot originSlot, List<Slot> slotHolder)
    {
        Slot currentSlot = grid.GetValue(currentPosition);
        if(!MergeRule.CanMerge(currentSlot,originSlot)) return;
            
        slotHolder.Add(currentSlot);
            
        foreach (Vector2Int direct in Direction.GetDirections(currentPosition))
        {
            Vector2Int nextPosition = new Vector2Int(currentPosition.x + direct.x, currentPosition.y + direct.y);
                
            if(!grid.IsOnGrid(nextPosition)) continue;
            Slot nextSlot = grid.GetValue(nextPosition);
                
            if(nextSlot == null || nextSlot.IsEmpty) continue;
            if(slotHolder.Contains(nextSlot)) continue;
                
            FloodFill(nextPosition,currentSlot,slotHolder);
        }
    }
        
}
