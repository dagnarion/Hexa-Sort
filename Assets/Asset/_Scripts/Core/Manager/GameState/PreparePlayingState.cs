public class PreparePlayingState : IState
{
    private GameManager gameManager;
    public PreparePlayingState(GameManager manager)
    {
        gameManager = manager;
    }
    public void Enter()
    {
       gameManager.ReleaseHolder(); 
       gameManager.SaveLoadServices.Load();
       gameManager.HexagonStackSpawner.Spawn();
    }

    public void Tick()
    {
       gameManager.StateMachine.ChangeState(gameManager.PlayingState);
    }

    public void FixedTick()
    {
        
    }

    public void Exit()
    {
       
    }
}