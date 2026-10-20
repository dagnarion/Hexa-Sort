using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace HexaSort.UI
{
    public class ShopUIController : MonoBehaviour
    {
        public static ShopUIController Instance { get; private set; }

        [Header("Canvas & Layout")]
        [SerializeField] private Canvas shopCanvas;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentRect;
        [SerializeField] private RectTransform coinGridSection;

        [Header("Currency Bar")]
        [SerializeField] private TMP_Text coinBalanceText;
        [SerializeField] private Button coinPlusButton;
        [SerializeField] private RectTransform coinPillTransform;
        [SerializeField] private int initialBalance = 219;

        [Header("Purchase Cancelled Popup")]
        [SerializeField] private ShopPurchasePopup purchasePopup;

        [Header("Bottom Navigation")]
        [SerializeField] private ShopNavTab[] navTabs;
        [SerializeField] private int defaultSelectedTabIndex = 1; // Shop tab

        private int currentCoins;
        private Tween coinTween;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (shopCanvas == null) shopCanvas = GetComponentInParent<Canvas>();
            currentCoins = initialBalance;
            UpdateCoinText(currentCoins);

            if (coinPlusButton != null)
            {
                coinPlusButton.onClick.AddListener(OnCoinPlusClicked);
            }
        }

        private void Start()
        {
            InitNavTabs();
        }

        private void InitNavTabs()
        {
            if (navTabs != null && navTabs.Length > 0)
            {
                for (int i = 0; i < navTabs.Length; i++)
                {
                    if (navTabs[i] != null)
                    {
                        navTabs[i].SetSelected(i == defaultSelectedTabIndex, false);
                    }
                }
            }
        }

        public void OnNavTabClicked(int index)
        {
            if (navTabs == null) return;

            for (int i = 0; i < navTabs.Length; i++)
            {
                if (navTabs[i] != null)
                {
                    navTabs[i].SetSelected(i == index, true);
                }
            }
        }

        public void OnCoinPlusClicked()
        {
            // Visual feedback on coin pill
            if (coinPillTransform != null)
            {
                coinPillTransform.DOKill();
                coinPillTransform.localScale = Vector3.one;
                coinPillTransform.DOPunchScale(new Vector3(0.18f, -0.1f, 0f), 0.25f, 10, 1f);
            }

            ScrollToCoins();
        }

        public void ScrollToCoins()
        {
            if (scrollRect == null || contentRect == null) return;

            // Scroll down towards the coin packs section
            // In a standard top-to-bottom ScrollRect, 1 is top and 0 is bottom
            // The coin packs grid is in the lower half (approx normalized pos 0.25f - 0.35f)
            float targetNormPos = 0.28f;

            if (coinGridSection != null)
            {
                // Calculate precise normalized position if possible
                float contentHeight = contentRect.rect.height - scrollRect.viewport.rect.height;
                if (contentHeight > 0)
                {
                    float targetY = Mathf.Abs(coinGridSection.anchoredPosition.y);
                    targetNormPos = Mathf.Clamp01(1f - (targetY / contentHeight));
                }
            }

            scrollRect.DOKill();
            DOTween.To(() => scrollRect.verticalNormalizedPosition,
                       x => scrollRect.verticalNormalizedPosition = x,
                       targetNormPos, 0.45f)
                   .SetEase(Ease.OutCubic);
        }

        public void OpenPurchaseCancelledPopup(string productName = "", string price = "")
        {
            if (purchasePopup != null)
            {
                purchasePopup.Show();
            }
        }

        public void ClosePurchaseCancelledPopup()
        {
            if (purchasePopup != null)
            {
                purchasePopup.Hide();
            }
        }

        public void SetShopVisible(bool visible)
        {
            if (shopCanvas != null)
            {
                shopCanvas.gameObject.SetActive(visible);
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }

        public void AddMockCoins(int amount)
        {
            int startVal = currentCoins;
            currentCoins += amount;

            coinTween?.Kill();
            coinTween = DOTween.To(() => startVal, x => UpdateCoinText(x), currentCoins, 0.5f)
                .SetEase(Ease.OutQuad);

            if (coinPillTransform != null)
            {
                coinPillTransform.DOPunchScale(new Vector3(0.15f, 0.15f, 0f), 0.3f);
            }
        }

        private void UpdateCoinText(int val)
        {
            if (coinBalanceText != null)
            {
                coinBalanceText.text = val.ToString("N0");
            }
        }
    }
}
