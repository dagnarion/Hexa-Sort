using Cysharp.Threading.Tasks;

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
        gameManager.HexagonStackSpawner.Release();
        gameManager.SaveLoadServices.Load();
        gameManager.HexagonStackSpawner.Spawn().Forget();
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