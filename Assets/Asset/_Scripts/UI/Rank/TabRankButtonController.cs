using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TabRankButtonController : MonoBehaviour
{
    [SerializeField] private Button currentButton;
    [SerializeField] private Button targetButton;
    [SerializeField] private GameObject selected;
    [SerializeField] private GameObject unselected;
    [SerializeField] private GameObject[] tabContent = new GameObject[0];
    [SerializeField] private RectTransform showIcon;
    [SerializeField] private RectTransform hideIcon;
    [SerializeField] private Vector3 punchVel;
    [SerializeField] private float duration;
    [SerializeField] private int vibrato;
    [SerializeField] private float elasticity;
    private Tween iconTween;
    private Sequence rowEntrance;
    public void Show()
    {
        currentButton.interactable = false;
        targetButton.interactable = true;
        selected.gameObject.SetActive(true);
        unselected.gameObject.SetActive(false);
        currentButton.transform.SetAsLastSibling();
        SetContentVisible(true);
        PlayIconTween(showIcon);
    }

    public void Hide()
    {
        currentButton.interactable = true;
        targetButton.interactable = false;
        selected.gameObject.SetActive(false);
        unselected.gameObject.SetActive(true);
        SetContentVisible(false);
        PlayIconTween(hideIcon);
    }

    private void SetContentVisible(bool visible)
    {
        rowEntrance.Kill(true);
        foreach (var item in tabContent) item.SetActive(visible);
        if (!visible) return;

        foreach (var item in tabContent)
        {
            if (item.TryGetComponent<ScrollRect>(out var scroll))
                AnimateRows(scroll);
        }
    }

    private void AnimateRows(ScrollRect scroll)
    {
        Canvas.ForceUpdateCanvases();
        var viewport = scroll.viewport;
        var startTime = 0f;
        rowEntrance = DOTween.Sequence().SetUpdate(true);
        foreach (RectTransform row in scroll.content)
        {
            if (!row.gameObject.activeInHierarchy || row.childCount == 0) continue;
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, row);
            if (bounds.max.y <= viewport.rect.yMin || bounds.min.y >= viewport.rect.yMax) continue;

            var endX = row.anchoredPosition.x;
            row.anchoredPosition -= Vector2.right * scroll.content.rect.width;
            var slide = row.DOAnchorPosX(endX, 0.22f)
                .SetEase(Ease.OutCubic);
            rowEntrance.Insert(startTime, slide);
            startTime += 0.025f;
        }
    }

    private void OnDisable()
    {
        rowEntrance.Kill(true);
    }

    private void PlayIconTween(RectTransform icon)
    {
        iconTween.Kill(true);
        iconTween = icon.DOPunchScale(punchVel, duration, vibrato, elasticity).SetEase(Ease.OutBack);
    }

}
