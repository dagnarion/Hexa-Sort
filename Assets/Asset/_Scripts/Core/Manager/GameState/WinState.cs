using UnityEngine;

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
        gameManager.SaveLoadServices.Save();
        await gameManager.GridClearServices.Release();
        if (gameManager == null) return;
        gameManager.StateMachine.ChangeState(gameManager.PreparePlayingState);
    }

    public void Tick()
    {

    }

    public void FixedTick()
    {
        
    }

    public void Exit()
    {
        // out ui
    }
}