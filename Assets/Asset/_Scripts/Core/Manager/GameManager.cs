using System;
using NaughtyAttributes;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [field: SerializeField] public Transform holder { get; private set; }
    [SerializeField] private ComponentPoolSO<Hexagon> hexagonPool;
    [SerializeField] private ComponentPoolSO<HexagonStack> hexagonStackPool;
    [SerializeField] private ComponentPoolSO<Slot> slotPool;

    [field: SerializeField] public SaveLoadServices SaveLoadServices { get; private set; }
    [field: SerializeField] public GridClearServices GridClearServices { get; private set; }
    [field: SerializeField] public TaskManager TaskManager { get; private set; }
    [field: SerializeField] public InputManager InputManager { get; private set; }
    [field: SerializeField] public HexagonStackSpawner HexagonStackSpawner { get; private set; }
    public StateMachine StateMachine { get; private set; }

    public WinState WinState { get; private set; }
    public LoseState LoseState { get; private set; }
    public PlayingState PlayingState { get; private set; }
    public UIState UIState { get; private set; }
    public PausedState PausedState { get; private set; }
    public PreparePlayingState PreparePlayingState { get; private set; }

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        hexagonPool.InitPool(holder);
        hexagonStackPool.InitPool(holder);
        slotPool.InitPool(holder);
        StateMachine.ForceChangeState(PreparePlayingState);
        InputManager.ChangeToUI();
    }

    private void Init()
    {
        StateMachine = new StateMachine();
        WinState = new WinState(this);
        LoseState = new LoseState(this);
        PlayingState = new PlayingState(this);
        UIState = new UIState(this);
        PausedState = new PausedState(this);
        PreparePlayingState = new PreparePlayingState(this);
    }

    private void Update()
    {
        StateMachine?.Tick();
    }

    private void FixedUpdate()
    {
        StateMachine?.FixedTick();
    }

    public void ReleaseHolder()
    {
        for (int i = holder.childCount - 1; i >= 0; i--)
        {
            Transform child = holder.GetChild(i);
            child.SetParent(null);
            if (child.TryGetComponent<Slot>(out var slot))
            {
                slot.Deselected();
                slotPool.Release(slot);
            }

            if (child.TryGetComponent<HexagonStack>(out var hexagonStack))
            {
                for (int j = child.childCount - 1; j >= 0; j--)
                {
                    Transform hexaChild = child.GetChild(j);
                    if (hexaChild.TryGetComponent<Hexagon>(out var hexagon))
                    {
                        hexagonPool.Release(hexagon);
                    }
                }

                hexagonStackPool.Release(hexagonStack);
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