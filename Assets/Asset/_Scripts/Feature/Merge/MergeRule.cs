public static class MergeRule
{
    public static bool CanMerge(Slot currentSlot, Slot targetSlot)
    {
        if (currentSlot == null || currentSlot.IsEmpty || currentSlot.IsLocked) return false;
        if (targetSlot == null || targetSlot.IsEmpty || currentSlot.IsLocked) return false;
        return currentSlot.GetHexagonStack().GetTopElement().ColorType ==
               targetSlot.GetHexagonStack().GetTopElement().ColorType;
    }

    public static bool CanClearSlot(Slot slot)
    {
        if (slot.IsEmpty || slot.IsLocked) return false;
        return slot.GetHexagonStack().GetNumberOfElement() >= 10;
    }
}