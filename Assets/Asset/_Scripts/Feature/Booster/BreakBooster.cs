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
        if(amount <= 0 || !IsOnBooster) return;
        Slot slot = gridController.GetSlotOnPosition(pos);
        if(slot == null || slot.IsEmpty || slot.IsLocked) return;
        List<Hexagon> hexagons = new List<Hexagon>();
        HexagonStack stack = slot.GetHexagonStack();
        for (int i = stack.GetNumberOfElement()-1; i >= 0; i--)
        {
            hexagons.Add(stack.GetElement(i));
        }
        await ReleaseHexagon(hexagons);
        
        for (int i = 0; i < stack.GetNumberOfElement(); i++)
        {
            stack.RemoveElement(i);
        }
        
        slot?.ReleaseSlot();
        stack.transform.SetParent(null);
        stack.gameObject.SetActive(false);
        IsOnBooster = false;
        amount--;
    }
    
    public async UniTask ReleaseHexagon(List<Hexagon> hexagons)
    {
        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < hexagons.Count; i++)
        {
            sequence.Insert(0.05f * i, hexagons[i].render.ReleaseHexagon());
        }
        await sequence.ToUniTask();
    }

    public override void Save(GameData gameData)
    {
        gameData.BreakBoosterRemain = amount;
    }

    public override void Load(GameData gameData)
    {
        amount = gameData.BreakBoosterRemain;
    }
}
