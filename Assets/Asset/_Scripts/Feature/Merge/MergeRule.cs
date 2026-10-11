using UnityEngine;

public static class MergeRule
{
    public static bool CanMerge(Slot currentSlot, Slot targetSlot)
    {
        if (currentSlot == null || currentSlot.IsEmpty || currentSlot.IsLocked) return false;
        if (targetSlot == null || targetSlot.IsEmpty || targetSlot.IsLocked) return false;
        Hexagon currentTop = currentSlot.GetHexagonStack().GetTopElement();
        Hexagon targetTop = targetSlot.GetHexagonStack().GetTopElement();
        if (currentTop == null || targetTop == null) return false;
        return IsSameColor(currentTop.ColorType, targetTop.ColorType);
    }

    public static bool IsSameColor(Color a, Color b)
    {
        Color32 c1 = a;
        Color32 c2 = b;
        return Mathf.Abs(c1.r - c2.r) <= 3 &&
               Mathf.Abs(c1.g - c2.g) <= 3 &&
               Mathf.Abs(c1.b - c2.b) <= 3;
    }

    public static bool CanClearSlot(Slot slot)
    {
        if (slot == null || slot.IsEmpty || slot.IsLocked) return false;
        HexagonStack stack = slot.GetHexagonStack();
        if (stack.GetNumberOfElement() < 10) return false;
        Hexagon top = stack.GetTopElement();
        if (top == null) return false;
        int sameColorCount = 1;
        for (int i = stack.GetNumberOfElement() - 2; i >= 0; i--)
        {
            Hexagon elem = stack.GetElement(i);
            if (elem != null && IsSameColor(elem.ColorType, top.ColorType))
                sameColorCount++;
            else
                break;
        }
        return sameColorCount >= 10;
    }
}