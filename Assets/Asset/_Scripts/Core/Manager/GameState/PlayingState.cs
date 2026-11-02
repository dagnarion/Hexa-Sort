using UnityEngine;

public class PlayingState : IState
{
    private GameManager gameManager;

    public PlayingState(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    public void Enter()
    {
        gameManager.InputManager.ChangeToGamePlay();
    }

    public void Tick()
    {
        WinLoseManager.Result result = gameManager.WinLoseManager.Evaluate(
            gameManager.InputManager.InputAction.GamePlay.Press.IsPressed());

        if (result == WinLoseManager.Result.Win)
        {
            gameManager.StateMachine.ChangeState(gameManager.WinState);
            return;
        }

        if (result == WinLoseManager.Result.Lose)
        {
            gameManager.StateMachine.ChangeState(gameManager.LoseState);
        }
    }

    public void FixedTick()
    {
    }

    public void Exit()
    {
    }
}
