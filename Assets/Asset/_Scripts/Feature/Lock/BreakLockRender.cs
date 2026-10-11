using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class BreakLockRender : MonoBehaviour
{
    public void Init()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
    }

    public async UniTask OnHit(int remain)
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
        await transform.DOPunchScale(Vector3.one * 0.15f, 0.2f).SetLink(gameObject).ToUniTask();
    }

    public async UniTask PlayUnlockEffect()
    {
        transform.DOKill();
        await transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).SetLink(gameObject).ToUniTask();
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}