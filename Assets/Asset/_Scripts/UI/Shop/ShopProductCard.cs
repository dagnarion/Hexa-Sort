using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HexaSort.UI
{
    public class ShopProductCard : MonoBehaviour
    {
        [Header("Card Info")]
        [SerializeField] private string productId;
        [SerializeField] private string productName;
        [SerializeField] private string priceText;

        [Header("UI References")]
        [SerializeField] private Button buyButton;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text priceLabel;

        private void Awake()
        {
            if (buyButton == null)
            {
                buyButton = GetComponentInChildren<Button>();
            }

            if (buyButton != null)
            {
                buyButton.onClick.AddListener(OnBuyClicked);
            }

            UpdateDisplay();
        }

        public void Setup(string id, string name, string price)
        {
            productId = id;
            productName = name;
            priceText = price;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (titleLabel != null && !string.IsNullOrEmpty(productName))
            {
                titleLabel.text = productName;
            }

            if (priceLabel != null && !string.IsNullOrEmpty(priceText))
            {
                priceLabel.text = priceText;
            }
        }

        private void OnBuyClicked()
        {
            if (ShopUIController.Instance != null)
            {
                ShopUIController.Instance.OpenPurchaseCancelledPopup(productName, priceText);
            }
        }
    }
}
