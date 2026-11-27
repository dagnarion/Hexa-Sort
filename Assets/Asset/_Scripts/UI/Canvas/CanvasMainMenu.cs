using DG.Tweening;
using TMPro;
using UnityEngine;

public class CanvasMainMenu : MonoBehaviour
{
    [SerializeField] private RectTransform[] panels;
    [SerializeField] private float duration = 0.35f;
    [SerializeField] private TMP_Text heartCount;
    [SerializeField] private TMP_Text refillTimer;
    [SerializeField] private UnityEngine.UI.Button standalonePlayButton;
    private SaveLoadServices save;
    private double nextRefresh;

    private void OnEnable()
    {
        if (heartCount == null || refillTimer == null) return;
        save = SaveLoadServices.FindForMenu(gameObject);
        RefreshLives();
    }

    private void Update()
    {
        if (save != null && Time.realtimeSinceStartupAsDouble >= nextRefresh)
            RefreshLives();
    }

    private void RefreshLives()
    {
        int lives = save.GetLives(out int seconds);
        heartCount.SetText("{0}", lives);
        refillTimer.text = seconds == 0 ? "FULL" : $"{seconds / 60:00}:{seconds % 60:00}";
        nextRefresh = Time.realtimeSinceStartupAsDouble + 1;
    }

    private int currentIndex = 1;
    private int direction = 1;
    private Tween moveTween;
    private RectTransform currentPanel;
    private RectTransform newPanel;

    private float Width => ((RectTransform)transform).rect.width;

    private void Start()
    {
        if (standalonePlayButton != null)
            standalonePlayButton.onClick.AddListener(() => GameManager.PlayFromStandaloneMenu(save));
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].anchoredPosition =
                Vector2.right * Width * (i - 1);
        }
    }

    public void SlideTo(int index)
    {
        if (index == currentIndex || moveTween.IsActive())
            return;

        direction = currentIndex < index ? -1 : 1;

        currentPanel = panels[currentIndex];
        newPanel = panels[index];
        currentIndex = index;
        newPanel.gameObject.SetActive(true);
        newPanel.anchoredPosition = Vector2.right * (-direction * Width);
        float value = 0f;
        moveTween = DOTween.To(() => value, x => value = x, Width, duration).OnUpdate(() => MoveAnim(value));
        moveTween.OnComplete(() => {currentPanel.gameObject.SetActive(false);});
    }

    private void MoveAnim(float value)
    {
        currentPanel.anchoredPosition = Vector2.right * (direction * value);
        newPanel.anchoredPosition = Vector2.right * (-direction * (Width - value));
    }
    
}
