using UnityEngine;
using Cysharp.Threading.Tasks;
public class Slot : MonoBehaviour
{
    public Vector2Int Position { get; private set; }
    
  
    [SerializeField] private SlotRender slotRender;
    
    private ILock currentLock;
    private HexagonStack currentStack;
    
    public bool IsEmpty => currentStack == null;
   [field:SerializeField] public bool IsLocked { get; private set; }
    
    public void Init(SlotType type,Vector2Int position,HexagonStack stack,ILock Lock)
    {
        this.currentLock = Lock;
        this.Position = position;
        this.currentStack = stack;
        SetLock(Lock);
    }
    
    public void SetLock(ILock lockItem)
    {
        if (currentLock != null)
            currentLock.OnUnlocked -= HandleLockUnlocked;
        currentLock = lockItem;
        if (currentLock != null)
        {
            currentLock.OnUnlocked += HandleLockUnlocked;
            IsLocked = true;
        }
    }
    
    private void HandleLockUnlocked(ILock unlockedLock)
    {
        if (currentLock == unlockedLock)
        {
            IsLocked = false;
            currentLock.OnUnlocked -= HandleLockUnlocked;
            currentLock = null;
        }
    }
    
    public async UniTask TryHitLockAsync()
    {
        if (currentLock is IHittableLock hittableLock)
        {
            await hittableLock.TakeHitAsync(1);
        }
    }


  
    public HexagonStack GetHexagonStack() => currentStack;

    public void FillHexagonStackToSlot(HexagonStack hexagonStack)  => this.currentStack = hexagonStack;
    
    public void ReleaseSlot() => this.currentStack = null;

    public void Deselected() => slotRender.Deselected();

    public void Selected() => slotRender.Selected();

}