using UnityEditor.Experimental.GraphView;
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
        if (gameManager.TaskManager.IsLose())
        {
            gameManager.StateMachine.ChangeState(gameManager.LoseState);
            return;
        }

        if (gameManager.TaskManager.IsWin())
        {
            gameManager.StateMachine.ChangeState(gameManager.WinState);
        }
    }

    public void FixedTick()
    {
    }

    public void Exit()
    {
    }
}