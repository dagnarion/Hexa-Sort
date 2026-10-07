using System;
using NaughtyAttributes;
using UnityEngine;

public class GridController : MonoBehaviour
{
    #region EventChannel
    [SerializeField] private EventChannel<Vector3> dragEventChannel;
    [SerializeField] private EventChannel<(HexagonStack, Vector3)> dropEventChannel;
    [SerializeField] private EventChannel<Vector2Int> dropHexagonChannel;
    #endregion

    #region Grid
    [SerializeField] private Grid gridComponent;
    private BoardBuilder boardBuilder;
    public Grid<Slot> grid { get; private set; }
    #endregion
    
    [SerializeField] private Transform holder;

    [SerializeField] private LevelDataSO levelData;
    [SerializeField] private ComponentPoolSO<Slot> slotPool;
    [SerializeField] private ComponentPoolSO<Hexagon> hexagonPool;
    [SerializeField] private ComponentPoolSO<HexagonStack> hexagonStackPool;
    [SerializeField] private LockFactory lockFactory;
    

    private Slot currentSlot;
    
    private void OnEnable()
    {
        dragEventChannel.OnEventRaise += SlotSelected;
        dropEventChannel.OnEventRaise += OnDropProccessing;
    }

    private void OnDisable()
    {
        dragEventChannel.OnEventRaise -= SlotSelected;
        dropEventChannel.OnEventRaise -= OnDropProccessing;
    }

    private void Awake()
    {
        boardBuilder = new BoardBuilder(slotPool,hexagonPool,hexagonStackPool,lockFactory, gridComponent, holder);
    }

    private void Start()
    {
        slotPool.InitPool(holder);
        GenerateGrid(levelData);
    }
    
    void GenerateGrid(LevelDataSO levelData)
    {
        grid = boardBuilder.Build(levelData);
    }
    

    void SlotSelected(Vector3 pos)
    {
        Vector3Int cell = gridComponent.WorldToCell(pos);
        Vector2Int gridPos = new Vector2Int(cell.y, cell.x);
        if (!grid.IsOnGrid(gridPos))
        {
            currentSlot?.Deselected();
            currentSlot = null;
            return;
        }

        Slot targetSlot = grid.GetValue(gridPos);
        if (targetSlot == null || !targetSlot.IsEmpty || targetSlot.IsLocked)
        {
            currentSlot?.Deselected();
            currentSlot = null;
            return;
        }
        
        if (currentSlot != targetSlot)
        {
            currentSlot?.Deselected();
            currentSlot = grid.GetValue(gridPos);
            currentSlot?.Selected();
        }
    }

    void OnDropProccessing((HexagonStack,Vector3) value)
    {
        HexagonStack hexaStack = value.Item1;
        Vector3 pos = value.Item2;
        Vector3Int cell = gridComponent.WorldToCell(pos);
        Vector2Int gridPos = new Vector2Int(cell.y, cell.x);
        if(hexaStack == null) return;
        
        if (!grid.IsOnGrid(gridPos))
        {
            hexaStack.Render.ReturnToOriginPosition();
            return;
        }

        Slot slot = grid.GetValue(gridPos);
        if(slot == null || !slot.IsEmpty || slot.IsLocked)
        {
            hexaStack.Render.ReturnToOriginPosition();
            return;
        }
        
        slot.FillHexagonStackToSlot(hexaStack);
        
        hexaStack.Render.DropToTargetPosition(slot.transform.position);
        hexaStack.transform.SetParent(slot.transform);
        
        dropHexagonChannel.Raise(gridPos);
        hexaStack.DisableAllCollider();
        
        currentSlot?.Deselected();
        currentSlot = null;
    }

    public Slot GetSlotOnPosition(Vector3 pos)
    {
        Vector3Int cell = gridComponent.WorldToCell(pos);
        Vector2Int gridPos = new Vector2Int(cell.y, cell.x);
        Slot slot = grid.GetValue(gridPos);
        return slot;
    }

    public bool IsFull()
    {
        bool fulled = true;
        grid.GridTraversal((pos, slot) =>
        {
            if (slot!= null && slot.IsEmpty)
            {
                fulled = false;
            }
        });
        return fulled;
    }
}