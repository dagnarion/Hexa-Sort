using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelPopup : MonoBehaviour
{
    [Header("Task layout")]
    [SerializeField, Min(1)] private int iconsPerRow = 1;
    [SerializeField] private float spinAngle = -360f;
    [SerializeField] private float breakCollapsedWidth = 88f;
    [SerializeField] private float sliderDuration = 0.35f;
    [SerializeField] private float progressFadeDuration = 0.15f;

    [Header("Timing")]
    [SerializeField] private float bgZoomDuration = 0.25f;
    [SerializeField] private float taskZoomDuration = 0.3f;
    [SerializeField] private float flyDownDuration = 0.3f;
    [SerializeField] private float flyUpDuration = 0.6f;
    [SerializeField] private float fadeDuration = 0.8f;
    [SerializeField] private float taskBgZoomTime = 0.8f;
    [SerializeField] private float taskBgZoomDuration = 0.5f;
    [SerializeField] private float rotateDuration = 0.35f;
    [SerializeField] private float iconFlyDuration = 0.3f;

    private Image panel;
    private RectTransform taskBg;
    private RectTransform bg;
    private RectTransform taskRect;
    private RectTransform goalIcon;
    private RectTransform rowBreak;
    private RectTransform rowObstacle;
    private Image bgImage;
    private TextMeshProUGUI levelText;
    private TextMeshProUGUI goalText;
    private readonly List<TaskIconEntry> taskIcons = new List<TaskIconEntry>();
    private readonly List<RectTransform> taskRows = new List<RectTransform>();
    private readonly List<RectTransform> extraRows = new List<RectTransform>();
    private Sequence popupSequence;
    private Sequence iconSequence;
    private Sequence rotationSequence;
    private RectTransformState bgState;
    private RectTransformState taskRectState;
    private RectTransformState taskBgState;
    private Color panelColor;
    private Color bgColor;
    private Color goalColor;
    private Color levelColor;
    private bool initialPanelEnabled;
    private bool initialRaycastTarget;
    private bool referencesReady;

    private sealed class TaskIconEntry
    {
        public RectTransform Root;
        public RectTransform RotationTarget;
        public RectTransform Slot;
        public RectTransform Row;
        public TaskGoalKind Kind;
        public Vector3 DestinationPosition;
        public RectTransformState RootState;
        public RectTransformState RotationState;
        public LayoutElement LayoutElement;
        public float FullWidth;
        public TextMeshProUGUI ProgressText;
        public GameObject SliderBackground;
        public GameObject SliderFillArea;
        public bool BackgroundWasActive;
        public bool FillAreaWasActive;
        public bool ProgressTextWasActive;
        public float ProgressTextAlpha;
    }

    private struct RectTransformState
    {
        private readonly Transform parent;
        private readonly int siblingIndex;
        private readonly Vector2 anchorMin;
        private readonly Vector2 anchorMax;
        private readonly Vector2 pivot;
        private readonly Vector2 sizeDelta;
        private readonly Vector3 anchoredPosition3D;
        private readonly Vector3 localPosition;
        private readonly Vector3 localScale;
        private readonly Quaternion localRotation;

        public RectTransformState(RectTransform rect)
        {
            parent = rect.parent;
            siblingIndex = rect.GetSiblingIndex();
            anchorMin = rect.anchorMin;
            anchorMax = rect.anchorMax;
            pivot = rect.pivot;
            sizeDelta = rect.sizeDelta;
            anchoredPosition3D = rect.anchoredPosition3D;
            localPosition = rect.localPosition;
            localScale = rect.localScale;
            localRotation = rect.localRotation;
        }

        public void Restore(RectTransform rect)
        {
            if (rect == null || parent == null) return;
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition3D = anchoredPosition3D;
            rect.localPosition = localPosition;
            rect.localRotation = localRotation;
            rect.localScale = localScale;
            rect.SetSiblingIndex(Mathf.Clamp(siblingIndex, 0, Mathf.Max(0, parent.childCount - 1)));
        }
    }

    private void Awake()
    {
        referencesReady = ResolveReferences();
        if (!referencesReady)
        {
            Debug.LogError($"[{nameof(LevelPopup)}] Required popup objects are missing from the LevelLoadPanel hierarchy.", this);
            return;
        }

        bgState = new RectTransformState(bg);
        taskRectState = new RectTransformState(taskRect);
        taskBgState = new RectTransformState(taskBg);
        panelColor = panel.color;
        bgColor = bgImage.color;
        goalColor = goalText.color;
        levelColor = levelText.color;
        initialPanelEnabled = panel.enabled;
        initialRaycastTarget = panel.raycastTarget;
    }

    private void OnDisable()
    {
        KillAllTweens();
    }

    private bool ResolveReferences()
    {
        panel = GetComponent<Image>();
        Transform hud = GetDirectChild(transform, "Goal Hud");
        Transform mid = GetDirectChild(transform, "Mid");
        Transform bgTransform = GetDirectChild(mid, "BG");
        taskRect = GetDirectChild(mid, "Task") as RectTransform;
        Transform goalIconTransform = GetDirectChild(taskRect, "GoalIcon");
        Transform levelTextTransform = GetDirectChild(taskRect, "LevelText");
        Transform goalTextTransform = GetDirectChild(taskRect, "GoalText");

        taskBg = hud as RectTransform;
        bg = bgTransform as RectTransform;
        goalIcon = goalIconTransform as RectTransform;
        rowBreak = GetDirectChild(taskBg, "Row Break") as RectTransform;
        rowObstacle = GetDirectChild(taskBg, "Row Obstacle") as RectTransform;
        bgImage = bgTransform != null ? bgTransform.GetComponent<Image>() : null;
        levelText = levelTextTransform != null ? levelTextTransform.GetComponent<TextMeshProUGUI>() : null;
        goalText = goalTextTransform != null ? goalTextTransform.GetComponent<TextMeshProUGUI>() : null;

        return panel != null && taskBg != null && bg != null && taskRect != null && goalIcon != null &&
               rowBreak != null && rowObstacle != null && bgImage != null && levelText != null && goalText != null;
    }

    private static Transform GetDirectChild(Transform parent, string childName)
    {
        if (parent == null) return null;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == childName) return child;
        }

        return null;
    }

    private static Transform GetDescendant(Transform parent, string objectName)
    {
        if (parent == null) return null;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == objectName) return child;
            Transform result = GetDescendant(child, objectName);
            if (result != null) return result;
        }

        return null;
    }

    private void KillAllTweens()
    {
        popupSequence?.Kill();
        iconSequence?.Kill();
        rotationSequence?.Kill();
    }

    public void ResetValues()
    {
        if (!referencesReady) return;
        KillAllTweens();
        RestoreTaskIcons();
        CleanupExtraRows();
        bgState.Restore(bg);
        taskRectState.Restore(taskRect);
        taskBgState.Restore(taskBg);
        panel.color = panelColor;
        panel.enabled = initialPanelEnabled;
        panel.raycastTarget = initialRaycastTarget;
        bgImage.color = bgColor;
        goalText.color = goalColor;
        levelText.color = levelColor;
        LayoutRebuilder.ForceRebuildLayoutImmediate(taskBg);
    }

    public void PopUp()
    {
        if (!referencesReady) return;
        KillAllTweens();
        RestoreTaskIcons();
        CleanupExtraRows();
        CaptureTaskIcons();
        ApplyCollapsedSliders();
        popupSequence = DOTween.Sequence();
        bg.localScale = Vector3.zero;
        taskRect.localScale = Vector3.zero;
        Color color = panel.color;
        color.a = panelColor.a;
        panel.color = color;
        panel.enabled = true;
        panel.raycastTarget = true;
        popupSequence.Append(bg.DOScale(1f, bgZoomDuration).SetEase(Ease.Flash));
        popupSequence.Append(taskRect.DOScale(1f, taskZoomDuration).SetEase(Ease.OutBack));
    }

    public Sequence FlyUp()
    {
        if (!referencesReady) return DOTween.Sequence();
        KillAllTweens();
        RestoreTaskIcons();
        CleanupExtraRows();
        CaptureTaskIcons();
        ApplyCollapsedSliders();
        CreateRowsAndSlots();
        Vector3 taskBgScale = taskBg.localScale;
        taskBg.localScale = Vector3.one;
        LayoutRebuilder.ForceRebuildLayoutImmediate(taskBg);
        Canvas.ForceUpdateCanvases();
        for (int i = 0; i < taskIcons.Count; i++)
        {
            RectTransform row = taskIcons[i].Row;
            LayoutRebuilder.ForceRebuildLayoutImmediate(row);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(taskBg);
        Canvas.ForceUpdateCanvases();
        for (int i = 0; i < taskIcons.Count; i++)
            taskIcons[i].DestinationPosition = taskIcons[i].Slot.position;
        taskBg.localScale = taskBgScale;

        popupSequence = DOTween.Sequence();
        taskBg.localScale = Vector3.zero;
        popupSequence.Append(panel.DOFade(0f, 0.5f).SetEase(Ease.InOutCubic));
        popupSequence.Append(bg.DOAnchorPosY(-285f, flyDownDuration).SetEase(Ease.OutCubic));
        popupSequence.Join(taskRect.DOAnchorPosY(-285f, flyDownDuration).SetEase(Ease.OutCubic));
        popupSequence.Append(bg.DOAnchorPosY(891f, flyUpDuration).SetEase(Ease.OutCubic));
        popupSequence.Join(taskRect.DOAnchorPosY(891f, flyUpDuration).SetEase(Ease.OutCubic));
        popupSequence.Join(bgImage.DOFade(0f, fadeDuration).SetEase(Ease.OutCubic));
        popupSequence.Join(goalText.DOFade(0f, fadeDuration).SetEase(Ease.OutCubic));
        popupSequence.Join(levelText.DOFade(0f, fadeDuration).SetEase(Ease.OutCubic));
        popupSequence.Insert(taskBgZoomTime, taskBg.DOScale(1f, taskBgZoomDuration).SetEase(Ease.OutSine));

        float flyStart = Mathf.Max(taskBgZoomTime + taskBgZoomDuration, flyDownDuration + flyUpDuration);
        popupSequence.Insert(flyStart, BuildTaskIconSequence());
        popupSequence.OnComplete(() =>
        {
            panel.enabled = false;
            panel.raycastTarget = false;
        });
        return popupSequence;
    }

    private void CaptureTaskIcons()
    {
        taskIcons.Clear();
        for (int i = 0; i < goalIcon.childCount; i++)
        {
            RectTransform root = goalIcon.GetChild(i) as RectTransform;
            if (root == null) continue;
            Transform visual = GetDescendant(root, "Task Icon") ?? GetDescendant(root, "Icon");
            RectTransform visualRect = visual as RectTransform;
            if (visualRect == null) visualRect = root;

            Slider slider = root.GetComponentInChildren<Slider>(true);
            TextMeshProUGUI progressText = root.GetComponentInChildren<TextMeshProUGUI>(true);
            LayoutElement layout = root.GetComponent<LayoutElement>();
            TaskGoalIcon taskGoalIcon = root.GetComponent<TaskGoalIcon>();
            Transform sliderBackground = slider != null ? GetDirectChild(slider.transform, "Background") : null;
            Transform sliderFillArea = slider != null ? GetDirectChild(slider.transform, "Fill Area") : null;
            RectTransform rotationTarget = slider != null ? visualRect : root;
            TaskGoalKind kind = taskGoalIcon != null
                ? taskGoalIcon.Kind
                : slider != null ? TaskGoalKind.CollectHexagons : TaskGoalKind.BreakBlocks;

            taskIcons.Add(new TaskIconEntry
            {
                Root = root,
                RotationTarget = rotationTarget,
                Kind = kind,
                Row = kind == TaskGoalKind.CollectHexagons ? rowBreak : rowObstacle,
                RootState = new RectTransformState(root),
                RotationState = new RectTransformState(rotationTarget),
                LayoutElement = layout,
                FullWidth = layout != null ? layout.preferredWidth : 0f,
                ProgressText = progressText,
                SliderBackground = sliderBackground != null ? sliderBackground.gameObject : null,
                SliderFillArea = sliderFillArea != null ? sliderFillArea.gameObject : null,
                BackgroundWasActive = sliderBackground != null && sliderBackground.gameObject.activeSelf,
                FillAreaWasActive = sliderFillArea != null && sliderFillArea.gameObject.activeSelf,
                ProgressTextWasActive = progressText != null && progressText.gameObject.activeSelf,
                ProgressTextAlpha = progressText != null ? progressText.alpha : 1f
            });
        }

        for (int i = 0; i < taskIcons.Count; i++)
            taskIcons[i].Root.anchoredPosition = new Vector2((i - (taskIcons.Count - 1) * 0.5f) * 70f, 0f);
    }

    private void ApplyCollapsedSliders()
    {
        for (int i = 0; i < taskIcons.Count; i++)
        {
            TaskIconEntry entry = taskIcons[i];
            if (entry.LayoutElement == null || entry.ProgressText == null) continue;
            entry.LayoutElement.preferredWidth = breakCollapsedWidth;
            if (entry.SliderBackground != null) entry.SliderBackground.SetActive(false);
            if (entry.SliderFillArea != null) entry.SliderFillArea.SetActive(false);
            entry.ProgressText.gameObject.SetActive(true);
            entry.ProgressText.alpha = 0f;
        }
    }

    private void CreateRowsAndSlots()
    {
        taskRows.Clear();
        extraRows.Clear();
        taskRows.Add(rowBreak);
        taskRows.Add(rowObstacle);
        List<RectTransform> collectRows = new List<RectTransform> { rowBreak };
        List<RectTransform> breakRows = new List<RectTransform> { rowObstacle };
        int collectCount = 0;
        int breakCount = 0;

        for (int i = 0; i < taskIcons.Count; i++)
        {
            TaskIconEntry entry = taskIcons[i];
            bool isCollectTask = entry.Kind == TaskGoalKind.CollectHexagons;
            List<RectTransform> matchingRows = isCollectTask ? collectRows : breakRows;
            RectTransform template = isCollectTask ? rowBreak : rowObstacle;
            int taskIndex = isCollectTask ? collectCount++ : breakCount++;
            int rowIndex = taskIndex / Mathf.Max(1, iconsPerRow);
            while (matchingRows.Count <= rowIndex)
            {
                RectTransform extraRow = Instantiate(template, taskBg, false);
                extraRow.name = isCollectTask
                    ? $"TaskRow_Break_{matchingRows.Count + 1}"
                    : $"TaskRow_Obstacle_{matchingRows.Count + 1}";
                matchingRows.Add(extraRow);
                taskRows.Add(extraRow);
                extraRows.Add(extraRow);
            }

            entry.Row = matchingRows[rowIndex];
            entry.Slot = Instantiate(entry.Root, entry.Row, false);
            entry.Slot.gameObject.SetActive(true);
            CanvasGroup canvasGroup = entry.Slot.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = entry.Slot.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
        }
    }

    private Sequence BuildTaskIconSequence()
    {
        iconSequence = DOTween.Sequence();
        rotationSequence = DOTween.Sequence();
        if (taskIcons.Count == 0) return iconSequence;

        for (int i = 0; i < taskIcons.Count; i++)
        {
            TaskIconEntry entry = taskIcons[i];
            float delay = i * 0.05f;
            float travelDuration = rotateDuration + iconFlyDuration;
            rotationSequence.Insert(delay, entry.Root.DOMove(entry.DestinationPosition, travelDuration).SetEase(Ease.OutCubic));
            rotationSequence.Insert(delay, entry.RotationTarget.DOLocalRotate(
                    entry.RotationTarget.localEulerAngles + Vector3.forward * spinAngle,
                    travelDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutCubic));
        }

        iconSequence.Append(rotationSequence);
        iconSequence.AppendCallback(AdoptAllSlots);
        Sequence sliderSequence = DOTween.Sequence();
        bool hasSlider = false;
        for (int i = 0; i < taskIcons.Count; i++)
        {
            TaskIconEntry entry = taskIcons[i];
            if (entry.LayoutElement == null || entry.ProgressText == null) continue;
            sliderSequence.Insert(0f, ShowSlider(entry));
            hasSlider = true;
        }

        if (hasSlider) iconSequence.Append(sliderSequence);
        return iconSequence;
    }

    private Sequence ShowSlider(TaskIconEntry entry)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() =>
        {
            if (entry.SliderBackground != null) entry.SliderBackground.SetActive(true);
            if (entry.SliderFillArea != null) entry.SliderFillArea.SetActive(true);
        });
        sequence.Append(DOTween.To(() => entry.LayoutElement.preferredWidth,
                value => entry.LayoutElement.preferredWidth = value,
                entry.FullWidth, sliderDuration)
            .SetEase(Ease.OutCubic));
        sequence.Append(entry.ProgressText.DOFade(1f, progressFadeDuration));
        return sequence;
    }

    private void AdoptSlot(TaskIconEntry entry)
    {
        if (entry.Root == null || entry.Slot == null) return;
        Transform row = entry.Slot.parent;
        int siblingIndex = entry.Slot.GetSiblingIndex();
        entry.Root.SetParent(row, false);
        entry.Root.anchorMin = entry.Slot.anchorMin;
        entry.Root.anchorMax = entry.Slot.anchorMax;
        entry.Root.pivot = entry.Slot.pivot;
        entry.Root.sizeDelta = entry.Slot.sizeDelta;
        entry.Root.anchoredPosition3D = entry.Slot.anchoredPosition3D;
        entry.Root.localRotation = entry.Slot.localRotation;
        entry.Root.localScale = entry.Slot.localScale;
        entry.Root.SetSiblingIndex(siblingIndex);
        entry.Root.gameObject.SetActive(true);
        entry.Slot.gameObject.SetActive(false);
        Destroy(entry.Slot.gameObject);
        entry.Slot = null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(row as RectTransform);
    }

    private void AdoptAllSlots()
    {
        for (int i = 0; i < taskIcons.Count; i++)
            AdoptSlot(taskIcons[i]);
    }

    private void RestoreTaskIcons()
    {
        for (int i = 0; i < taskIcons.Count; i++)
        {
            TaskIconEntry entry = taskIcons[i];
            if (entry.Slot != null)
            {
                entry.Slot.gameObject.SetActive(false);
                Destroy(entry.Slot.gameObject);
                entry.Slot = null;
            }

            entry.RootState.Restore(entry.Root);
            if (entry.RotationTarget != entry.Root)
                entry.RotationState.Restore(entry.RotationTarget);
            if (entry.LayoutElement != null)
                entry.LayoutElement.preferredWidth = entry.FullWidth;
            if (entry.SliderBackground != null)
                entry.SliderBackground.SetActive(entry.BackgroundWasActive);
            if (entry.SliderFillArea != null)
                entry.SliderFillArea.SetActive(entry.FillAreaWasActive);
            if (entry.ProgressText != null)
            {
                entry.ProgressText.gameObject.SetActive(entry.ProgressTextWasActive);
                entry.ProgressText.alpha = entry.ProgressTextAlpha;
            }
        }

        taskIcons.Clear();
    }

    private void CleanupExtraRows()
    {
        for (int i = 0; i < extraRows.Count; i++)
        {
            if (extraRows[i] == null) continue;
            extraRows[i].gameObject.SetActive(false);
            Destroy(extraRows[i].gameObject);
        }

        extraRows.Clear();
        taskRows.Clear();
    }
}
