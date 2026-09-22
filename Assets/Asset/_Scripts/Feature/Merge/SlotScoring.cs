

using UnityEngine;

public class SlotScoring
{
    private Grid<Slot> grid;

    public SlotScoring(Grid<Slot> grid)
    {
        this.grid = grid;
    }

    public int CalculateMergeCost(Vector2Int pos)
    {
        if (!grid.IsOnGrid(pos)) return 0;
        Slot slot = grid.GetValue(pos);
        if (slot == null || slot.IsEmpty) return 0;
        Color targetColor =
            GetSecondElementColorTypes(slot.GetHexagonStack(), slot.GetHexagonStack().GetTopElement().ColorType);
            
        int cnt = 0;
        foreach (var direct in Direction.GetDirections(pos))
        {
            Vector2Int nextPos = new Vector2Int(pos.x + direct.x, pos.y + direct.y);
            if (!grid.IsOnGrid(nextPos)) continue;
            Slot nextSlot = grid.GetValue(nextPos);
            if (nextSlot == null || nextSlot.IsEmpty) continue;
            if (CompareColor(nextSlot.GetHexagonStack().GetTopElement(), targetColor)) cnt++;
        }
        return cnt;
    }

    private bool CompareColor(Hexagon target, Color targetColor) => target.ColorType == targetColor;

    private Color GetSecondElementColorTypes(HexagonStack stack, Color currentColorType)
    {
        Color type = currentColorType;
        for (int i = stack.GetNumberOfElement() - 1; i >= 0; i--)
        {
            if (stack.GetElement(i).ColorType == currentColorType) continue;
            else
            {
                type = stack.GetElement(i).ColorType;
                break;
            }
        }

        return type;
    }
}