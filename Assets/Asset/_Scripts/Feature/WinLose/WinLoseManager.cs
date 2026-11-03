using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class WinLoseManager : MonoBehaviour
{
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private GridController gridController;
    [SerializeField] private MergeService mergeService;
    [SerializeField] private string menuSceneName = "UI scene";
    private GameManager gameManager;
    private GameObject resultPanel;
    private TMP_Text resultText;
    private TMP_FontAsset panelFont;
    private Button continueButton;
    private Button retryButton;
    private Button backToMenuButton;
    private Result currentResult;
    private bool isTransitioning;

    private void Awake()
    {
        gameManager = GetComponent<GameManager>();
        CreateResultPanelIfNeeded();
    }

    public Result Evaluate(bool inputInProgress)
    {
        if (taskManager == null || gridController == null || mergeService == null || !taskManager.HasLevelData)
        {
            return Result.None;
        }

        if (inputInProgress || !mergeService.IsFinishAllMerge())
        {
            return Result.None;
        }

        if (taskManager.IsTaskCompleted())
        {
            return Result.Win;
        }

        return gridController.IsFull() ? Result.Lose : Result.None;
    }

    public void ShowWin()
    {
        int awardedReward = 0;
        if (gameManager.LevelLoader != null)
        {
            bool rewardGranted = gameManager.SaveLoadServices.CompleteLevelAndSave(
                gameManager.LevelLoader.CurrentLevelID,
                gameManager.LevelLoader.CurrentLevelReward);
            if (rewardGranted)
            {
                awardedReward = Mathf.Max(0, gameManager.LevelLoader.CurrentLevelReward);
            }
        }
        ShowResult(Result.Win, awardedReward);
    }

    public void ShowLose()
    {
        ShowResult(Result.Lose);
    }

    private void ShowResult(Result result, int awardedReward = 0)
    {
        currentResult = result;
        isTransitioning = false;
        resultText.text = result == Result.Win
            ? awardedReward > 0 ? $"WIN\n+{awardedReward} GOLD" : "WIN"
            : "LOSE";
        continueButton.gameObject.SetActive(result == Result.Win);
        retryButton.gameObject.SetActive(result == Result.Lose);
        backToMenuButton.gameObject.SetActive(result == Result.Lose);
        resultPanel.SetActive(true);
        resultPanel.transform.SetAsLastSibling();
    }

    private void ContinueAfterWin()
    {
        if (currentResult != Result.Win || isTransitioning) return;
        isTransitioning = true;
        resultPanel.SetActive(false);
        gameManager.StateMachine.ChangeState(gameManager.PreparePlayingState);
    }

    private void RetryAfterLose()
    {
        if (currentResult != Result.Lose || isTransitioning) return;
        isTransitioning = true;
        resultPanel.SetActive(false);
        gameManager.StateMachine.ChangeState(gameManager.PreparePlayingState);
    }

    private void BackToMenu()
    {
        if (currentResult != Result.Lose || isTransitioning) return;
        isTransitioning = true;
        SceneManager.LoadScene(menuSceneName);
    }

    private void CreateResultPanelIfNeeded()
    {
        if (resultPanel != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("WinLoseManager requires a Canvas to show the level result.", this);
            return;
        }

        TMP_Text existingText = canvas.GetComponentInChildren<TMP_Text>(true);
        panelFont = existingText != null ? existingText.font : TMP_Settings.defaultFontAsset;

        resultPanel = new GameObject("Level Result Panel", typeof(RectTransform), typeof(Image));
        RectTransform panelRect = resultPanel.GetComponent<RectTransform>();
        panelRect.SetParent(canvas.transform, false);
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image overlay = resultPanel.GetComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.72f);
        overlay.raycastTarget = true;

        GameObject card = CreateElement("Result Card", resultPanel.transform, new Vector2(320f, 330f), Vector2.zero);
        Image cardImage = card.AddComponent<Image>();
        cardImage.color = new Color(1f, 0.94f, 0.78f, 1f);

        resultText = CreateText("Result Text", card.transform, "WIN", 46f, new Vector2(0f, 95f), new Vector2(280f, 72f));
        resultText.fontSize = 38f;
        resultText.rectTransform.sizeDelta = new Vector2(280f, 100f);
        resultText.color = new Color(0.25f, 0.16f, 0.08f, 1f);
        resultText.fontStyle = FontStyles.Bold;

        continueButton = CreateButton("Continue Button", card.transform, "CONTINUE", new Vector2(0f, 5f));
        retryButton = CreateButton("Retry Button", card.transform, "RETRY", new Vector2(0f, 35f));
        backToMenuButton = CreateButton("Back To Menu Button", card.transform, "BACK TO MENU", new Vector2(0f, -45f));

        continueButton.onClick.AddListener(ContinueAfterWin);
        retryButton.onClick.AddListener(RetryAfterLose);
        backToMenuButton.onClick.AddListener(BackToMenu);
        resultPanel.SetActive(false);
    }

    private static GameObject CreateElement(string name, Transform parent, Vector2 size, Vector2 position)
    {
        GameObject element = new GameObject(name, typeof(RectTransform));
        RectTransform rect = element.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return element;
    }

    private TMP_Text CreateText(string name, Transform parent, string value, float size, Vector2 position, Vector2 rectSize)
    {
        GameObject textObject = CreateElement(name, parent, rectSize, position);
        TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = panelFont;
        text.fontSize = size;
        text.alignment = TextAlignmentOptions.Center;
        return text;
    }

    private Button CreateButton(string name, Transform parent, string label, Vector2 position)
    {
        GameObject buttonObject = CreateElement(name, parent, new Vector2(250f, 58f), position);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.94f, 0.48f, 0.16f, 1f);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        TMP_Text text = CreateText("Label", buttonObject.transform, label, 24f, Vector2.zero, new Vector2(230f, 52f));
        text.color = Color.white;
        text.fontStyle = FontStyles.Bold;
        text.raycastTarget = false;
        return button;
    }

    public enum Result
    {
        None,
        Win,
        Lose
    }
}
