using System;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [field:SerializeField] public LevelPopup LevelPopup { get; private set; }
    [field: SerializeField] public Transform holder { get; private set; }
    [SerializeField] private ComponentPoolSO<Hexagon> hexagonPool;
    [SerializeField] private ComponentPoolSO<HexagonStack> hexagonStackPool;
    [SerializeField] private ComponentPoolSO<Slot> slotPool;
    [SerializeField] private Canvas MainMenuCanvas;
    [SerializeField] private Canvas GamePlayCanvas;
    [field: SerializeField] public SaveLoadServices SaveLoadServices { get; private set; }
    [field: SerializeField] public GridClearServices GridClearServices { get; private set; }
    [field: SerializeField] public GridController GridController { get; private set; }
    [field: SerializeField] public TaskManager TaskManager { get; private set; }
    public LevelLoader LevelLoader { get; private set; }
    public WinLoseManager WinLoseManager { get; private set; }
    [field: SerializeField] public InputManager InputManager { get; private set; }
    [field: SerializeField] public HexagonStackSpawner HexagonStackSpawner { get; private set; }
    [Header("Main Menu Flow")]
    [SerializeField] private Button playButton;
    [SerializeField] private CanvasTransition canvasTransition;
    public StateMachine StateMachine { get; private set; }

    public WinState WinState { get; private set; }
    public LoseState LoseState { get; private set; }
    public PlayingState PlayingState { get; private set; }
    public MainMenuState MainMenuState { get; private set; }
    public PausedState PausedState { get; private set; }
    public PreparePlayingState PreparePlayingState { get; private set; }
    private bool isStartingGame;

    private void Awake()
    {
        Init();
        BindMainMenuPlayButton();
    }

    private void Start()
    {
        hexagonPool.InitPool(holder);
        hexagonStackPool.InitPool(holder);
        slotPool.InitPool(holder);
        StateMachine.ForceChangeState(MainMenuState);
        InputManager.ChangeToUI();
    }

    private void Init()
    {
        WinLoseManager = GetComponent<WinLoseManager>();
        LevelLoader = GetComponent<LevelLoader>();
        StateMachine = new StateMachine();
        WinState = new WinState(this);
        LoseState = new LoseState(this);
        PlayingState = new PlayingState(this);
        MainMenuState = new MainMenuState(this);
        PausedState = new PausedState(this);
        PreparePlayingState = new PreparePlayingState(this);
    }

    private void BindMainMenuPlayButton()
    {
        if (playButton == null)
        {
            foreach (Button button in FindObjectsByType<Button>(FindObjectsSortMode.None))
            {
                if (button.gameObject.name != "PlayButton_Level6") continue;
                playButton = button;
                break;
            }
        }

        if (canvasTransition == null)
            canvasTransition = FindFirstObjectByType<CanvasTransition>();

        if (playButton == null)
        {
            Debug.LogWarning("GameManager could not find a Button named PlayButton_Level6.", this);
            return;
        }

        playButton.onClick.RemoveListener(MainMenuState.OnPlayPressed);
        playButton.onClick.AddListener(MainMenuState.OnPlayPressed);
    }

    internal async void StartGameFromMainMenu()
    {
        if (isStartingGame || StateMachine.CurrentState != MainMenuState)
            return;

        isStartingGame = true;

        if (canvasTransition == null)
        {
            Debug.LogWarning("CanvasTransition is not present or assigned. Starting without the transition effect.", this);
            StateMachine.ChangeState(PreparePlayingState);
            isStartingGame = false;
            return;
        }

        try
        {

            await canvasTransition.PlayAsync(() =>
            {
                if (StateMachine.CurrentState == MainMenuState)
                {
                    MainMenuCanvas.gameObject.SetActive(false);
                    GamePlayCanvas.gameObject.SetActive(true);
                    StateMachine.ChangeState(PreparePlayingState);
                }
            });
        }
        finally
        {
            isStartingGame = false;
        }
    }

    private void Update()
    {
        StateMachine?.Tick();
    }

    private void FixedUpdate()
    {
        StateMachine?.FixedTick();
    }

    public async UniTask<bool> ReleaseHolderAsync()
    {
        if (GridController == null)
        {
            Debug.LogError("GameManager requires a GridController before releasing the board.", this);
            return false;
        }

        await GridController.StopMergesAndWaitAsync();
        GridController.ClearGridReference();

        for (int i = holder.childCount - 1; i >= 0; i--)
        {
            Transform child = holder.GetChild(i);
            if (!child.gameObject.activeSelf) continue;

            if (child.TryGetComponent<Slot>(out var slot))
            {
                HexagonStack slotStack = slot.GetHexagonStack();
                if (slotStack != null)
                {
                    ReleaseStackElements(slotStack);
                    slotStack.Release();
                }

                slot.ReleaseLock();
                slot.ReleaseSlot();
                slot.Deselected();
                slotPool.Release(slot);
                continue;
            }

            if (child.TryGetComponent<HexagonStack>(out var hexagonStack))
            {
                ReleaseStackElements(hexagonStack);
                hexagonStack.Release();
                continue;
            }

            if (child.TryGetComponent<Hexagon>(out var hexagon))
            {
                hexagon.ReleaseHexagon();
                continue;
            }

            if (child.TryGetComponent<ILock>(out var lockItem))
            {
                lockItem.ReleaseLock();
            }
        }

        return true;
    }

    private void ReleaseStackElements(HexagonStack hexagonStack)
    {
        for (int i = hexagonStack.GetNumberOfElement() - 1; i >= 0; i--)
        {
            Hexagon hexa = hexagonStack.GetElement(i);
            if (hexa != null)
            {
                hexa.ReleaseHexagon();
            }
        }
        hexagonStack.Clear();

        for (int j = hexagonStack.transform.childCount - 1; j >= 0; j--)
        {
            Transform hexaChild = hexagonStack.transform.GetChild(j);
            if (hexaChild.TryGetComponent<Hexagon>(out var hexagon))
            {
                hexagon.ReleaseHexagon();
            }
        }
    }

    public void PauseGame()
    {
        StateMachine.ChangeState(PausedState);
    }

    public void ResumeGame()
    {
        StateMachine.ChangeState(PlayingState);
    }
    
}
