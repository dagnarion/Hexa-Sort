using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class SwapBooster : MonoBehaviour
{
    [SerializeField] private int remain;
    [SerializeField] private GridController gridController;
    [SerializeField] private EventChannel<Vector3> pressedEvent;
    [SerializeField] private EventChannel<Vector2Int> dropEvent;
    private Slot SelectedSlot;
    private bool isBoosterPlay;

    private void OnEnable()
    {
        pressedEvent.OnEventRaise += OnPressed;
    }

    private void OnDisable()
    {
        pressedEvent.OnEventRaise -= OnPressed;
    }

    private void Start()
    {
        isBoosterPlay = false;
    }

    public async void Apply()
    {
        if (isBoosterPlay || remain <= 0) return;
        isBoosterPlay = true;
    }


    private async void OnPressed(Vector3 pos)
    {
        if (!isBoosterPlay || remain <= 0) return;
        Slot slot = gridController.GetSlotOnPosition(pos);
        if (slot == null || slot.IsLocked)
        {
            if (SelectedSlot != null && !SelectedSlot.IsEmpty)
            {
                SelectedSlot.GetHexagonStack().Render.MoveDown();
                SelectedSlot = null;
            }
            return;
        }

        if (SelectedSlot == null)
        {
            if (slot.IsEmpty) return;
            SelectedSlot = slot;
            SelectedSlot.GetHexagonStack().Render.MoveUp();
        }
        else
        {
            if (slot == SelectedSlot)
            {
                if (!SelectedSlot.IsEmpty)
                {
                    SelectedSlot.GetHexagonStack().Render.MoveDown();
                }

                SelectedSlot = null;
            }
            else
            {
                Slot firstSlot = SelectedSlot;
                Slot secondSlot = slot;

                if (secondSlot.IsEmpty)
                {
                    SelectedSlot = null;
                    await SwapSlotNotHaveStack(firstSlot, secondSlot);
                    isBoosterPlay = false;
                    return;
                }

                SelectedSlot = null;
                await secondSlot.GetHexagonStack().Render.MoveUp().ToUniTask();
                await SwapSlotHasStack(firstSlot, secondSlot);

                isBoosterPlay = false;
                remain--;
            }
        }
    }

    private async UniTask SwapSlotHasStack(Slot origin, Slot target)
    {
        Sequence sq = DOTween.Sequence();
        HexagonStack currentStack = origin.GetHexagonStack();
        HexagonStack targetStack = target.GetHexagonStack();

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
}