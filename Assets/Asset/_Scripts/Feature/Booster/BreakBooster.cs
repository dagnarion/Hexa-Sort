using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class BreakBooster : SaveLoadAbstract
{
    [SerializeField] private int amount;
    [SerializeField] private GridController gridController;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private SaveLoadServices saveLoadServices;
    [SerializeField] private EventChannel<Vector3> pressedEventChannel;
    [SerializeField] private TMP_Text buttonLabel;

    private bool isSelected;
    private bool isApplying;

    private void Awake()
    {
        if (buttonLabel == null) buttonLabel = GetComponentInChildren<TMP_Text>();
        UpdateButtonLabel();
    }

    private void OnEnable()
    {
        if (pressedEventChannel != null) pressedEventChannel.OnEventRaise += OnPressed;
        if (inputManager != null) inputManager.BoosterModeChanged += OnBoosterModeChanged;
    }

    private void OnDisable()
    {
        if (pressedEventChannel != null) pressedEventChannel.OnEventRaise -= OnPressed;
        if (inputManager != null)
        {
            inputManager.BoosterModeChanged -= OnBoosterModeChanged;
            inputManager.CancelBoosterMode(this);
        }
        isSelected = false;
        isApplying = false;
    }

    public void SetBoosterState()
    {
        if (isApplying) return;
        if (isSelected)
        {
            inputManager.CancelBoosterMode(this);
            return;
        }

        if (amount <= 0 || isApplying || gridController == null || !gridController.IsMergeIdle) return;
        if (inputManager != null && inputManager.ActivateBoosterMode(this))
        {
            isSelected = true;
        }
    }

    private void OnBoosterModeChanged(MonoBehaviour activeBooster)
    {
        isSelected = activeBooster == this;
    }

    private void OnPressed(Vector3 pos)
    {
        if (isSelected && !isApplying) ApplyAsync(pos).Forget();
    }

    private async UniTaskVoid ApplyAsync(Vector3 pos)
    {
        if (amount <= 0 || !isSelected || isApplying || gridController == null || !gridController.IsMergeIdle) return;
        Slot slot = gridController.GetSlotOnPosition(pos);
        if (slot == null)
        {
            inputManager.CancelBoosterMode(this);
            return;
        }
        if (slot.IsEmpty || slot.IsLocked || slot.Type == SlotType.Block) return;

        HexagonStack stack = slot.GetHexagonStack();
        if (stack == null || !stack.gameObject.activeInHierarchy || stack.IsEmpty) return;
        if (inputManager == null || !inputManager.BeginBoosterAction(this)) return;

        isApplying = true;
        try
        {
            if (!isSelected || slot.GetHexagonStack() != stack || !gridController.IsMergeIdle) return;

            slot.ReleaseSlot();
            List<Hexagon> hexagons = new List<Hexagon>();
            for (int i = stack.GetNumberOfElement() - 1; i >= 0; i--)
            {
                Hexagon hexagon = stack.GetElement(i);
                if (hexagon == null) continue;
                hexagon.SetParent(slot.transform.parent);
                hexagons.Add(hexagon);
            }

            stack.Clear();
            stack.Release();
            await ReleaseHexagonsAsync(hexagons);

            amount = Mathf.Max(0, amount - 1);
            UpdateButtonLabel();
            inputManager?.CancelBoosterMode(this);
        }
        finally
        {
            isApplying = false;
            inputManager?.EndBoosterAction(this);
        }
    }

    private static async UniTask ReleaseHexagonsAsync(List<Hexagon> hexagons)
    {
        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < hexagons.Count; i++)
        {
            if (hexagons[i] != null) sequence.Insert(0.05f * i, hexagons[i].render.ReleaseHexagon(false));
        }

        await sequence.ToUniTask();
        foreach (Hexagon hexagon in hexagons)
        {
            if (hexagon != null) hexagon.ReleaseHexagon();
        }
    }
    

    public override void Save(GameData gameData)
    {
        inputManager?.CancelBoosterMode(this);
        isSelected = false;
        gameData.BreakBoosterRemain = amount;
    }

    public override void Load(GameData gameData)
    {
        inputManager?.CancelBoosterMode(this);
        isSelected = false;
        isApplying = false;
        amount = Mathf.Max(0, gameData.BreakBoosterRemain);
        UpdateButtonLabel();
    }

    private void UpdateButtonLabel()
    {
        if (buttonLabel != null) buttonLabel.text = amount.ToString();
    }
}
