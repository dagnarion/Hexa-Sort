using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmp;
    [SerializeField] private Slider progressBar;
    private TaskManager taskManager;
    [SerializeField] private TextMeshProUGUI blockTmp;

    private void Awake()
    {
        taskManager = GetComponent<TaskManager>();
    }

    private void OnEnable()
    {
        if (taskManager == null) taskManager = GetComponent<TaskManager>();
        if (taskManager == null) return;
        taskManager.ProgressChanged += UpdateUI;
        if (taskManager.TryGetProgress(out int collected, out int collectTarget, out int broken, out int breakTarget))
        {
            UpdateUI(collected, collectTarget, broken, breakTarget);
        }
    }

    private void OnDisable()
    {
        if (taskManager != null) taskManager.ProgressChanged -= UpdateUI;
    }

    private void UpdateUI(int collected, int collectTarget, int broken, int breakTarget)
    {
        if (tmp == null || progressBar == null || blockTmp == null) return;

        int shownCollected = Mathf.Min(collected, collectTarget);
        int shownBroken = Mathf.Min(broken, breakTarget);
        progressBar.value = collectTarget > 0 ? Mathf.Clamp01((float)shownCollected / collectTarget) : 1f;
        tmp.text = $"{shownCollected}/{collectTarget}";

        bool hasBreakTask = breakTarget > 0;
        blockTmp.gameObject.SetActive(hasBreakTask);
        if (!hasBreakTask) return;
        blockTmp.text = $"{breakTarget - shownBroken}";
    }
    
}
