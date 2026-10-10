using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class InputManager : MonoBehaviour
{
    private PlayerInputSetup _inputAction;
    public PlayerInputSetup InputAction
    {
        get
        {
            if (_inputAction == null)
            {
                _inputAction = new PlayerInputSetup();
            }
            return _inputAction;
        }
    }

    public void ChangeToUI()
    {
        InputAction.GamePlay.Disable();
        InputAction.UI.Enable();
    }
    
    public void ChangeToGamePlay()
    {
        InputAction.GamePlay.Enable();
        InputAction.UI.Disable();
    }
    
}
