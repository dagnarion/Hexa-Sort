public class WinState : IState
{
    private GameManager gameManager;
    public WinState(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }
    public void Enter()
    {
        gameManager.SaveLoadServices.Save();
        // instance + popup UI ra
    }

    public void Tick()
    {
        
    }

    public void FixedTick()
    {
        
    }

    public void Exit()
    {
        // out ui
    }
}