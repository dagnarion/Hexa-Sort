using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public partial class WinLoseManager : MonoBehaviour
{
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private GridController gridController;
    [SerializeField] private MergeService mergeService;
    [SerializeField] private string menuSceneName = "UI scene";
    private GameManager gameManager;
    [Header("Lose panel")]
    [SerializeField] private GameObject losePanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button backToMenuButton;
    private Result currentResult;
    private bool isTransitioning;
    private bool winRecorded;

    public void BeginAttempt() => winRecorded = false;

#if UNITY_EDITOR
    public void HideResultForTest()
    {
        if (losePanel != null) losePanel.SetActive(false);
        CloseWinPanel();
    }
#endif

    private void Awake()
    {
        gameManager = GetComponent<GameManager>();
        InitializeWinPanel();
        losePanel.SetActive(false);
        retryButton.onClick.AddListener(RetryAfterLose);
        backToMenuButton.onClick.AddListener(BackToMenu);
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

    public int CompleteWinAndSave()
    {
        if (winRecorded || gameManager.LevelLoader == null || gameManager.SaveLoadServices == null ||
            gameManager.LevelLoader.CurrentLevelID < 0) return 0;
        int oldBalance = gameManager.SaveLoadServices.Gold;
        bool rewardGranted = gameManager.SaveLoadServices.CompleteLevelAndSave(
            gameManager.LevelLoader.CurrentLevelID,
            gameManager.LevelLoader.CurrentLevelReward);
        winRecorded = rewardGranted;
        return rewardGranted ? gameManager.SaveLoadServices.Gold - oldBalance : 0;
    }

    public void ShowLose()
    {
        currentResult = Result.Lose;
        isTransitioning = false;
        losePanel.SetActive(true);
        losePanel.transform.SetAsLastSibling();
    }

    private void ContinueAfterWin()
    {
        if (currentResult != Result.Win || isTransitioning) return;
        isTransitioning = true;
        CloseWinPanel();
        gameManager.StateMachine.ChangeState(gameManager.PreparePlayingState);
    }

    private void RetryAfterLose()
    {
        if (currentResult != Result.Lose || isTransitioning) return;
        if (!OutOfLivesPopup.CanPlay(gameManager.SaveLoadServices)) return;
        isTransitioning = true;
        losePanel.SetActive(false);
        gameManager.StateMachine.ChangeState(gameManager.PreparePlayingState);
    }

    private void BackToMenu()
    {
        if (isTransitioning || (currentResult != Result.Lose && currentResult != Result.Win)) return;
        isTransitioning = true;
        SceneManager.LoadScene(menuSceneName);
    }

    public enum Result
    {
        None,
        Win,
        Lose
    }
}
