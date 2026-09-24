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
            slot.Init(SlotType.Nozmal,pos);
            return slot;
        });
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
        if (!targetSlot.IsEmpty)
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
            hexaStack.ReturnToOriginPosition();
            return;
        }

        Slot slot = grid.GetValue(gridPos);
        if(!slot.IsEmpty)
        {
            hexaStack.ReturnToOriginPosition();
            return;
        }
        slot.FillHexagonStackToSlot(hexaStack);
        hexaStack.DropToTargetPosition(slot.transform.position);
        hexaStack.transform.SetParent(slot.transform);
        dropHexagonChannel.Raise(gridPos);
        hexaStack.DisableAllCollider();
        currentSlot?.Deselected();
        currentSlot = null;
    }
    
    
    
}