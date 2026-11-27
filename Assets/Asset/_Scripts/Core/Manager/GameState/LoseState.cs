public class LoseState : IState
{
    private GameManager gameManager;
    public LoseState(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }
    public void Enter()
    {
        gameManager.SaveLoadServices.LoseLife();
        gameManager.InputManager.ChangeToUI();
        gameManager.WinLoseManager.ShowLose();
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
