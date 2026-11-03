using System;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;

public class GridController : MonoBehaviour
{
    #region EventChannel

    [SerializeField] private EventChannel<LevelData> levelLoadEventChannel;
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
    [SerializeField] private ComponentPoolSO<Slot> slotPool;
    [SerializeField] private ComponentPoolSO<Hexagon> hexagonPool;
    [SerializeField] private ComponentPoolSO<HexagonStack> hexagonStackPool;
    [SerializeField] private LockFactory lockFactory;
    [SerializeField] private MergeService mergeService;
    private Slot currentSlot;
    private void OnEnable()
    {
        if (dragEventChannel != null) dragEventChannel.OnEventRaise += SlotSelected;
        if (dropEventChannel != null) dropEventChannel.OnEventRaise += OnDropProccessing;
        if (levelLoadEventChannel != null) levelLoadEventChannel.OnEventRaise += GenerateGrid;
    }

    private void OnDisable()
    {
        if (dragEventChannel != null) dragEventChannel.OnEventRaise -= SlotSelected;
        if (dropEventChannel != null) dropEventChannel.OnEventRaise -= OnDropProccessing;
        if (levelLoadEventChannel != null) levelLoadEventChannel.OnEventRaise -= GenerateGrid;
    }

    private void Awake()
    { 
        boardBuilder = new BoardBuilder(slotPool,hexagonPool,hexagonStackPool,lockFactory, gridComponent, holder);
    }
    
    void GenerateGrid(LevelData data)
    {
        if (mergeService != null && !mergeService.IsFinishAllMerge())
        {
            Debug.LogError("Cannot rebuild the board while a merge is still running.", this);
            return;
        }

        currentSlot?.Deselected();
        currentSlot = null;
        grid = boardBuilder.Build(data.Level);
        mergeService.Init(grid);
    }
    
    void SlotSelected(Vector3 pos)
    {
        if (grid == null || gridComponent == null)
        {
            ClearCurrentSelection();
            return;
        }

        Vector3Int cell = gridComponent.WorldToCell(pos);
        Vector2Int gridPos = new Vector2Int(cell.y, cell.x);
        if (!grid.IsOnGrid(gridPos))
        {
            currentSlot?.Deselected();
            currentSlot = null;
            return;
        }

        Slot targetSlot = grid.GetValue(gridPos);
        if (targetSlot == null || !targetSlot.IsEmpty || targetSlot.IsLocked || targetSlot.Type == SlotType.Block)
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
        if (hexaStack == null || !hexaStack.gameObject.activeInHierarchy) return;
        if (hexaStack.GetComponentInParent<Slot>() != null) return;
        if (mergeService == null || !mergeService.IsAcceptingCommands || !mergeService.IsFinishAllMerge())
        {
            ReturnToOrigin(hexaStack);
            ClearCurrentSelection();
            return;
        }

        if (grid == null || gridComponent == null)
        {
            ReturnToOrigin(hexaStack);
            ClearCurrentSelection();
            return;
        }

        Vector3Int cell = gridComponent.WorldToCell(pos);
        Vector2Int gridPos = new Vector2Int(cell.y, cell.x);
        
        if (!grid.IsOnGrid(gridPos))
        {
            ClearCurrentSelection();
            ReturnToOrigin(hexaStack);
            return;
        }

        Slot slot = grid.GetValue(gridPos);
        if(slot == null || !slot.IsEmpty || slot.IsLocked || slot.Type == SlotType.Block)
        {
            ClearCurrentSelection();
            ReturnToOrigin(hexaStack);
            return;
        }
        
        slot.FillHexagonStackToSlot(hexaStack);
        
        hexaStack.Render.DropToTargetPosition(slot.transform.position);
        hexaStack.transform.SetParent(slot.transform);
        hexaStack.DisableAllCollider();
        ClearCurrentSelection();
        dropHexagonChannel?.Raise(gridPos);
    }

    public UniTask StopMergesAndWaitAsync()
    {
        return mergeService == null ? UniTask.CompletedTask : mergeService.StopAcceptingAndWaitAsync();
    }

    public bool IsMergeIdle => mergeService == null || mergeService.IsFinishAllMerge();

    public Plane GetBoardPlane(Vector3 point)
    {
        Vector3 normal = gridComponent == null ? Vector3.up : gridComponent.transform.up;
        return new Plane(normal, point);
    }

    public Plane GetBoardSurfacePlane()
    {
        if (gridComponent == null) return new Plane(Vector3.up, Vector3.up * 1f);
        return new Plane(gridComponent.transform.up, gridComponent.transform.position);
    }

    public void ClearGridReference()
    {
        ClearCurrentSelection();
        grid = null;
    }

    private void ClearCurrentSelection()
    {
        currentSlot?.Deselected();
        currentSlot = null;
    }

    private static void ReturnToOrigin(HexagonStack stack)
    {
        if (stack != null && stack.gameObject.activeInHierarchy && stack.GetComponentInParent<Slot>() == null)
        {
            stack.Render.ReturnToOriginPosition();
        }
    }
    
    public Slot GetSlotOnPosition(Vector3 pos)
    {
        if (grid == null || gridComponent == null) return null;
        Vector3Int cell = gridComponent.WorldToCell(pos);
        Vector2Int gridPos = new Vector2Int(cell.y, cell.x);
        Slot slot = grid.GetValue(gridPos);
        return slot;
    }

    public bool IsFull()
    {
        if (grid == null) return false;
        bool fulled = true;
        grid.GridTraversal((pos, slot) =>
        {
            if (slot != null && slot.IsEmpty && !slot.IsLocked && slot.Type != SlotType.Block)
            {
                fulled = false;
            }
        });
        return fulled;
    }
}
