using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class BreakLockRender : MonoBehaviour
{
    public async UniTask OnHit(int remain)
    {
        await transform.DOPunchScale(Vector3.one * 0.15f, 0.2f).ToUniTask();
    }

    public async UniTask PlayUnlockEffect()
    {
        await transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).ToUniTask();
    }
}