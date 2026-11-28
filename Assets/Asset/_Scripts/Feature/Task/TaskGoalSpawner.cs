using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskGoalSpawner : MonoBehaviour
{
    [Header("Level data")]
    [SerializeField] private EventChannel<LevelData> levelLoadEventChannel;
    [SerializeField] private TaskManager taskManager;

    [Header("Goal icon container")]
    [SerializeField] private RectTransform goalIcon;

    [Header("Task prefabs")]
    [SerializeField] private GameObject collectHexagonTaskPrefab;
    [SerializeField] private GameObject breakBlockTaskPrefab;
    [SerializeField] private TaskPickupPool pickupPool;
    [SerializeField] private Sprite hexPickupSprite;

    private void Awake()
    {
        if (pickupPool != null) pickupPool.InitPool(GetComponentInParent<Canvas>().rootCanvas.transform);
    }

    private void OnDestroy() => pickupPool?.Clear();

    private readonly List<TaskView> taskViews = new List<TaskView>();
    private readonly List<GameObject> spawnedTasks = new List<GameObject>();

    private sealed class TaskView
    {
        public Image Icon;
        public bool IsCollectTask;
        public TextMeshProUGUI ProgressText;
        public Slider ProgressBar;
        private Tween arrivalTween;

        public void Bounce()
        {
            if (Icon == null) return;
            arrivalTween.Kill(true);
            var rect = Icon.rectTransform;
            arrivalTween = rect.DOPunchScale(rect.localScale * 0.11f, 0.19f, 1, 0.3f)
                .SetLink(Icon.gameObject);
        }
    }

    private void OnEnable()
    {
        if (levelLoadEventChannel != null)
            levelLoadEventChannel.OnEventRaise += SpawnForLevel;
        if (taskManager != null)
            taskManager.ProgressChanged += UpdateTaskViews;
    }

    private void OnDisable()
    {
        if (levelLoadEventChannel != null)
            levelLoadEventChannel.OnEventRaise -= SpawnForLevel;
        if (taskManager != null)
            taskManager.ProgressChanged -= UpdateTaskViews;
    }

    private void SpawnForLevel(LevelData levelData)
    {
        ClearSpawnedTasks();

        RectTransform parent = goalIcon != null ? goalIcon : (RectTransform)transform;
        if (levelData.CollectHexagonTarget > 0)
            Spawn(collectHexagonTaskPrefab, parent, true);

        if (levelData.BreakBlockTarget > 0)
            Spawn(breakBlockTaskPrefab, parent, false);

        if (taskManager != null && taskManager.TryGetProgress(out int collected, out int collectTarget, out int broken, out int breakTarget))
            UpdateTaskViews(collected, collectTarget, broken, breakTarget);
    }

    private void Spawn(GameObject prefab, RectTransform parent, bool isCollectTask)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"[{nameof(TaskGoalSpawner)}] A required task prefab is not assigned.", this);
            return;
        }

        GameObject instance = Instantiate(prefab, parent, false);
        instance.AddComponent<TaskGoalIcon>().Initialize(isCollectTask
            ? TaskGoalKind.CollectHexagons
            : TaskGoalKind.BreakBlocks);
        spawnedTasks.Add(instance);
        taskViews.Add(new TaskView
        {
            Icon = instance.transform.Find(isCollectTask ? "Task Icon" : "Icon")?.GetComponent<Image>(),
            IsCollectTask = isCollectTask,
            ProgressText = instance.GetComponentInChildren<TextMeshProUGUI>(true),
            ProgressBar = instance.GetComponentInChildren<Slider>(true)
        });
    }

    private void ClearSpawnedTasks()
    {
        RectTransform parent = goalIcon != null ? goalIcon : (RectTransform)transform;
        for (int i = 0; i < spawnedTasks.Count; i++)
        {
            if (spawnedTasks[i] == null) continue;
            spawnedTasks[i].SetActive(false);
            Destroy(spawnedTasks[i]);
        }

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (!child.gameObject.activeSelf) continue;
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        spawnedTasks.Clear();
        taskViews.Clear();
    }

    public static async UniTask FlyPickups(TaskGoalKind kind, Vector3 source, Color color, int amount,
        System.Action<int> onArrival = null)
    {
        var hud = FindFirstObjectByType<TaskGoalSpawner>();
        bool collect = kind == TaskGoalKind.CollectHexagons;
        var view = hud == null ? null : hud.taskViews.Find(item => item.IsCollectTask == collect);
        var icon = view?.Icon;
        var camera = Camera.main;
        if (icon == null || !icon.isActiveAndEnabled || hud.pickupPool == null || camera == null)
        {
            onArrival?.Invoke(amount);
            return;
        }

        var canvas = icon.canvas.rootCanvas;
        var root = (RectTransform)canvas.transform;
        var uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(root, camera.WorldToScreenPoint(source),
            uiCamera, out Vector2 origin);
        Vector3 end = root.InverseTransformPoint(icon.rectTransform.TransformPoint(icon.rectTransform.rect.center));
        int count = Mathf.Min(amount, 6);
        var sequence = DOTween.Sequence().SetLink(canvas.gameObject);
        var tokens = new Image[count];
        color.a = 1f;
        Vector2 direction = ((Vector2)end - origin).normalized;
        Vector2 sideways = new Vector2(-direction.y, direction.x);
        float bend = Mathf.Clamp(Vector2.Distance(origin, end) * 0.22f, 55f, 175f);
        if (origin.x > 0f) bend = -bend;
        for (int i = 0; i < count; i++)
        {
            var token = tokens[i] = hud.pickupPool.Get();
            token.sprite = collect ? hud.hexPickupSprite : icon.sprite;
            token.color = color;
            var rect = token.rectTransform;
            rect.SetAsLastSibling();
            rect.localPosition = origin;
            rect.localScale = Vector3.one;
            token.enabled = false;
            float delay = i * 0.072f;
            float duration = 0.52f + 0.012f * (i % 3);
            Vector2 control = Vector2.Lerp(origin, end, 0.35f) + sideways * (bend + Mathf.Sin(i * 2.4f) * 50f);
            sequence.InsertCallback(delay, () => token.enabled = true);
            sequence.Insert(delay, DOVirtual.Float(0f, 1f, duration, t =>
            {
                float u = 1f - t;
                rect.localPosition = u * u * (Vector3)origin + 2f * u * t * (Vector3)control + t * t * end;
            }).SetEase(Ease.InQuad));
            sequence.Insert(delay, rect.DOScale(0.55f, duration));
            int credit = (i + 1) * amount / count - i * amount / count;
            sequence.InsertCallback(delay + duration, () =>
            {
                token.gameObject.SetActive(false);
                view.Bounce();
                onArrival?.Invoke(credit);
            });
        }

        await sequence.ToUniTask();
        if (hud != null)
            foreach (var token in tokens) hud.pickupPool.Release(token);
    }

    private void UpdateTaskViews(int collected, int collectTarget, int broken, int breakTarget)
    {
        for (int i = 0; i < taskViews.Count; i++)
        {
            TaskView view = taskViews[i];
            int progress = view.IsCollectTask ? collected : broken;
            int target = view.IsCollectTask ? collectTarget : breakTarget;
            int shownProgress = Mathf.Clamp(progress, 0, target);

            if (view.ProgressText != null)
                view.ProgressText.text = view.IsCollectTask
                    ? $"{shownProgress}/{target}"
                    : (target - shownProgress).ToString();

            if (view.ProgressBar != null)
                view.ProgressBar.value = target > 0 ? (float)shownProgress / target : 1f;
        }
    }
}
