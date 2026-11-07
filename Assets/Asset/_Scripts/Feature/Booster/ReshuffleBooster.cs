using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class ReshuffleBooster : SaveLoadAbstract
{
    [SerializeField] private int remain;
    [SerializeField] private HexagonStackSpawner hexagonStackSpawner;
    [SerializeField] private GridController gridController;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private SaveLoadServices saveLoadServices;
    [SerializeField] private TMP_Text buttonLabel;

    private bool isSelected;
    private bool isProcessing;

    private void Awake()
    {
        if (buttonLabel == null) buttonLabel = GetComponentInChildren<TMP_Text>();
        UpdateButtonLabel();
    }

    private void OnEnable()
    {
        if (inputManager != null) inputManager.BoosterModeChanged += OnBoosterModeChanged;
    }

    private void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.BoosterModeChanged -= OnBoosterModeChanged;
            inputManager.CancelBoosterMode(this);
        }
        isSelected = false;
        isProcessing = false;
    }

    public void Reshuffle()
    {
        if (isProcessing) return;
        if (isSelected)
        {
            inputManager.CancelBoosterMode(this);
            return;
        }

        if (remain <= 0 || hexagonStackSpawner == null || !hexagonStackSpawner.IsReady ||
            gridController == null || !gridController.IsMergeIdle || inputManager == null) return;

        if (!inputManager.ActivateBoosterMode(this)) return;
        if (!inputManager.BeginBoosterAction(this))
        {
            inputManager.CancelBoosterMode(this);
            return;
        }
        isSelected = true;
        ReshuffleAsync().Forget();
    }

    private async UniTaskVoid ReshuffleAsync()
    {
        isProcessing = true;
        try
        {
            bool completed = await hexagonStackSpawner.ReshuffleAsync();
            if (!completed || !inputManager.IsGameplayEnabled || !inputManager.IsBoosterModeActive(this)) return;

            remain = Mathf.Max(0, remain - 1);
            UpdateButtonLabel();
            saveLoadServices?.Save();
        }
        finally
        {
            isProcessing = false;
            isSelected = false;
            inputManager?.CancelBoosterMode(this);
            inputManager?.EndBoosterAction(this);
        }
    }

    private void OnBoosterModeChanged(MonoBehaviour activeBooster)
    {
        isSelected = activeBooster == this && !isProcessing;
 
    }

    public override void Save(GameData gameData)
    {
        inputManager?.CancelBoosterMode(this);
        isSelected = false;
        gameData.ShuffleBoosterRemain = remain;
    }

    public override void Load(GameData gameData)
    {
        inputManager?.CancelBoosterMode(this);
        isSelected = false;
        isProcessing = false;
        remain = Mathf.Max(0, gameData.ShuffleBoosterRemain);
        UpdateButtonLabel();
    }

    private void UpdateButtonLabel()
    {
        if (buttonLabel != null) buttonLabel.text = remain.ToString();
    }
}
