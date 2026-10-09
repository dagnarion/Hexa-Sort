using UnityEngine;
using Cysharp.Threading.Tasks;

public class Slot : MonoBehaviour
{
    public Vector2Int Position { get; private set; }
    public SlotType Type { get; private set; }
    private ILock currentLock;
    public bool IsEmpty => currentStack == null;
    public bool IsLocked { get; private set; }
    [SerializeField] private EventChannel<int> OnLockBreaked;
    private HexagonStack currentStack;
    [SerializeField] private SlotRender slotRender;
    public void Init(SlotType type, Vector2Int position, HexagonStack stack, ILock Lock)
    {
        this.Type = type;
        this.Position = position;
        this.currentStack = stack;
        this.IsLocked = false;
        this.currentLock = null;
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
            OnLockBreaked?.Raise(1);
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

    public void FillHexagonStackToSlot(HexagonStack hexagonStack) => this.currentStack = hexagonStack;

    public void ReleaseSlot() => this.currentStack = null;

    public void Deselected() => slotRender.Deselected();

    public void Selected() => slotRender.Selected();
}