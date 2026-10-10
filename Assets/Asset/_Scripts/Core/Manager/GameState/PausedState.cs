public class PausedState : IState
{
    private GameManager gameManager;
    public PausedState(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }
    public void Enter()
    {
        gameManager.InputManager.ChangeToUI();
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