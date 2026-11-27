using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class SwapBooster : SaveLoadAbstract
{
    [SerializeField] private int remain;
    [SerializeField] private GridController gridController;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private SaveLoadServices saveLoadServices;
    [SerializeField] private EventChannel<Vector3> pressedEvent;
    [SerializeField] private EventChannel<Vector2Int> dropEvent;
    [SerializeField] private TMP_Text buttonLabel;

    private Slot selectedSlot;
    private bool isSelected;
    private bool isProcessing;
    private bool isCommitting;

    private void Awake()
    {
        if (buttonLabel == null) buttonLabel = GetComponentInChildren<TMP_Text>();
        UpdateButtonLabel();
    }

    private void OnEnable()
    {
        if (pressedEvent != null) pressedEvent.OnEventRaise += OnPressed;
        if (inputManager != null) inputManager.BoosterModeChanged += OnBoosterModeChanged;
    }

    private void OnDisable()
    {
        if (pressedEvent != null) pressedEvent.OnEventRaise -= OnPressed;
        if (inputManager != null)
        {
            inputManager.BoosterModeChanged -= OnBoosterModeChanged;
            inputManager.CancelBoosterMode(this);
        }
        ClearSelectedSlot(false);
        isSelected = false;
        isProcessing = false;
        isCommitting = false;
    }

    public void Apply()
    {
        if (isProcessing)
        {
            if (isSelected && !isCommitting) inputManager?.CancelBoosterMode(this);
            return;
        }
        if (isSelected)
        {
            inputManager.CancelBoosterMode(this);
            return;
        }

        if (remain <= 0 || gridController == null || !gridController.IsMergeIdle || inputManager == null) return;
        if (inputManager.ActivateBoosterMode(this))
        {
            isSelected = true;
        }
    }

    private void OnBoosterModeChanged(MonoBehaviour activeBooster)
    {
        if (activeBooster == this)
        {
            isSelected = true;
        }
        else
        {
            ClearSelectedSlot(!isCommitting);
            isSelected = false;
        }
    }

    private void OnPressed(Vector3 pos)
    {
        if (isSelected && !isProcessing) HandlePressedAsync(pos).Forget();
    }

    private async UniTaskVoid HandlePressedAsync(Vector3 pos)
    {
        if (!isSelected || isProcessing || remain <= 0 || gridController == null || !gridController.IsMergeIdle) return;
        Slot slot = gridController.GetSlotOnPosition(pos);
        if (slot == null)
        {
            inputManager.CancelBoosterMode(this);
            return;
        }
        if (slot.IsLocked || slot.Type == SlotType.Block) return;

        if (selectedSlot == null)
        {
            if (slot.IsEmpty) return;
            HexagonStack firstStack = slot.GetHexagonStack();
            if (firstStack == null || !firstStack.gameObject.activeInHierarchy) return;
            selectedSlot = slot;
            isProcessing = true;
            try
            {
                await firstStack.Render.MoveUp().ToUniTask();
            }
            finally
            {
                isProcessing = false;
            }
            return;
        }

        if (slot == selectedSlot)
        {
            inputManager.CancelBoosterMode(this);
            return;
        }

        HexagonStack originStack = selectedSlot.GetHexagonStack();
        HexagonStack targetStack = slot.GetHexagonStack();
        if (originStack == null || !originStack.gameObject.activeInHierarchy ||
            (!slot.IsEmpty && (targetStack == null || !targetStack.gameObject.activeInHierarchy)))
        {
            ClearSelectedSlot(false);
            return;
        }

        Slot originSlot = selectedSlot;
        if (inputManager == null || !inputManager.BeginBoosterAction(this)) return;
        isProcessing = true;
        isCommitting = true;
        try
        {
            if (targetStack != null)
                await targetStack.Render.MoveUp().ToUniTask();

            Sequence travel = DOTween.Sequence();
            travel.Join(originStack.Render.MoveToRaisedSlot(slot.transform));
            if (targetStack != null)
                travel.Join(targetStack.Render.MoveToRaisedSlot(originSlot.transform));
            await travel.ToUniTask();

            originSlot.FillHexagonStackToSlot(targetStack);
            slot.FillHexagonStackToSlot(originStack);
            Sequence landing = DOTween.Sequence();
            landing.Join(originStack.Render.MoveDown());
            if (targetStack != null)
                landing.Join(targetStack.Render.MoveDown());
            await landing.ToUniTask();

            selectedSlot = null;
            remain = Mathf.Max(0, remain - 1);
            UpdateButtonLabel();
            isSelected = false;
            inputManager.CancelBoosterMode(this);
            dropEvent?.Raise(originSlot.Position);
            dropEvent?.Raise(slot.Position);
        }
        finally
        {
            isProcessing = false;
            isCommitting = false;
            inputManager?.EndBoosterAction(this);
        }
    }

    private void ClearSelectedSlot(bool animateDown)
    {
        Slot oldSlot = selectedSlot;
        selectedSlot = null;
        if (!animateDown || oldSlot == null || oldSlot.IsEmpty) return;
        HexagonStack stack = oldSlot.GetHexagonStack();
        if (stack != null && stack.gameObject.activeInHierarchy) stack.Render.MoveDown();
    }

    public override void Save(GameData gameData)
    {
        inputManager?.CancelBoosterMode(this);
        ClearSelectedSlot(false);
        isSelected = false;
        gameData.SwapBoosterRemain = remain;
    }

    public override void Load(GameData gameData)
    {
        inputManager?.CancelBoosterMode(this);
        ClearSelectedSlot(false);
        isSelected = false;
        isProcessing = false;
        isCommitting = false;
        remain = Mathf.Max(0, gameData.SwapBoosterRemain);
        UpdateButtonLabel();
    }

    private void UpdateButtonLabel()
    {
        if (buttonLabel != null) buttonLabel.text = remain.ToString();
    }
}
