public class WinState : IState
{
    private GameManager gameManager;
    public WinState(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }
    public async void Enter()
    {
        gameManager.InputManager.ChangeToUI();
        await gameManager.GridClearServices.Release();
        gameManager.WinLoseManager.CompleteWinAndSave();
        gameManager.StateMachine.ChangeState(gameManager.PreparePlayingState);
    }

    public void Tick()
    {

    }

    public void FixedTick()
    {
        
    }

    public void Exit()
    {
    }
}
