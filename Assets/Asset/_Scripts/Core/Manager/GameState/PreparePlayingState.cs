using Cysharp.Threading.Tasks;

public class PreparePlayingState : IState
{
    private GameManager gameManager;
    private bool isPreparing;
    private bool isReady;
    public PreparePlayingState(GameManager manager)
    {
        gameManager = manager;
    }
    public void Enter()
    {
        gameManager.InputManager.ChangeToUI();
        isReady = false;
        if (isPreparing) return;
        isPreparing = true;
        PrepareAsync().Forget();
    }

    public void Tick()
    {
        if (!isReady) return;
        isReady = false;
        gameManager.StateMachine.ChangeState(gameManager.PlayingState);
    }

    private async UniTask PrepareAsync()
    {
        try
        {
            if (!await gameManager.ReleaseHolderAsync()) return;
            gameManager.HexagonStackSpawner.Release();
            gameManager.SaveLoadServices.Load();
            await gameManager.HexagonStackSpawner.Spawn();
            isReady = true;
        }
        finally
        {
            isPreparing = false;
        }
    }

    public void FixedTick()
    {
        
    }

    public void Exit()
    {
       
    }
}
