using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RewardKind = SaveLoadServices.RewardKind;

namespace HexaSort.UI
{
    public class LuckyWheelController : MonoBehaviour
    {
        [Header("Wheel Components")]
        [SerializeField] private RectTransform wheelDiscTransform;
        [SerializeField] private Button spinButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Canvas luckyWheelCanvas;

        [Header("Celebration & Visuals")]
        [SerializeField] private RectTransform[] prizeItemTransforms;
        [SerializeField] private TextMeshProUGUI subtitleText;

        [Header("Spin Configuration")]
        [SerializeField] private float spinDuration = 4.5f;
        [SerializeField] private int minFullTurns = 5;
        [SerializeField] private int maxFullTurns = 7;

        private bool m_IsSpinning = false;
        private SaveLoadServices save;
        private TMP_Text progressText;
        private TMP_Text spinText;
        private RectTransform progressFill;
        private RectTransform celebratingPrize;
        private Vector3 originalScale;
        private Coroutine m_SpinCoroutine;

        private readonly struct PrizeData
        {
            public readonly RewardKind Kind;
            public readonly int Amount;

            public PrizeData(RewardKind kind, int amount)
            {
                Kind = kind;
                Amount = amount;
            }
        }

        // Clockwise from the first segment to the right of the pointer.
        private static readonly PrizeData[] m_Prizes =
        {
            new PrizeData(RewardKind.Coins, 100),
            new PrizeData(RewardKind.Swap, 1),
            new PrizeData(RewardKind.Coins, 10),
            new PrizeData(RewardKind.Reshuffle, 1),
            new PrizeData(RewardKind.Coins, 5),
            new PrizeData(RewardKind.Break, 1),
            new PrizeData(RewardKind.Coins, 5),
            new PrizeData(RewardKind.Coins, 10)
        };

        private void Awake()
        {
            save = SaveLoadServices.FindForMenu(gameObject);
            progressText = transform.Find("ProgressCapsule/ProgressText").GetComponent<TMP_Text>();
            progressFill = (RectTransform)transform.Find("ProgressCapsule/Fill");
            spinText = transform.Find("SpinButton/SpinText").GetComponent<TMP_Text>();
            if (wheelDiscTransform == null)
            {
                var rotator = transform.Find("WheelArea/WheelRotator");
                if (rotator != null) wheelDiscTransform = rotator.GetComponent<RectTransform>();
            }
            if (spinButton == null)
            {
                spinButton = transform.Find("SpinButton")?.GetComponent<Button>();
            }
            if (closeButton == null)
            {
                closeButton = transform.Find("CloseButton")?.GetComponent<Button>();
            }
            if (luckyWheelCanvas == null)
            {
                luckyWheelCanvas = GetComponent<Canvas>();
            }
            if (subtitleText == null)
            {
                subtitleText = transform.Find("Subtitle")?.GetComponent<TextMeshProUGUI>();
            }
            if (prizeItemTransforms == null || prizeItemTransforms.Length == 0 || prizeItemTransforms[0] == null)
            {
                prizeItemTransforms = new RectTransform[m_Prizes.Length];
                for (int i = 0; i < prizeItemTransforms.Length; i++)
                    prizeItemTransforms[i] = wheelDiscTransform.Find($"DiscImage/Prize_{i}") as RectTransform;
            }

            if (spinButton != null)
            {
                spinButton.onClick.AddListener(OnSpinClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }
        }

        private void OnEnable()
        {
            save.WheelChanged += RefreshWheel;
            RefreshWheel();
        }

        private void RefreshWheel()
        {
            int wins = save.WheelWins;
            int spins = save.WheelSpins;
            progressText.text = $"{wins}/{GameData.WinsPerSpin}";
            spinText.text = $"SPIN ({spins})";
            var bar = (RectTransform)progressFill.parent;
            float width = Mathf.Max(0f, bar.rect.width - 2f * progressFill.anchoredPosition.x);
            progressFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * wins / GameData.WinsPerSpin);
            spinButton.interactable = !m_IsSpinning && spins > 0;
        }

