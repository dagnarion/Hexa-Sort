using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
public class TaskLockRender : MonoBehaviour
{
    [SerializeField] private Color lockColor;
    [SerializeField] private MeshRenderer render;

    public void Lock()
    {
        render.material.color = lockColor;
    }

    public async UniTask PlayUnlockEffect()
    {
        await transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).ToUniTask();
    }
    // sửa sau
}