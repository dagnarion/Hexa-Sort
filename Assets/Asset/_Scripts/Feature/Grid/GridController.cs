using System;
using NaughtyAttributes;
using UnityEngine;

public class GridController : MonoBehaviour
{
    [SerializeField] private DragAndDropHandler _dragAndDropHandler;
    [SerializeField] private GridDataSO data;
    [SerializeField] private Grid gridComponent;
    [SerializeField] private Slot slotPrefab;
    [SerializeField] private Transform holder;
    private Grid<Slot> grid;
    private Slot currentSlot;
    
    private void OnEnable()
    {
        _dragAndDropHandler.OnDrag += SlotSelected;
    }

    private void OnDisable()
    {
        _dragAndDropHandler.OnDrag -= SlotSelected;
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
            slot.Init(SlotType.Nozmal);
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
        if (currentSlot != grid.GetValue(gridPos))
        {
            currentSlot?.Deselected();
            currentSlot = grid.GetValue(gridPos);
            currentSlot?.Selected();
        }
    }
    
    
    
}