using UnityEngine;

public class BoardBuilder
{
    private ComponentPoolSO<Slot> slotPool;
    private ComponentPoolSO<Hexagon> hexagonPool;
    private ComponentPoolSO<HexagonStack> hexagonStackPool;

    private readonly LockFactory lockFactory;
    private readonly Grid unityGrid;
    private readonly Transform holder;

    public BoardBuilder(ComponentPoolSO<Slot> slotPool, ComponentPoolSO<Hexagon> hexagonPool,
        ComponentPoolSO<HexagonStack> hexagonStackPool, LockFactory lockFactory, Grid unityGrid, Transform holder)
    {
        this.slotPool = slotPool;
        this.hexagonPool = hexagonPool;
        this.hexagonStackPool = hexagonStackPool;
        this.lockFactory = lockFactory;
        this.unityGrid = unityGrid;
        this.holder = holder;
    }

    public Grid<Slot> Build(LevelDataSO levelData)
    {
        Vector2Int size = levelData.gridSize;

        if (size.x <= 0 || size.y <= 0)
        {
            levelData.RecalculateBounds();
            size = levelData.gridSize;
        }

        var grid = new Grid<Slot>(size, _ => null);

        foreach (var slotData in levelData.GetActiveSlots())
        {
            Slot slot = BuildSlot(levelData, slotData);
            grid.SetValue(slotData.gridPosition, slot);
        }

        return grid;
    }

    private Slot BuildSlot(LevelDataSO levelData, SlotLevelData slotData)
    {
        Vector2Int gridPosition = slotData.gridPosition;

        GetWorldTransform(levelData, gridPosition, out Vector3 position, out Quaternion rotation);
        Slot slot = slotPool.Get();
        slot.transform.SetParent(holder);
        slot.transform.SetPositionAndRotation(position, rotation);

        HexagonStack stack = null;
        if (slotData.hasStack && slotData.stackColors != null && slotData.stackColors.Count > 0)
        {
            stack = BuildStack(slot, slotData);
        }

        ILock lockInstance = null;
        if (slotData.lockType != LockType.None)
        {
            Vector3 lockPosition = stack != null
                ? position.With(y: position.y + 0.2f + 0.2f * stack.GetNumberOfElement())
                : position.With(y: position.y + 0.2f);

            lockInstance = lockFactory.CreateLock(
                slotData.lockType,
                slotData.lockType == LockType.BreakLock
                    ? slotData.breakHitCount
                    : slotData.taskTargetScore,
                lockPosition,
                rotation,
                holder
            );
        }

        slot.Init(slotData.slotType, gridPosition, stack, lockInstance);
        return slot;
    }

    private HexagonStack BuildStack(Slot slot, SlotLevelData slotData)
    {
        if (!slotData.hasStack || slotData.stackColors == null || slotData.stackColors.Count == 0) return null;
        HexagonStack stack = hexagonStackPool.Get();
        stack.transform.SetParent(slot.transform, false);
        stack.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        stack.transform.localRotation = Quaternion.identity;
        stack.transform.localScale = Vector3.one;

        stack.Render.SetOriginPosition(stack.transform.position);
        stack.Render.SetUpAndDownPosition(slot.transform.position);

        foreach (var color in slotData.stackColors)
        {
            Hexagon hex = hexagonPool.Get();

            hex.SetParent(stack.transform);

            hex.transform.localPosition = Vector3.zero;
            hex.transform.localRotation = Quaternion.identity;
            hex.transform.localScale = Vector3.one;

            hex.Init(color);

            hex.render.transform.localRotation = Quaternion.identity;
            hex.render.SetPosition(
                stack.Render.GetTopPosition()
            );

            stack.AddElement(hex);
        }

        slot.FillHexagonStackToSlot(stack);
        stack.DisableAllCollider();
        return stack;
    }

    private void GetWorldTransform(LevelDataSO levelData, Vector2Int pos, out Vector3 position, out Quaternion rotation)
    {
        position = unityGrid.GetCellCenterWorld(new Vector3Int(pos.y, pos.x, 0));
        rotation = Quaternion.Euler(0f, 30f, 0f);
    }
}