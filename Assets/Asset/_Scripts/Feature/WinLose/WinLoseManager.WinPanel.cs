using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class WinLoseManager
{
    [Header("Win panel")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private Button claim;
    [SerializeField] private TMP_Text claimLabel;
    [SerializeField] private CanvasGroup navigation;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private GameObject bars;
    [SerializeField] private TMP_Text coins;
    [SerializeField] private TMP_Text lives;
    [SerializeField] private RectTransform destination;
    [SerializeField] private RectTransform flyingCoinTemplate;

    private Sequence claimAnimation;
    private readonly System.Collections.Generic.List<RectTransform> flyingCoins = new System.Collections.Generic.List<RectTransform>();
    private string claimFormat;
    private int reward;

    private void InitializeWinPanel()
    {
        if (winPanel == null) return;
        winPanel.SetActive(false);
        claimFormat = claimLabel.text;
        claim.onClick.AddListener(AnimateClaim);
        nextLevelButton.onClick.AddListener(ContinueAfterWin);
        mainMenuButton.onClick.AddListener(BackToMenu);
    }

    public void ShowWin(int awardedReward)
    {
        if (winPanel.activeSelf) return;
        currentResult = Result.Win;
        isTransitioning = false;
        reward = awardedReward;
        claimLabel.text = claimFormat.Replace("{reward}", reward.ToString());
        bars.SetActive(false);
        navigation.gameObject.SetActive(false);
        navigation.alpha = 0;
        navigation.interactable = navigation.blocksRaycasts = false;
        claim.gameObject.SetActive(true);
        claim.interactable = true;
        flyingCoinTemplate.gameObject.SetActive(false);
        winPanel.SetActive(true);
    }

    private void AnimateClaim()
    {
        if (!claim.interactable) return;
        claim.interactable = false;
        var wallet = gameManager.SaveLoadServices;
        int balance = wallet.Gold;
        int start = Mathf.Max(0, balance - reward);
        bars.SetActive(true);
        Canvas.ForceUpdateCanvases();
        lives.text = wallet.InfiniteLifeSeconds > 0 ? "∞" : wallet.GetLives(out _).ToString();
        coins.text = start.ToString();

        claimAnimation = DOTween.Sequence().SetUpdate(true).SetLink(winPanel);
        int count = Mathf.Min(12, reward);
        Transform flightParent = flyingCoinTemplate.parent;
        Vector3 origin = flightParent.InverseTransformPoint(claim.transform.position);
        Vector3 end = flightParent.InverseTransformPoint(destination.position);
        for (int i = 0; i < count; i++)
        {
            RectTransform rect = Instantiate(flyingCoinTemplate, flightParent, false);
            rect.localPosition = origin;
            flyingCoins.Add(rect);
            GameObject coin = rect.gameObject;
            float delay = i * 0.075f;
            Vector3 spread = origin + new Vector3(Mathf.Sin(i * 2.4f) * 130, 75 + (i % 3) * 24);
            int arrivalBalance = start + (int)((long)reward * (i + 1) / count);
            claimAnimation.InsertCallback(delay, () => coin.SetActive(true));
            claimAnimation.Insert(delay, rect.DOLocalMove(spread, 0.2f).SetEase(Ease.OutQuad));
            claimAnimation.Insert(delay + 0.2f, rect.DOLocalMove(end, 0.56f).SetEase(Ease.InQuad));
            claimAnimation.InsertCallback(delay + 0.76f, () =>
            {
                coin.SetActive(false);
                coins.text = arrivalBalance.ToString();
            });
        }

        claimAnimation.AppendInterval(0.18f);
        claimAnimation.AppendCallback(() =>
        {
            bars.SetActive(false);
            claim.gameObject.SetActive(false);
            navigation.gameObject.SetActive(true);
        });
        claimAnimation.Append(navigation.DOFade(1, 0.35f));
        claimAnimation.OnComplete(() => navigation.interactable = navigation.blocksRaycasts = true);
    }

    private void CloseWinPanel()
    {
        claimAnimation.Kill();
        if (winPanel == null) return;
        winPanel.SetActive(false);
        foreach (var coin in flyingCoins) if (coin != null) Destroy(coin.gameObject);
        flyingCoins.Clear();
    }
}
