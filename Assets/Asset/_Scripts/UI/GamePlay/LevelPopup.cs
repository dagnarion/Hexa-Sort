using DG.Tweening;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelPopup : MonoBehaviour
{
    [SerializeField] private RectTransform BG;
    [SerializeField] private RectTransform TaskRect;
    [SerializeField] private Image bgImage;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI goalText;
    [SerializeField] private float BGZoomDuration;
    [SerializeField] private float TaskZoomDuration;
    [SerializeField] private float flyDownDuration;
    [SerializeField] private float flyUpDuration;
    [SerializeField] private float fadeDuration;
    private Sequence sq;

    [Button]
    public void PopUp()
    {
        sq.Kill();
        sq = DOTween.Sequence();
        BG.localScale = Vector3.zero;
        TaskRect.localScale = Vector3.zero;
        sq.Append(BG.DOScale(1, BGZoomDuration).SetEase(Ease.Flash));
        sq.Append(TaskRect.DOScale(1, TaskZoomDuration).SetEase(Ease.OutBack));
    }

    [Button]
    public void FlyUp()
    {
        sq.Kill();
        sq = DOTween.Sequence();
        sq.Append(BG.DOAnchorPosY(-285, flyDownDuration).SetEase(Ease.OutCubic));
        sq.Join(TaskRect.DOAnchorPosY(-285, flyDownDuration).SetEase(Ease.OutCubic));
        sq.Append(BG.DOAnchorPosY(891, flyUpDuration).SetEase(Ease.OutCubic));
        sq.Join(TaskRect.DOAnchorPosY(891, flyUpDuration).SetEase(Ease.OutCubic));
        sq.Join(bgImage.DOFade(0,fadeDuration).SetEase(Ease.OutCubic));
        sq.Join(goalText.DOFade(0, fadeDuration).SetEase(Ease.OutCubic));
        sq.Join(levelText.DOFade(0, fadeDuration).SetEase(Ease.OutCubic));
        
    }
}