using UnityEngine;

public class Slot : MonoBehaviour
{
    [SerializeField] private SlotType Type;
    [SerializeField] private SlotRender slotRender;
    private HexagonStack currentStack;
    public bool IsEmpty => currentStack == null;
    
    public void Init(SlotType type)
    {
        this.Type = type;
    }

    public void Deselected() => slotRender.Deselected();

    public void Selected() => slotRender.Selected();

}