using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelPopup : MonoBehaviour
{
    [SerializeField] private Image Panel;
    private Vector3 icon1Local, icon2Local;
    private Vector3 icon1Scale, icon2Scale;
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
        icon1Scale = Task1Icon.localScale;
        icon2Scale = Task2Icon.localScale;
        hasSavedLocal = true;
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

        float flyStart = Mathf.Max(taskBGZoomTime + taskBGZoomDuration, flyDownDuration + flyUpDuration);
        sq.InsertCallback(flyStart, RotateIconsIntoGoalHud);
        sq.OnComplete(() =>
        {
            Panel.enabled = false;
            Panel.raycastTarget = false;
        });

        return sq;
    }

    private void RotateIconsIntoGoalHud()
    {
        RectTransform goalHud = RowBreak.parent as RectTransform;
        Vector3 center = goalHud.TransformPoint(goalHud.rect.center);
        Vector3 pairCenter = (Task1Icon.position + Task2Icon.position) * 0.5f;
        Vector3 off1 = (Task1Icon.position - pairCenter) * 0.55f;
        Vector3 off2 = (Task2Icon.position - pairCenter) * 0.55f;

        rotSq = DOTween.Sequence();
        rotSq.Append(Task1Icon.DOMove(center + off1, rotateDuration).SetEase(Ease.OutCubic));
        rotSq.Join(Task2Icon.DOMove(center + off2, rotateDuration).SetEase(Ease.OutCubic));
        rotSq.Join(Task1Icon.DORotate(Task1Icon.eulerAngles + Vector3.forward * 360f, rotateDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutCubic));
        rotSq.Join(Task2Icon.DORotate(Task2Icon.eulerAngles + Vector3.forward * 360f, rotateDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutCubic));
        rotSq.OnComplete(FlyIconsToRows);
    }

    private void FlyIconsToRows()
    {
        iconSq = DOTween.Sequence();
        Fly(Task1Icon, slot1, 0f);
        Fly(Task2Icon, slot2, 0.05f);
        iconSq.OnComplete(() => hasSavedLocal = false);
    }

    private void Fly(RectTransform icon, RectTransform slot, float delay)
    {
        iconSq.Insert(delay, icon.DOMove(slot.position, iconFlyDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                Transform row = slot.parent;
                int siblingIndex = slot.GetSiblingIndex();
                icon.SetParent(row, false);
                icon.anchorMin = slot.anchorMin;
                icon.anchorMax = slot.anchorMax;
                icon.pivot = slot.pivot;
                icon.sizeDelta = slot.sizeDelta;
                icon.anchoredPosition3D = slot.anchoredPosition3D;
                icon.localRotation = slot.localRotation;
                icon.localScale = slot.localScale;
                icon.SetSiblingIndex(siblingIndex);
                icon.gameObject.SetActive(true);
                slot.gameObject.SetActive(false);
                Destroy(slot.gameObject);
                if (slot == slot1) slot1 = null;
                if (slot == slot2) slot2 = null;
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)row);
            }));
    }

    private RectTransform MakeSlot(RectTransform src, RectTransform row)
    {
        RectTransform slot = Instantiate(src, row, false);
        slot.gameObject.SetActive(true);
        if (!slot.TryGetComponent(out CanvasGroup canvasGroup))
            canvasGroup = slot.gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        return slot;
    }

    private void CleanupSlots()
    {
        if (hasSavedLocal)
        {
            Task1Icon.localPosition = icon1Local;
            Task2Icon.localPosition = icon2Local;
            Task1Icon.localScale = icon1Scale;
            Task2Icon.localScale = icon2Scale;
        }

        DestroySlot(ref slot1);
        DestroySlot(ref slot2);
        Task1Icon.gameObject.SetActive(true);
        Task2Icon.gameObject.SetActive(true);
    }

    private static void DestroySlot(ref RectTransform slot)
    {
        if (!slot) return;
        slot.SetParent(null);
        Destroy(slot.gameObject);
        slot = null;
    }
}
