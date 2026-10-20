using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace HexaSort.UI
{
    public class MainMenuUIEffects : MonoBehaviour
    {
        [Header("Hero Island")]
        [SerializeField] private RectTransform islandTransform;
        [SerializeField] private RectTransform[] seagulls;

        [Header("Main Play Button")]
        [SerializeField] private RectTransform playButtonTransform;
        [SerializeField] private Button playButton;
        [SerializeField] private RectTransform playButtonShine;

        [Header("Top Bar")]
        [SerializeField] private Button avatarButton;
        [SerializeField] private Button settingsButton;

        [Header("Floating Side Buttons")]
        [SerializeField] private Button[] sideButtons;
        [SerializeField] private RectTransform colorWheelTransform;
        [SerializeField] private RectTransform piggyTransform;
        [SerializeField] private RectTransform[] lockBadges;
        [SerializeField] private RectTransform[] notificationDots;

        [Header("Bottom Action Buttons")]
        [SerializeField] private Button mailboxButton;
        [SerializeField] private RectTransform mailboxIcon;
        [SerializeField] private RectTransform checklistIcon;

        [Header("Navigation Bar")]
        [SerializeField] private Button[] navTabButtons;
        [SerializeField] private RectTransform[] navTabRoots;
        [SerializeField] private Color activeNavColor = Color.white;
        [SerializeField] private Color inactiveNavColor = new Color(0.45f, 0.52f, 0.72f, 1f);

        [Header("Background Sparkles")]
        [SerializeField] private RectTransform[] backgroundStars;

        private int currentTabIndex = 1;
        private Vector3 islandInitPos;
        private Vector2 badgeInitPos;
        private Coroutine badgeHopCoroutine;
        private Coroutine lockRattleCoroutine;
        private Coroutine shineCoroutine;

        private void Start()
        {
            SetupHeroIsland();
            SetupPlayButton();
            SetupTopBar();
            SetupSideButtons();
            SetupBottomActionButtons();
            SetupNavigationTabs();
            SetupBackgroundStars();
        }

        #region Hero Island & Badge
        private void SetupHeroIsland()
        {
            if (islandTransform != null)
            {
                islandInitPos = islandTransform.localPosition;
                islandTransform.DOLocalMoveY(islandInitPos.y + 14f, 2.4f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
                islandTransform.DORotate(new Vector3(0, 0, 1.2f), 3.4f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
            if (seagulls != null)
            {
                for (int i = 0; i < seagulls.Length; i++)
                {
                    if (seagulls[i] == null) continue;
                    float delay = i * 0.45f;
                    Vector3 initPos = seagulls[i].localPosition;
                    seagulls[i].DOLocalMove(initPos + new Vector3(10f + i * 4f, 7f + i * 3f, 0), 1.9f + i * 0.4f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetDelay(delay);
                }
            }
        }
        #endregion

        #region Main Play Button ("LEVEL 6")
        private void SetupPlayButton()
        {
            if (playButtonTransform != null)
            {
                // Rhythmic pulsing heartbeat
                playButtonTransform.DOScale(1.045f, 0.85f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }

            if (playButtonShine != null)
            {
                shineCoroutine = StartCoroutine(PlayButtonShineRoutine());
            }

            if (playButton != null)
            {
                playButton.onClick.AddListener(() =>
                {
                    if (playButtonTransform == null) return;
                    playButtonTransform.DOKill(true);

                    playButtonTransform.DOPunchScale(new Vector3(0.15f, -0.1f, 0), 0.35f, 10, 0.7f)
                        .OnComplete(() =>
                        {
                            playButtonTransform.DOScale(1.045f, 0.85f)
                                .SetEase(Ease.InOutSine)
                                .SetLoops(-1, LoopType.Yoyo);
                        });
                });
            }
        }

        private IEnumerator PlayButtonShineRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(2.8f);
                if (playButtonShine != null)
                {
                    playButtonShine.anchoredPosition = new Vector2(-360, 0);
                    playButtonShine.DOAnchorPosX(360, 0.7f).SetEase(Ease.InOutQuad);
                }
            }
        }
        #endregion

        #region Top Bar
        private void SetupTopBar()
        {
            if (avatarButton != null)
            {
                avatarButton.onClick.AddListener(() =>
                {
                    avatarButton.transform.DOKill(true);
                    Sequence avatarSeq = DOTween.Sequence();
                    avatarSeq.Append(avatarButton.transform.DOScale(new Vector3(1.16f, 0.86f, 1f), 0.1f).SetEase(Ease.OutQuad));
                    avatarSeq.Append(avatarButton.transform.DOScale(new Vector3(0.92f, 1.10f, 1f), 0.12f).SetEase(Ease.OutQuad));
                    avatarSeq.Append(avatarButton.transform.DOScale(Vector3.one, 0.18f).SetEase(Ease.OutBack));
                });
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(() =>
                {
                    settingsButton.transform.DOKill(true);
                    settingsButton.transform.DOPunchScale(new Vector3(0.16f, 0.16f, 0), 0.35f, 8, 0.5f);
                    settingsButton.transform.DORotate(new Vector3(0, 0, -360f), 0.5f, RotateMode.FastBeyond360)
                        .SetEase(Ease.OutBack);
                });
            }
        }
        #endregion

        #region Side Buttons & Micro-Interactions
        private void SetupSideButtons()
        {
            if (sideButtons != null)
            {
                foreach (var btn in sideButtons)
                {
                    if (btn == null) continue;
                    btn.onClick.AddListener(() =>
                    {
                        btn.transform.DOKill(true);
                        btn.transform.DOPunchScale(new Vector3(-0.12f, -0.12f, 0), 0.25f, 8, 0.5f);
                    });
                }
            }

            // Color wheel continuous rotation
            if (colorWheelTransform != null)
            {
                colorWheelTransform.DORotate(new Vector3(0, 0, -360f), 10f, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1);
            }
            

            // Piggy bank idle breathing
            if (piggyTransform != null)
            {
                Sequence pigSeq = DOTween.Sequence();
                pigSeq.Append(piggyTransform.DOScale(new Vector3(1.05f, 0.95f, 1f), 0.9f).SetEase(Ease.InOutSine));
                pigSeq.Append(piggyTransform.DOScale(new Vector3(0.96f, 1.04f, 1f), 0.9f).SetEase(Ease.InOutSine));
                pigSeq.SetLoops(-1);
            }
            

            // Notification dots attention wobble
            if (notificationDots != null)
            {
                for (int i = 0; i < notificationDots.Length; i++)
                {
                    var dot = notificationDots[i];
                    if (dot == null) continue;
                    StartCoroutine(PeriodicDotWobbleRoutine(dot, i * 0.4f));
                }
            }

            // Periodic padlock curiosity rattles
            if (lockBadges != null && lockBadges.Length > 0)
            {
                lockRattleCoroutine = StartCoroutine(PeriodicLockRattleRoutine());
            }
        }

        private IEnumerator PeriodicDotWobbleRoutine(RectTransform dot, float startDelay)
        {
            yield return new WaitForSeconds(startDelay);
            while (true)
            {
                if (dot != null)
                {
                    dot.DOPunchRotation(new Vector3(0, 0, 16f), 0.55f, 10, 0.7f);
                    dot.DOScale(1.25f, 0.28f).SetEase(Ease.OutQuad).OnComplete(() =>
                    {
                        dot.DOScale(1.0f, 0.22f).SetEase(Ease.OutBack);
                    });
                }
                yield return new WaitForSeconds(3.5f + Random.Range(-0.3f, 0.5f));
            }
        }

        private IEnumerator PeriodicLockRattleRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(4.2f + Random.Range(0f, 1.5f));
                if (lockBadges != null && lockBadges.Length > 0)
                {
                    int randomIndex = Random.Range(0, lockBadges.Length);
                    var lk = lockBadges[randomIndex];
                    if (lk != null)
                    {
                        lk.DOPunchRotation(new Vector3(0, 0, 14f), 0.45f, 12, 0.8f);
                    }
                }
            }
        }
        #endregion

        #region Bottom Action Buttons (Mailbox & Checklist)
        private void SetupBottomActionButtons()
        {
            if (mailboxButton != null)
            {
                mailboxButton.onClick.AddListener(() =>
                {
                    if (mailboxIcon != null)
                    {
                        mailboxIcon.DOKill(true);
                        mailboxIcon.DOPunchRotation(new Vector3(0, 0, 16f), 0.38f, 8, 0.6f);
                    }
                });
            }
            
        }
        #endregion

        #region Navigation Tabs Switch Feel
        private void SetupNavigationTabs()
        {
            if (navTabButtons == null || navTabButtons.Length == 0) return;

            for (int i = 0; i < navTabButtons.Length; i++)
            {
                int index = i;
                navTabButtons[i].onClick.AddListener(() => SelectNavTab(index));
            }

            UpdateNavTabVisuals(currentTabIndex, false);
        }

        public void SelectNavTab(int index)
        {
            if (index == currentTabIndex)
            {
                if (navTabRoots != null && index < navTabRoots.Length && navTabRoots[index] != null)
                {
                    navTabRoots[index].DOKill(true);
                    navTabRoots[index].DOPunchScale(new Vector3(0.12f, 0.12f, 0), 0.25f, 8, 0.5f);
                }
                return;
            }

            currentTabIndex = index;
            UpdateNavTabVisuals(index, true);
        }

        private void UpdateNavTabVisuals(int selectedIndex, bool animate)
        {
            if (navTabRoots == null) return;

            for (int i = 0; i < navTabRoots.Length; i++)
            {
                if (navTabRoots[i] == null) continue;
                bool isSelected = (i == selectedIndex);

                if (animate && isSelected)
                {
                    navTabRoots[i].DOKill(true);
                    Sequence tabSeq = DOTween.Sequence();
                    tabSeq.Append(navTabRoots[i].DOScale(new Vector3(1.15f, 0.90f, 1f), 0.1f).SetEase(Ease.OutQuad));
                    tabSeq.Append(navTabRoots[i].DOScale(new Vector3(0.96f, 1.06f, 1f), 0.1f).SetEase(Ease.OutQuad));
                    tabSeq.Append(navTabRoots[i].DOScale(Vector3.one, 0.14f).SetEase(Ease.OutBack));
                }

                Image iconImg = navTabRoots[i].Find("Icon")?.GetComponent<Image>();
                if (iconImg != null)
                {
                    Color targetColor = isSelected ? activeNavColor : inactiveNavColor;
                    float targetY = isSelected ? 16f : 0f;
                    RectTransform iconRt = iconImg.rectTransform;

                    if (animate)
                    {
                        iconImg.DOColor(targetColor, 0.2f);
                        iconRt.DOAnchorPosY(targetY, 0.2f).SetEase(Ease.OutQuad);
                    }
                    else
                    {
                        iconImg.color = targetColor;
                        iconRt.anchoredPosition = new Vector2(0, targetY);
                    }
                }

                Transform labelTransform = navTabRoots[i].Find("Label");
                if (labelTransform != null)
                {
                    labelTransform.gameObject.SetActive(isSelected);
                }

                Transform activeBg = navTabRoots[i].Find("ActivePlate");
                if (activeBg != null)
                {
                    activeBg.gameObject.SetActive(isSelected);
                    if (isSelected && animate)
                    {
                        activeBg.localScale = new Vector3(0.7f, 0.7f, 1f);
                        activeBg.DOScale(Vector3.one, 0.22f).SetEase(Ease.OutBack);
                    }
                }
            }
        }
        #endregion

        #region Background Stars
        private void SetupBackgroundStars()
        {
            if (backgroundStars == null) return;

            for (int i = 0; i < backgroundStars.Length; i++)
            {
                var star = backgroundStars[i];
                if (star == null) continue;

                float duration = 1.2f + (i % 4) * 0.45f;
                float delay = (i * 0.35f) % 2.5f;

                star.DOScale(1.35f, duration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetDelay(delay);

                CanvasGroup cg = star.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.DOFade(0.25f, duration)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetDelay(delay);
                }
            }
        }
        #endregion

        private void OnDestroy()
        {
            DOTween.Kill(transform);
            if (islandTransform != null) DOTween.Kill(islandTransform);
            if (playButtonTransform != null) DOTween.Kill(playButtonTransform);
            if (colorWheelTransform != null) DOTween.Kill(colorWheelTransform);
            if (piggyTransform != null) DOTween.Kill(piggyTransform);
            if (badgeHopCoroutine != null) StopCoroutine(badgeHopCoroutine);
            if (lockRattleCoroutine != null) StopCoroutine(lockRattleCoroutine);
            if (shineCoroutine != null) StopCoroutine(shineCoroutine);
        }
    }
}
