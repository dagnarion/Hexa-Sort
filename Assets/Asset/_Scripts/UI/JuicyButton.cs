using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace HexaSort.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class JuicyButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [Header("Squish / Press Feel")]
        [SerializeField] private float pressScale = 0.92f;
        [SerializeField] private float pressDuration = 0.08f;
        [SerializeField] private float releaseDuration = 0.20f;

        [Header("Punch on Click")]
        [SerializeField] private bool useClickPunch = true;
        [SerializeField] private Vector3 punchScale = new Vector3(0.14f, -0.08f, 0f);
        [SerializeField] private float punchDuration = 0.30f;

        [Header("Idle Animations")]
        [SerializeField] private bool idlePulse = false;
        [SerializeField] private float pulseScale = 1.045f;
        [SerializeField] private float pulseDuration = 0.85f;

        [SerializeField] private bool idleFloat = false;
        [SerializeField] private float floatDistance = 6f;
        [SerializeField] private float floatDuration = 1.6f;

        [SerializeField] private bool idleWobble = false;
        [SerializeField] private float wobbleAngle = 8f;
        [SerializeField] private float wobbleInterval = 3f;

        [Header("Shine Sweep (e.g. Play Button)")]
        [SerializeField] private RectTransform shineBar;
        [SerializeField] private float shineInterval = 2.8f;
        [SerializeField] private float shineDuration = 0.65f;

        private RectTransform rectTransform;
        private Vector3 initialScale;
        private Tween idleTween;
        private Coroutine wobbleCoroutine;
        private Coroutine shineCoroutine;
        private bool isPressed = false;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            initialScale = rectTransform != null ? rectTransform.localScale : Vector3.one;
            if (initialScale == Vector3.zero) initialScale = Vector3.one;
        }

        private void Start()
        {
            StartIdleAnimations();
        }

        private void OnEnable()
        {
            StartIdleAnimations();
        }

        private void OnDisable()
        {
            StopAllTweens();
        }

        private void StartIdleAnimations()
        {
            StopAllTweens();

            if (idlePulse && rectTransform != null)
            {
                idleTween = rectTransform.DOScale(initialScale * pulseScale, pulseDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
            else if (idleFloat && rectTransform != null)
            {
                float initY = rectTransform.anchoredPosition.y;
                idleTween = rectTransform.DOAnchorPosY(initY + floatDistance, floatDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }

            if (idleWobble && gameObject.activeInHierarchy)
            {
                wobbleCoroutine = StartCoroutine(PeriodicWobbleRoutine());
            }

            if (shineBar != null && gameObject.activeInHierarchy)
            {
                shineCoroutine = StartCoroutine(PeriodicShineRoutine());
            }
        }

        private IEnumerator PeriodicWobbleRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(wobbleInterval + Random.Range(-0.4f, 0.4f));
                if (rectTransform != null && !isPressed)
                {
                    rectTransform.DOPunchRotation(new Vector3(0, 0, wobbleAngle), 0.5f, 10, 0.7f);
                }
            }
        }

        private IEnumerator PeriodicShineRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(shineInterval);
                if (shineBar != null && !isPressed)
                {
                    shineBar.anchoredPosition = new Vector2(-360, 0);
                    shineBar.DOAnchorPosX(360, shineDuration).SetEase(Ease.InOutQuad);
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;
            if (rectTransform == null) return;

            DOTween.Kill(rectTransform);

            // Scale down from center only - do NOT move position so pointer remains valid
            rectTransform.DOScale(initialScale * pressScale, pressDuration)
                .SetEase(Ease.OutQuad);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
            if (rectTransform == null) return;

            rectTransform.DOScale(initialScale, releaseDuration)
                .SetEase(Ease.OutBack);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            TriggerClickJuice();
        }

        public void TriggerClickJuice()
        {
            if (rectTransform == null) return;

            if (useClickPunch)
            {
                rectTransform.DOKill(true);
                rectTransform.localScale = initialScale;

                rectTransform.DOPunchScale(punchScale, punchDuration, 10, 0.7f)
                    .OnComplete(() =>
                    {
                        rectTransform.localScale = initialScale;
                        StartIdleAnimations();
                    });
            }
            else
            {
                rectTransform.DOScale(initialScale, releaseDuration)
                    .SetEase(Ease.OutBack)
                    .OnComplete(StartIdleAnimations);
            }
        }

        private void StopAllTweens()
        {
            if (rectTransform != null)
            {
                DOTween.Kill(rectTransform);
                rectTransform.localScale = initialScale;
            }

            if (idleTween != null)
            {
                idleTween.Kill();
                idleTween = null;
            }

            if (wobbleCoroutine != null)
            {
                StopCoroutine(wobbleCoroutine);
                wobbleCoroutine = null;
            }

            if (shineCoroutine != null)
            {
                StopCoroutine(shineCoroutine);
                shineCoroutine = null;
            }
        }

        private void OnDestroy()
        {
            StopAllTweens();
        }
    }
}
