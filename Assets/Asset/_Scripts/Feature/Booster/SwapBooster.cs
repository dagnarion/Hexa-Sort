using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class SwapBooster : SaveLoadAbstract
{
    [SerializeField] private int remain;
    [SerializeField] private GridController gridController;
    [SerializeField] private EventChannel<Vector3> pressedEvent;
    [SerializeField] private EventChannel<Vector2Int> dropEvent;
    private Slot SelectedSlot;
    private bool isBoosterPlay;
    private bool isProcessing;

    private void OnEnable()
    {
        pressedEvent.OnEventRaise += OnPressed;
    }

    private void OnDisable()
    {
        pressedEvent.OnEventRaise -= OnPressed;
        SelectedSlot = null;
        isProcessing = false;
    }

    public void Apply()
    {
        if (isBoosterPlay || isProcessing || remain <= 0) return;
        isBoosterPlay = true;
    }

    private async void OnPressed(Vector3 pos)
    {
        if (!isBoosterPlay || isProcessing || remain <= 0) return;
        isProcessing = true;
        try
        {
            Slot slot = gridController.GetSlotOnPosition(pos);
            if (slot == null || slot.IsLocked || slot.Type == SlotType.Block)
            {
                if (SelectedSlot != null && !SelectedSlot.IsEmpty)
                {
                    await SelectedSlot.GetHexagonStack().Render.MoveDown().ToUniTask();
                }
                SelectedSlot = null;
                return;
            }

            if (SelectedSlot == null)
            {
                if (slot.IsEmpty) return;
                SelectedSlot = slot;
                await SelectedSlot.GetHexagonStack().Render.MoveUp().ToUniTask();
            }
            else
            {
                if (slot == SelectedSlot || SelectedSlot.IsEmpty)
                {
                    if (!SelectedSlot.IsEmpty)
                    {
                        await SelectedSlot.GetHexagonStack().Render.MoveDown().ToUniTask();
                    }
                    SelectedSlot = null;
                }
                else
                {
                    Slot firstSlot = SelectedSlot;
                    Slot secondSlot = slot;
                    SelectedSlot = null;

                    if (secondSlot.IsEmpty)
                    {
                        await SwapSlotNotHaveStack(firstSlot, secondSlot);
                        isBoosterPlay = false;
                        remain--;
                        return;
                    }

                    await secondSlot.GetHexagonStack().Render.MoveUp().ToUniTask();
                    await SwapSlotHasStack(firstSlot, secondSlot);

                    isBoosterPlay = false;
                    remain--;
                }
            }
        }
        finally
        {
            isProcessing = false;
        }
    }

    private async UniTask SwapSlotHasStack(Slot origin, Slot target)
    {
        Sequence sq = DOTween.Sequence();
        HexagonStack currentStack = origin.GetHexagonStack();
        HexagonStack targetStack = target.GetHexagonStack();
        if (currentStack == null || targetStack == null) return;

        currentStack.transform.SetParent(null);
        targetStack.transform.SetParent(null);

        sq.Join(currentStack.Render.LerpToTargetPosition(
            target.transform.position.With(y: currentStack.transform.position.y)));
        sq.Join(targetStack.Render.LerpToTargetPosition(
            origin.transform.position.With(y: targetStack.transform.position.y)));

        await sq.ToUniTask();
        sq.Kill();

        currentStack.transform.SetParent(target.transform);
        targetStack.transform.SetParent(origin.transform);

        currentStack.Render.SetUpAndDownPosition(target.transform.position);
        targetStack.Render.SetUpAndDownPosition(origin.transform.position);

        origin.FillHexagonStackToSlot(targetStack);
        target.FillHexagonStackToSlot(currentStack);

        await UniTask.WhenAll(
            currentStack.Render.MoveDown().ToUniTask(),
            targetStack.Render.MoveDown().ToUniTask()
        );

        dropEvent.Raise(origin.Position);
        dropEvent.Raise(target.Position);
    }

    private async UniTask SwapSlotNotHaveStack(Slot origin, Slot target)
    {
        Sequence sq = DOTween.Sequence();
        HexagonStack currentStack = origin.GetHexagonStack();
        if (currentStack == null) return;
        currentStack.transform.SetParent(null);

        sq.Join(currentStack.Render.LerpToTargetPosition(
            target.transform.position.With(y: currentStack.transform.position.y)));

        await sq.ToUniTask();
        sq.Kill();

        currentStack.transform.SetParent(target.transform);
        currentStack.Render.SetUpAndDownPosition(target.transform.position);

        target.FillHexagonStackToSlot(currentStack);
        origin.FillHexagonStackToSlot(null);

        await currentStack.Render.MoveDown().ToUniTask();
        dropEvent.Raise(target.Position);
    }

    public override void Save(GameData gameData)
    {
        isBoosterPlay = false;
        isProcessing = false;
        SelectedSlot = null;
        gameData.SwapBoosterRemain = remain;
    }

    public override void Load(GameData gameData)
    {
        isBoosterPlay = false;
        isProcessing = false;
        SelectedSlot = null;
        remain = gameData.SwapBoosterRemain;
    }
}