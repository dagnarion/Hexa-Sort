using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace HexaSort.UI
{
    public class ShopNavTab : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button tabButton;
        [SerializeField] private RectTransform iconTransform;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text tabLabel;
        [SerializeField] private GameObject activeIndicator;

        [Header("State")]
        [SerializeField] private bool isSelected = false;
        [SerializeField] private int tabIndex = 0;

        [Header("Styling")]
        [SerializeField] private Color selectedColor = Color.white;
        [SerializeField] private Color unselectedColor = new Color(0.6f, 0.65f, 0.85f, 0.75f);
        [SerializeField] private float selectedScale = 1.15f;

        private Tween scaleTween;

        public bool IsSelected => isSelected;
        public int TabIndex => tabIndex;

        private void Awake()
        {
            if (tabButton == null) tabButton = GetComponent<Button>();
            if (iconTransform == null && iconImage != null) iconTransform = iconImage.rectTransform;

            if (tabButton != null)
            {
                tabButton.onClick.AddListener(() =>
                {
                    if (ShopUIController.Instance != null)
                    {
                        ShopUIController.Instance.OnNavTabClicked(tabIndex);
                    }
                });
            }
        }

        public void SetSelected(bool selected, bool animate = true)
        {
            isSelected = selected;

            if (activeIndicator != null)
            {
                activeIndicator.SetActive(selected);
            }

            if (iconImage != null)
            {
                iconImage.color = selected ? selectedColor : unselectedColor;
            }

            if (tabLabel != null)
            {
                tabLabel.gameObject.SetActive(selected);
                tabLabel.color = selected ? selectedColor : unselectedColor;
            }

            if (iconTransform != null)
            {
                scaleTween?.Kill();
                float targetScale = selected ? selectedScale : 1.0f;
                if (animate)
                {
                    scaleTween = iconTransform.DOScale(targetScale, 0.22f).SetEase(Ease.OutBack);
                }
                else
                {
                    iconTransform.localScale = Vector3.one * targetScale;
                }
            }
        }
    }
}
