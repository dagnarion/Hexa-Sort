using DG.Tweening;
using UnityEngine;

public class BoosterBoardFeel : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private float tiltAngle = 15f;
    [SerializeField] private float tiltDuration = 0.3f;
    [SerializeField] private float returnDuration = 0.8f;

    private Quaternion originalLocalRotation;
    private Quaternion tiltedLocalRotation;
    private bool isTilted;

    private void Awake()
    {
        originalLocalRotation = transform.localRotation;
        tiltedLocalRotation = originalLocalRotation * Quaternion.Euler(tiltAngle, 0f, 0f);
    }

    private void OnEnable()
    {
        if (inputManager != null) inputManager.BoosterModeChanged += OnBoosterModeChanged;
    }

    private void OnDisable()
    {
        if (inputManager != null) inputManager.BoosterModeChanged -= OnBoosterModeChanged;
        transform.DOKill();
        transform.localRotation = originalLocalRotation;
        isTilted = false;
    }

    private void OnBoosterModeChanged(MonoBehaviour activeBooster)
    {
        bool shouldTilt = activeBooster is BreakBooster || activeBooster is SwapBooster;
        if (isTilted == shouldTilt) return;

        isTilted = shouldTilt;
        transform.DOKill();
        transform.DOLocalRotateQuaternion(
                shouldTilt ? tiltedLocalRotation : originalLocalRotation,
                shouldTilt ? tiltDuration : returnDuration)
            .SetEase(Ease.InOutSine);
    }
}
