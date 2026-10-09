using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [field:SerializeField] public Transform holder { get; private set; }
    [SerializeField] private ComponentPoolSO<Hexagon> hexagonPool;
    [SerializeField] private ComponentPoolSO<HexagonStack> hexagonStackPool;
    [SerializeField] private ComponentPoolSO<Slot> slotPool;
    
    [field:SerializeField] public SaveLoadServices SaveLoadServices { get; private set; }
    [field:SerializeField] public TaskManager TaskManager { get; private set; }
    [field:SerializeField] public HexagonStackSpawner HexagonStackSpawner { get; private set; }
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
        foreach (Transform child in holder)
        {
            child.SetParent(null);
            
            if (child.TryGetComponent<Slot>(out var slot))
            {
                slotPool.Release(slot);
            }         
            
            if (child.TryGetComponent<HexagonStack>(out var hexagonStack))
            {
                foreach (Transform hexaChild in child)
                {
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
