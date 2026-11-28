using Cysharp.Threading.Tasks;

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
        int reward = gameManager.WinLoseManager.CompleteWinAndSave();
        WinConfetti.Play(UnityEngine.Camera.main).Forget();
        await gameManager.GridClearServices.Release();
        if (!gameManager) return;
        if (gameManager.StateMachine.CurrentState == this)
            gameManager.WinLoseManager.ShowWin(reward);
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
