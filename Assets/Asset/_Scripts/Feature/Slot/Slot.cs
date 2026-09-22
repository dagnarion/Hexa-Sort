using UnityEngine;

public class Slot : MonoBehaviour
{
    public Vector2Int Position { get; private set; }
    [field:SerializeField] public SlotType Type { get; private set; }
    [SerializeField] private SlotRender slotRender;
    private HexagonStack currentStack;
    public bool IsEmpty => currentStack == null;
    
    public void Init(SlotType type,Vector2Int position)
    {
        this.Type = type;
        this.Position = position;
    }

    public HexagonStack GetHexagonStack() => currentStack;

    public void FillHexagonStackToSlot(HexagonStack hexagonStack)  => this.currentStack = hexagonStack;
    public void ReleaseSlot() => this.currentStack = null;

    public void Deselected() => slotRender.Deselected();

    public void Selected() => slotRender.Selected();

}