        private void OnDestroy()
        {
            if (spinButton != null)
            {
                spinButton.onClick.RemoveListener(OnSpinClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseClicked);
            }
        }

        private void OnDisable()
        {
            save.WheelChanged -= RefreshWheel;
            if (m_SpinCoroutine != null)
            {
                StopCoroutine(m_SpinCoroutine);
                m_SpinCoroutine = null;
            }
            m_IsSpinning = false;
            if (celebratingPrize != null)
            {
                celebratingPrize.localScale = originalScale;
                celebratingPrize = null;
            }
        }

        public void OnSpinClicked()
        {
            if (m_IsSpinning) return;

            // Pick a random prize
            int targetPrizeIndex = Random.Range(0, m_Prizes.Length);
            SpinToPrize(targetPrizeIndex);
        }

        public void SpinToPrize(int prizeIndex)
        {
            if (m_IsSpinning || !isActiveAndEnabled || save.WheelSpins == 0 ||
                prizeIndex < 0 || prizeIndex >= m_Prizes.Length) return;

            if (m_SpinCoroutine != null)
            {
                StopCoroutine(m_SpinCoroutine);
            }

            m_SpinCoroutine = StartCoroutine(SpinRoutine(prizeIndex));
        }

        private IEnumerator SpinRoutine(int prizeIndex)
        {
            m_IsSpinning = true;
            if (spinButton != null)
            {
                spinButton.interactable = false;
            }

            try
            {
                float currentAngle = wheelDiscTransform.localEulerAngles.z;
                int fullTurns = Random.Range(minFullTurns, maxFullTurns + 1);
                
                float targetAngleInCircle = 22.5f + prizeIndex * 45f;

                float startAngleNorm = (currentAngle % 360f + 360f) % 360f;
                float degreesToTarget = (startAngleNorm - targetAngleInCircle);
                if (degreesToTarget <= 0) degreesToTarget += 360f;
                
                float totalDelta = fullTurns * 360f + degreesToTarget;
                float endAngle = currentAngle - totalDelta;

                float elapsed = 0f;

                while (elapsed < spinDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / spinDuration);
                    float ease = 1f - Mathf.Pow(1f - t, 5f);
                    float angle = Mathf.Lerp(currentAngle, endAngle, ease);

                    wheelDiscTransform.localEulerAngles = new Vector3(0f, 0f, angle);

                    if (elapsed < spinDuration) yield return null;
                }
                
                wheelDiscTransform.localEulerAngles = new Vector3(0f, 0f, endAngle);
                
                var prize = m_Prizes[prizeIndex];
                if (!save.TryCompleteWheelSpin(prize.Kind, prize.Amount)) yield break;

                if (prizeItemTransforms != null && prizeIndex >= 0 && prizeIndex < prizeItemTransforms.Length)
                {
                    var prizeTransform = prizeItemTransforms[prizeIndex];
                    if (prizeTransform != null)
                    {
                        yield return CelebrationPunchRoutine(prizeTransform);
                    }
                }


            }
            finally
            {
                m_IsSpinning = false;
                if (isActiveAndEnabled) RefreshWheel();
            }
        }
        
        private IEnumerator CelebrationPunchRoutine(RectTransform target)
        {
            Vector3 originalScale = Vector3.one;
            float punchDuration = 0.6f;
            float elapsed = 0f;

            while (elapsed < punchDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / punchDuration;
                float scaleMultiplier = 1f + Mathf.Sin(t * Mathf.PI * 2.5f) * (0.35f * (1f - t));
                target.localScale = originalScale * scaleMultiplier;
                yield return null;
            }

            target.localScale = originalScale;
            celebratingPrize = null;
        }

        public void OnCloseClicked()
        {
            if (luckyWheelCanvas != null)
            {
                luckyWheelCanvas.gameObject.SetActive(false);
            }
        }

        public void OpenWheel()
        {
            if (luckyWheelCanvas != null)
            {
                luckyWheelCanvas.gameObject.SetActive(true);
            }
        }
    }
}
