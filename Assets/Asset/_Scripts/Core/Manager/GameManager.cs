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
            if (!child.gameObject.activeSelf) continue;

            if (child.TryGetComponent<Slot>(out var slot))
            {
                HexagonStack slotStack = slot.GetHexagonStack();
                if (slotStack != null)
                {
                    ReleaseStackElements(slotStack);
                    slotStack.Release();
                }

                for (int j = child.childCount - 1; j >= 0; j--)
                {
                    Transform slotChild = child.GetChild(j);
                    if (slotChild.TryGetComponent<HexagonStack>(out var childStack))
                    {
                        ReleaseStackElements(childStack);
                        childStack.Release();
                    }
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