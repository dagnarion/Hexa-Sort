using UnityEngine;

public static class MergeRule
{
    public static bool CanMerge(Slot currentSlot, Slot targetSlot)
    {
        if (currentSlot == null || currentSlot.IsEmpty || currentSlot.IsLocked) return false;
        if (targetSlot == null || targetSlot.IsEmpty || currentSlot.IsLocked) return false;
        return IsSameColor(currentSlot.GetHexagonStack().GetTopElement().ColorType,
               targetSlot.GetHexagonStack().GetTopElement().ColorType);
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
        if (slot.IsEmpty || slot.IsLocked) return false;
        return slot.GetHexagonStack().GetNumberOfElement() >= 10;
    }
}