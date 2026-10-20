using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace HexaSort.UI
{
    public class ShopPurchasePopup : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform windowTransform;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button backdropButton;

        [Header("Animation Settings")]
        [SerializeField] private float animDuration = 0.28f;

        private Tween scaleTween;
        private Tween fadeTween;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (windowTransform == null) windowTransform = transform.Find("Window") as RectTransform;

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }

            if (backdropButton != null)
            {
                backdropButton.onClick.AddListener(Hide);
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                canvasGroup.alpha = 0f;

                fadeTween?.Kill();
                fadeTween = canvasGroup.DOFade(1f, animDuration * 0.75f).SetUpdate(true);
            }

            if (windowTransform != null)
            {
                scaleTween?.Kill();
                windowTransform.localScale = Vector3.one * 0.72f;
                scaleTween = windowTransform.DOScale(1f, animDuration)
                    .SetEase(Ease.OutBack, 1.35f)
                    .SetUpdate(true);
            }
        }

        public void Hide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
                fadeTween?.Kill();
                fadeTween = canvasGroup.DOFade(0f, 0.16f).SetUpdate(true);
            }

            if (windowTransform != null)
            {
                scaleTween?.Kill();
                scaleTween = windowTransform.DOScale(0.8f, 0.16f)
                    .SetEase(Ease.InQuad)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
                        gameObject.SetActive(false);
                    });
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
