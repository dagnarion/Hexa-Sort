public class StateMachine
{
    public IState PreviousState { get; private set; }
    public IState CurrentState { get; private set; }

    public void Tick() => CurrentState?.Tick();

    public void FixedTick() => CurrentState?.FixedTick();

    public void ChangeState(IState state)
    {
        if (CurrentState == state) return;
        PreviousState = CurrentState;
        CurrentState?.Exit();
        CurrentState = state;
        CurrentState?.Enter();
    }

    public void ForceChangeState(IState state)
    {
        PreviousState = CurrentState;
        CurrentState?.Exit();
        CurrentState = state;
        CurrentState?.Enter();
    }
}