using System.Collections.Generic;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BreakBooster : SaveLoadAbstract
{
    private int amount;
    private bool IsOnBooster;
    [SerializeField] private GridController gridController;
    [SerializeField] private EventChannel<Vector3> pressedEventChannel;

    private void OnEnable()
    {
        pressedEventChannel.OnEventRaise += OnPressed;
    }

    private void OnDisable()
    {
        pressedEventChannel.OnEventRaise -= OnPressed;
    }
    
    public void SetBoosterState()
    {
        IsOnBooster = true;
    }

    private async void OnPressed(Vector3 pos)
    {
        await Apply(pos);
    }
    
    private async UniTask Apply(Vector3 pos)
    {
        if (amount <= 0 || !IsOnBooster) return;
        Slot slot = gridController.GetSlotOnPosition(pos);
        if (slot == null || slot.IsEmpty || slot.IsLocked || slot.Type == SlotType.Block) return;
        HexagonStack stack = slot.GetHexagonStack();
        if (stack == null) return;

        IsOnBooster = false;
        amount--;
        slot.ReleaseSlot();

        List<Hexagon> hexagons = new List<Hexagon>();
        for (int i = stack.GetNumberOfElement() - 1; i >= 0; i--)
        {
            Hexagon hexa = stack.GetElement(i);
            if (hexa != null)
            {
                hexa.SetParent(slot.transform.parent);
                hexagons.Add(hexa);
            }
        }
        stack.Clear();
        stack.Release();

        await ReleaseHexagon(hexagons);
    }

    public async UniTask ReleaseHexagon(List<Hexagon> hexagons)
    {
        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < hexagons.Count; i++)
        {
            sequence.Insert(0.05f * i, hexagons[i].render.ReleaseHexagon());
        }
        await sequence.ToUniTask();
        for (int i = 0; i < hexagons.Count; i++)
        {
            if (hexagons[i] != null)
                hexagons[i].ReleaseHexagon();
        }
    }

    public override void Save(GameData gameData)
    {
        IsOnBooster = false;
        gameData.BreakBoosterRemain = amount;
    }

    public override void Load(GameData gameData)
    {
        IsOnBooster = false;
        amount = gameData.BreakBoosterRemain;
    }
}
