using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmp;
    [SerializeField] private Slider progressBar;
    private TaskManager taskManager;
    private TextMeshProUGUI blockTmp;
    private Slider blockProgressBar;

    private void Awake()
    {
        taskManager = GetComponent<TaskManager>();
        CreateBlockProgressUI();
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
        if (tmp == null || progressBar == null || blockTmp == null || blockProgressBar == null) return;

        int shownCollected = Mathf.Min(collected, collectTarget);
        int shownBroken = Mathf.Min(broken, breakTarget);
        progressBar.value = collectTarget > 0 ? Mathf.Clamp01((float)shownCollected / collectTarget) : 1f;
        tmp.text = $"HEX {shownCollected}/{collectTarget}";

        bool hasBreakTask = breakTarget > 0;
        blockTmp.gameObject.SetActive(hasBreakTask);
        blockProgressBar.gameObject.SetActive(hasBreakTask);
        if (!hasBreakTask) return;

        blockProgressBar.value = Mathf.Clamp01((float)shownBroken / breakTarget);
        blockTmp.text = $"BLOCKS {shownBroken}/{breakTarget}";
    }

    private void CreateBlockProgressUI()
    {
        if (tmp == null || progressBar == null) return;

        tmp.fontSize = Mathf.Min(tmp.fontSize, 30f);
        tmp.rectTransform.sizeDelta = new Vector2(260f, tmp.rectTransform.sizeDelta.y);
        RectTransform progressRect = progressBar.transform as RectTransform;
        progressRect.sizeDelta = new Vector2(240f, progressRect.sizeDelta.y);

        blockTmp = Instantiate(tmp, tmp.transform.parent);
        blockTmp.name = "Block Task Progress";
        blockTmp.fontSize = Mathf.Min(blockTmp.fontSize, 30f);
        blockTmp.rectTransform.sizeDelta = new Vector2(260f, blockTmp.rectTransform.sizeDelta.y);
        blockTmp.rectTransform.anchoredPosition += Vector2.down * 82f;

        blockProgressBar = Instantiate(progressBar, progressBar.transform.parent);
        blockProgressBar.name = "Block Task Progress Bar";
        RectTransform blockProgressRect = blockProgressBar.transform as RectTransform;
        blockProgressRect.sizeDelta = new Vector2(240f, blockProgressRect.sizeDelta.y);
        blockProgressRect.anchoredPosition += Vector2.down * 82f;
    }
}
