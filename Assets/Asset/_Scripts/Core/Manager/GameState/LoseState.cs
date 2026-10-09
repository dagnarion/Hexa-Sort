using UnityEngine;

public class LoseState : IState
{
    private GameManager gameManager;
    public LoseState(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }
    public void Enter()
    {
        Debug.Log("Lose");
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