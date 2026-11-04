using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelPopup : MonoBehaviour
{
    [SerializeField] private Image Panel;
    private Vector3 icon1Local, icon2Local;
    private bool hasSavedLocal;
    [SerializeField] private float rotateDuration = 0.35f;
    private Sequence rotSq;
    [SerializeField] private RectTransform TaskBG;
    [SerializeField] private RectTransform BG;
    [SerializeField] private RectTransform TaskRect;
    [SerializeField] private Image bgImage;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI goalText;
    [SerializeField] private RectTransform Task1Icon;
    [SerializeField] private RectTransform Task2Icon;
    [SerializeField] private RectTransform RowBreak;
    [SerializeField] private RectTransform RowObstacle;
    [SerializeField] private float BGZoomDuration;
    [SerializeField] private float TaskZoomDuration;
    [SerializeField] private float flyDownDuration;
    [SerializeField] private float flyUpDuration;
    [SerializeField] private float fadeDuration;
    [SerializeField] private float taskBGZoomTime = 0.8f;
    [SerializeField] private float taskBGZoomDuration = 0.5f;
    [SerializeField] private float iconFlyDuration = 0.3f;

    private Sequence sq, iconSq;
    private RectTransform slot1, slot2;
    private float originalAlpha;

    private void Start()
    {
        originalAlpha = Panel.color.a;
    }

    public void PopUp()
    {
        sq.Kill();
        iconSq.Kill();
        rotSq.Kill();
        CleanupSlots();
        sq = DOTween.Sequence();
        BG.localScale = Vector3.zero;
        TaskRect.localScale = Vector3.zero;
        Color color = Panel.color;
        color.a = originalAlpha;
        Panel.color = color;
        Panel.enabled = true;
        Panel.raycastTarget = true;

        sq.Append(BG.DOScale(1, BGZoomDuration).SetEase(Ease.Flash));
        sq.Append(TaskRect.DOScale(1, TaskZoomDuration).SetEase(Ease.OutBack));
    }

    public Sequence FlyUp()
    {
        sq.Kill();
        iconSq.Kill();
        rotSq.Kill();
        CleanupSlots();
        icon1Local = Task1Icon.localPosition;
        icon2Local = Task2Icon.localPosition;
        hasSavedLocal = true;
        Vector3 center = (icon1Local + icon2Local) * 0.5f;
        Vector3 off1 = icon1Local - center;
        Vector3 off2 = icon2Local - center;
        slot1 = MakeSlot(Task1Icon, RowBreak);
        slot2 = MakeSlot(Task2Icon, RowObstacle);
        LayoutRebuilder.ForceRebuildLayoutImmediate(TaskBG);

        sq = DOTween.Sequence();
        TaskBG.localScale = Vector3.zero;
        sq.Append(Panel.DOFade(0, 0.5f).SetEase(Ease.InOutCubic));
        sq.Append(BG.DOAnchorPosY(-285, flyDownDuration).SetEase(Ease.OutCubic));
        sq.Join(TaskRect.DOAnchorPosY(-285, flyDownDuration).SetEase(Ease.OutCubic));
        sq.Append(BG.DOAnchorPosY(891, flyUpDuration).SetEase(Ease.OutCubic));
        sq.Join(TaskRect.DOAnchorPosY(891, flyUpDuration).SetEase(Ease.OutCubic));
        sq.Join(bgImage.DOFade(0, fadeDuration).SetEase(Ease.OutCubic));
        sq.Join(goalText.DOFade(0, fadeDuration).SetEase(Ease.OutCubic));
        sq.Join(levelText.DOFade(0, fadeDuration).SetEase(Ease.OutCubic));
        sq.Insert(taskBGZoomTime, TaskBG.DOScale(1, taskBGZoomDuration).SetEase(Ease.OutSine));
        sq.Insert(flyDownDuration, DOVirtual.Float(0f, -90f, flyUpDuration, a =>
        {
            var q = Quaternion.Euler(0f, 0f, a);
            Task1Icon.localPosition = center + q * off1;
            Task2Icon.localPosition = center + q * off2;
        }).SetEase(Ease.OutCubic));

        float flyStart = Mathf.Max(taskBGZoomTime + taskBGZoomDuration, flyDownDuration + flyUpDuration);
        sq.InsertCallback(flyStart, FlyIconsToRows);
        sq.OnComplete(() =>
        {
            Panel.enabled = false;
            Panel.raycastTarget = false;
        });

        return sq;
    }

    private void FlyIconsToRows()
    {
        iconSq = DOTween.Sequence();
        Fly(Task1Icon, slot1, 0f);
        Fly(Task2Icon, slot2, 0.05f);
    }

    private void Fly(RectTransform icon, RectTransform slot, float delay)
    {
        iconSq.Insert(delay, icon.DOMove(slot.position, iconFlyDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                slot.GetComponent<CanvasGroup>().alpha = 1f;
                icon.gameObject.SetActive(false);
            }));
    }

    private RectTransform MakeSlot(RectTransform src, RectTransform row)
    {
        var s = Instantiate(src, row, false);
        s.gameObject.SetActive(true);
        if (!s.TryGetComponent(out CanvasGroup cg)) cg = s.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        return s;
    }

    private void CleanupSlots()
    {
        if (hasSavedLocal)
        {
            Task1Icon.localPosition = icon1Local;
            Task2Icon.localPosition = icon2Local;
        }

        DestroySlot(ref slot1);
        DestroySlot(ref slot2);
        Task1Icon.gameObject.SetActive(true);
        Task2Icon.gameObject.SetActive(true);
    }

    private static void DestroySlot(ref RectTransform s)
    {
        if (!s) return;
        s.SetParent(null);
        Destroy(s.gameObject);
        s = null;
    }
}