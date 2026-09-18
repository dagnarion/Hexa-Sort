using UnityEngine;

public class Slot : MonoBehaviour
{
    [SerializeField] private SlotType Type;
    private HexagonStack currentStack;
    public bool IsEmpty => currentStack == null;
    
    public void Init(SlotType type)
    {
        this.Type = type;
    }

}