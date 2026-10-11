using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
public class TaskLockRender : MonoBehaviour
{
    [SerializeField] private Color lockColor;
    [SerializeField] private MeshRenderer render;
    private Material materialInstance;

    public void Lock()
    {
        ResetRender();
        if (materialInstance == null && render != null)
        {
            materialInstance = render.material;
        }
        if (materialInstance != null)
        {
            materialInstance.color = lockColor;
        }
    }

    public void ResetRender()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
    }

    public async UniTask PlayUnlockEffect()
    {
        transform.DOKill();
        await transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).SetLink(gameObject).ToUniTask();
    }

    private void OnDestroy()
    {
        transform.DOKill();
        if (materialInstance != null)
        {
            Destroy(materialInstance);
            materialInstance = null;
        }
    }
}