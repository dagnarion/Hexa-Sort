using UnityEngine;
using DG.Tweening;

public class PanelPopUp : MonoBehaviour
{
    [SerializeField] private RectTransform popup;
    private Sequence sequence;

    public void Show()
    {
        popup.gameObject.SetActive(true);
        popup.localScale = Vector3.one * 0.8f;
        sequence.Kill();
        sequence = DOTween.Sequence();
        sequence.Join(popup.DOScale(1.05f, 0.25f).SetEase(Ease.OutBack)).Append(popup.DOScale(1f, 0.1f));
    }

    public void Hide()
    {
        sequence.Kill();
        sequence = DOTween.Sequence();
        sequence.Join(popup.DOScale(0.9f, 0.15f).SetEase(Ease.InBack)).OnComplete(() => popup.gameObject.SetActive(false));
    }
}