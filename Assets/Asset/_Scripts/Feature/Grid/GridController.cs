using System;
using NaughtyAttributes;
using UnityEngine;

public class GridController : MonoBehaviour
{
    [SerializeField] private EventChannel<Vector3> dragEventChannel;
    [SerializeField] private EventChannel<(HexagonStack, Vector3)> dropEventChannel;
    [SerializeField] private EventChannel<Vector2Int> dropHexagonChannel;
    [SerializeField] private GridDataSO data;
    [SerializeField] private Grid gridComponent;
    
    [SerializeField] private Slot slotPrefab;
    [SerializeField] private LockFactory lockFactory;
    
    [SerializeField] private Transform holder;
    public Grid<Slot> grid { get; private set; }
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

    private void Start()
    {
        GenerateGrid();
    }
    
    [Button]
    void GenerateGrid()
    {
        holder.Clear();
        grid = new Grid<Slot>(data.GridSize, pos =>
        {
            Vector3 position = gridComponent.GetCellCenterWorld(new Vector3Int(pos.x,pos.y,0));
            Slot slot = Instantiate(slotPrefab,position,Quaternion.identity,holder);
            slot.Init(SlotType.Nozmal,pos,null,null);
            return slot;
        });
        Test(new Vector2Int(0,0));
        Test2(new Vector2Int(0,1));
    }

    private void Test(Vector2Int pos)
    {
        Destroy(grid.GetValue(pos).gameObject);
        Vector3 position = gridComponent.GetCellCenterWorld(new Vector3Int(pos.x,pos.y,0));
        Slot slot = Instantiate(slotPrefab,position,Quaternion.identity,holder);
        ILock taskLock = lockFactory.CreateLock(LockType.TaskLock, 50, position);
        slot.Init(SlotType.Nozmal,pos,null,taskLock);
        grid.SetValue(pos,slot);     
    }    
    
    private void Test2(Vector2Int pos)
    {
        Destroy(grid.GetValue(pos).gameObject);
        Vector3 position = gridComponent.GetCellCenterWorld(new Vector3Int(pos.x,pos.y,0));
        Slot slot = Instantiate(slotPrefab,position,Quaternion.identity,holder);
        ILock breakLock = lockFactory.CreateLock(LockType.BreakLock,2, position);
        slot.Init(SlotType.Nozmal,pos,null,breakLock);
        grid.SetValue(pos,slot);     
    }

    void SlotSelected(Vector3 pos)
    {
        Vector2Int gridPos = (Vector2Int) gridComponent.WorldToCell(pos);
        if (!grid.IsOnGrid(gridPos))
        {
            currentSlot?.Deselected();
            currentSlot = null;
            return;
        }

        Slot targetSlot = grid.GetValue(gridPos);
        if (!targetSlot.IsEmpty || targetSlot.IsLocked)
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
        Vector2Int gridPos = (Vector2Int)gridComponent.WorldToCell(pos);
        if(hexaStack == null) return;
        
        if (!grid.IsOnGrid(gridPos))
        {
            hexaStack.Render.ReturnToOriginPosition();
            return;
        }

        Slot slot = grid.GetValue(gridPos);
        if(!slot.IsEmpty || slot.IsLocked)
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
        Vector2Int gridPos = (Vector2Int)gridComponent.WorldToCell(pos);
        Slot slot = grid.GetValue(gridPos);
        return slot;
    }
    
}