public class MainMenuState : IState
{
    private GameManager gameManager;
    public MainMenuState(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }
    public void Enter()
    {
        gameManager.InputManager.ChangeToUI();
    }

    public void OnPlayPressed()
    {
        gameManager.StartGameFromMainMenu();
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
