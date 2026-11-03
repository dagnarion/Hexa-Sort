using System.Collections.Generic;
using UnityEngine;

public class ConnectedSlotFinder
{
    private readonly Grid<Slot> grid;

    public ConnectedSlotFinder(Grid<Slot> grid)
    {
        this.grid = grid;
    }

    public List<Slot> FindConnectedSameColorSlots(Vector2Int startPos)
    {
        List<Slot> results = new List<Slot>();
        if (grid == null || !grid.IsOnGrid(startPos)) return results;

        Slot originSlot = grid.GetValue(startPos);
        if (originSlot == null || originSlot.IsEmpty) return results;

        Queue<Vector2Int> positions = new Queue<Vector2Int>();
        HashSet<Slot> visited = new HashSet<Slot>();
        positions.Enqueue(startPos);

        while (positions.Count > 0)
        {
            Vector2Int currentPosition = positions.Dequeue();
            Slot currentSlot = grid.GetValue(currentPosition);
            if (!MergeRule.CanMerge(currentSlot, originSlot) || !visited.Add(currentSlot)) continue;

            results.Add(currentSlot);
            foreach (Vector2Int direction in Direction.GetDirections(currentPosition))
            {
                Vector2Int nextPosition = currentPosition + direction;
                if (!grid.IsOnGrid(nextPosition)) continue;
                Slot nextSlot = grid.GetValue(nextPosition);
                if (nextSlot != null && !nextSlot.IsEmpty && !visited.Contains(nextSlot))
                {
                    positions.Enqueue(nextPosition);
                }
            }
        }

        return results;
    }
}
