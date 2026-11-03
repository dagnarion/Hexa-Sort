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
        gameManager.WinLoseManager.ShowWin();
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
