public class UIState : IState
{
    private GameManager gameManager;
    public UIState(GameManager gameManager)
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