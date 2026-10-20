using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        private float m_LastTickAngle = 0f;
        private Coroutine m_SpinCoroutine;

        [System.Serializable]
        public struct PrizeData
        {
            public string name;
            public int amount;
            public int segmentIndex; // 0 to 7
        }

        private readonly PrizeData[] m_Prizes = new PrizeData[]
        {
            new PrizeData { name = "Coins", amount = 100, segmentIndex = 0 },      // Yellow (Top)
            new PrizeData { name = "Hand Booster", amount = 1, segmentIndex = 1 }, // Cyan (Top-Right)
            new PrizeData { name = "Coins", amount = 10, segmentIndex = 2 },       // Blue (Right)
            new PrizeData { name = "Swap Booster", amount = 1, segmentIndex = 3 }, // Violet (Bottom-Right)
            new PrizeData { name = "Coins", amount = 5, segmentIndex = 4 },        // Purple (Bottom)
            new PrizeData { name = "Hammer Booster", amount = 1, segmentIndex = 5 },// Magenta (Bottom-Left)
            new PrizeData { name = "Coins", amount = 5, segmentIndex = 6 },        // Hot Pink (Left)
            new PrizeData { name = "Coins", amount = 10, segmentIndex = 7 }       // Coral/Orange (Top-Left)
        };

        private void Awake()
        {
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
                var list = new System.Collections.Generic.List<RectTransform>();
                for (int i = 0; i < 8; i++)
                {
                    var p = transform.Find($"WheelArea/WheelRotator/Prize_{i}");
                    if (p != null) list.Add(p.GetComponent<RectTransform>());
                }
                prizeItemTransforms = list.ToArray();
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
            if (m_SpinCoroutine != null)
            {
                StopCoroutine(m_SpinCoroutine);
                m_SpinCoroutine = null;
            }
            m_IsSpinning = false;
            if (spinButton != null)
            {
                spinButton.interactable = true;
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
            if (m_IsSpinning) return;

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

                m_LastTickAngle = currentAngle;
                float elapsed = 0f;

                while (elapsed < spinDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / spinDuration);
                    float ease = 1f - Mathf.Pow(1f - t, 5f);
                    float angle = Mathf.Lerp(currentAngle, endAngle, ease);

                    wheelDiscTransform.localEulerAngles = new Vector3(0f, 0f, angle);

                    yield return null;
                }
                
                wheelDiscTransform.localEulerAngles = new Vector3(0f, 0f, endAngle);
                
                if (prizeItemTransforms != null && prizeIndex >= 0 && prizeIndex < prizeItemTransforms.Length)
                {
                    var prizeTransform = prizeItemTransforms[prizeIndex];
                    if (prizeTransform != null)
                    {
                        yield return CelebrationPunchRoutine(prizeTransform);
                    }
                }

                var prize = m_Prizes[prizeIndex];
                Debug.Log($"<color=yellow><b>[LuckyWheel]</b> Congratulations! You won <b>{prize.amount}x {prize.name}</b>!</color>");
            }
            finally
            {
                m_IsSpinning = false;
                if (spinButton != null)
                {
                    spinButton.interactable = true;
                }
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
