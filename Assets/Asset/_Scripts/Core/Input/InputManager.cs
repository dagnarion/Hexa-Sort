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

    private void Start()
    {
        ChangeToGamePlay(); // test
    }

    private void ChangeToUI()
    {
        InputAction.GamePlay.Disable();
        InputAction.UI.Enable();
    }
    
    private void ChangeToGamePlay()
    {
        InputAction.GamePlay.Enable();
        InputAction.UI.Disable();
    }
    
}
