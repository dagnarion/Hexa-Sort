using System.Collections.Generic;
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

    private readonly List<TaskView> taskViews = new List<TaskView>();
    private readonly List<GameObject> spawnedTasks = new List<GameObject>();

    private sealed class TaskView
    {
        public bool IsCollectTask;
        public TextMeshProUGUI ProgressText;
        public Slider ProgressBar;
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
