using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class InputManager : MonoBehaviour
{
    private PlayerInputSetup _inputAction;
    public bool IsGameplayEnabled { get; private set; }
    public bool IsBoosterInputLocked => activeBooster != null || processingBooster != null;
    public event Action<MonoBehaviour> BoosterModeChanged;
    private MonoBehaviour activeBooster;
    private MonoBehaviour processingBooster;
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
        IsGameplayEnabled = false;
        CancelActiveBoosterMode();
        InputAction.GamePlay.Disable();
        InputAction.UI.Enable();
    }
    
    public void ChangeToGamePlay()
    {
        CancelActiveBoosterMode();
        IsGameplayEnabled = true;
        InputAction.GamePlay.Enable();
        InputAction.UI.Disable();
    }

    public bool ActivateBoosterMode(MonoBehaviour booster)
    {
        if (booster == null || !IsGameplayEnabled) return false;
        if (processingBooster != null && processingBooster != booster) return false;
        if (activeBooster == booster) return true;

        activeBooster = booster;
        BoosterModeChanged?.Invoke(activeBooster);
        return true;
    }

    public bool BeginBoosterAction(MonoBehaviour booster)
    {
        if (booster == null || activeBooster != booster || processingBooster != null) return false;
        processingBooster = booster;
        return true;
    }

    public void EndBoosterAction(MonoBehaviour booster)
    {
        if (processingBooster == booster) processingBooster = null;
    }

    public bool IsBoosterModeActive(MonoBehaviour booster) => activeBooster == booster;

    public void CancelBoosterMode(MonoBehaviour booster)
    {
        if (activeBooster != booster) return;
        activeBooster = null;
        BoosterModeChanged?.Invoke(null);
    }

    private void CancelActiveBoosterMode()
    {
        if (activeBooster == null) return;
        activeBooster = null;
        BoosterModeChanged?.Invoke(null);
    }

    private void OnDestroy()
    {
        if (_inputAction != null)
        {
            _inputAction.Disable();
            _inputAction.Dispose();
            _inputAction = null;
        }
    }
}
