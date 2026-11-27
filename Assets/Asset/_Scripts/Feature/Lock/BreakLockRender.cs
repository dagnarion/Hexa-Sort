using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class BreakLockRender : MonoBehaviour
{
    [Header("Wooden barrier")]
    [SerializeField] private Transform upperPlank;
    [SerializeField] private Transform lowerPlank;
    [SerializeField] private ParticleSystem woodChips;
    [SerializeField] private ParticleSystem dustAndRivets;

    [Header("Impact")]
    [Range(0.8f, 1f)] public float impactScale = 0.94f;
    [Min(0.01f)] public float impactDuration = 0.15f;
    [Min(0f)] public float shakeStrength = 0.055f;
    [Min(0.01f)] public float shakeDuration = 0.1f;

    [Header("Break")]
    [Min(0f)] public float plankFlyDistance = 0.52f;
    [Range(0f, 70f)] public float plankRotation = 36f;
    [Range(0, 8)] public int debrisCount = 6;
    [Range(0.1f, 0.7f)] public float debrisLifetime = 0.45f;
    [Range(0, 10)] public int dustParticleCount = 6;
    [Range(0.2f, 0.6f)] public float breakAnimationDuration = 0.36f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitClip, crackClip, breakClip;
    [Range(0f, 1f)] public float audioVolume = 0.65f;

    private struct PlankPose
    {
        public Transform plank;
        public Vector3 position, scale;
        public Quaternion rotation;

        public PlankPose(Transform plank)
        {
            this.plank = plank;
            position = plank.localPosition;
            rotation = plank.localRotation;
            scale = plank.localScale;
        }

        public void Restore()
        {
            plank.SetLocalPositionAndRotation(position, rotation);
            plank.localScale = scale;
            plank.gameObject.SetActive(true);
        }
    }

    private PlankPose upper, lower;
    private int plankCount;
    private Sequence impact, upperFlight, lowerFlight, removal;

    private void Awake()
    {
        upper = new PlankPose(upperPlank);
        lower = new PlankPose(lowerPlank);
    }

    public void Init(int planks = 2)
    {
        impact.Kill();
        upperFlight.Kill();
        lowerFlight.Kill();
        removal.Kill();
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
        upper.Restore();
        lower.Restore();
        plankCount = planks;
        upperPlank.gameObject.SetActive(planks == 2);
        lowerPlank.gameObject.SetActive(planks >= 1);
        woodChips.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        dustAndRivets.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void Impact()
    {
        impact.Kill();
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        impact = DOTween.Sequence().SetLink(gameObject);
        impact.Append(transform.DOScale(new Vector3(1.035f, impactScale, 1.035f), impactDuration * .3f));
        impact.Append(transform.DOScale(new Vector3(.99f, 1.04f, .99f), impactDuration * .35f));
        impact.Append(transform.DOScale(1f, impactDuration * .35f));
        impact.Insert(0f, transform.DOShakePosition(shakeDuration, new Vector3(shakeStrength, 0f, shakeStrength * .45f), 5));
        Play(hitClip);
    }

    public async UniTask OnHit(int remainingPlanks)
    {
        Impact();
        Play(crackClip);
        if (plankCount == 2 && remainingPlanks < 2)
            upperFlight = Fly(upper, -1f);
        if (plankCount >= 1 && remainingPlanks == 0)
            lowerFlight = Fly(lower, 1f);
        plankCount = remainingPlanks;
        Burst(false);
        await impact.ToUniTask();
    }

    public async UniTask PlayUnlockEffect()
    {
        Impact();
        if (plankCount == 2) upperFlight = Fly(upper, -1f);
        if (plankCount >= 1) lowerFlight = Fly(lower, 1f);
        Burst(plankCount > 0);
        Play(breakClip);
        removal = DOTween.Sequence().SetLink(gameObject);
        removal.Insert(Mathf.Max(impact.Duration(), breakAnimationDuration * .75f),
            transform.DOScale(0f, .14f).SetEase(Ease.InBack));
        // Let world-space chips finish before this lock returns to its pool.
        removal.AppendInterval(Mathf.Max(0f, debrisLifetime - removal.Duration()));
        await removal.ToUniTask();
    }

    private Sequence Fly(PlankPose pose, float side)
    {
        Transform plank = pose.plank;
        Vector3 lateral = transform.right * side + transform.forward * Random.Range(-.35f, .35f);
        Vector3 travel = plank.parent.InverseTransformVector(lateral * plankFlyDistance);
        Vector3 lift = plank.parent.InverseTransformVector(Vector3.up * .48f);
        float t = breakAnimationDuration;
        var flight = DOTween.Sequence().SetLink(gameObject);
        flight.Append(plank.DOLocalMove(pose.position + travel * .55f + lift, t * .45f).SetEase(Ease.OutQuad));
        flight.Append(plank.DOLocalMove(pose.position + travel, t * .55f).SetEase(Ease.InQuad));
        flight.Insert(0, plank.DOLocalRotateQuaternion(pose.rotation * Quaternion.Euler(plankRotation * side, 0f, side * 12f), t));
        flight.Insert(t * .57f, plank.DOScale(0f, t * .43f).SetEase(Ease.InQuad));
        return flight;
    }

    private void Burst(bool final)
    {
        Vector3 center = lowerPlank.position + Vector3.up * .09f;
        for (int i = 0; i < debrisCount; i++)
            Emit(woodChips, center, Random.Range(.17f, .34f), Color.white, 1f);
        for (int i = 0; i < dustParticleCount; i++)
            Emit(dustAndRivets, center, Random.Range(.09f, .16f), new Color(.91f,.71f,.47f,.65f), .65f);
        if (final)
            for (int side = -1; side <= 1; side += 2)
                Emit(dustAndRivets, lowerPlank.TransformPoint(Vector3.right * (.62f * side)), .09f,
                    new Color(.42f,.40f,.50f), 1.15f);
    }

    private void Emit(ParticleSystem system, Vector3 center, float size, Color color, float speed)
    {
        Vector2 spread = Random.insideUnitCircle;
        var particle = new ParticleSystem.EmitParams {
            position = center + new Vector3(spread.x, 0f, spread.y) * .12f,
            velocity = new Vector3(spread.x * 2f, Random.Range(1.3f, 2.4f), spread.y * 2f) * speed,
            startSize = size, startColor = color, rotation = Random.Range(0f, 360f),
            startLifetime = debrisLifetime * Random.Range(.75f, 1f)
        };
        system.Emit(particle, 1);
    }

    private void Play(AudioClip clip)
    {
        if (!clip || !audioSource) return;
        audioSource.pitch = Random.Range(.96f, 1.04f);
        audioSource.PlayOneShot(clip, audioVolume);
    }
}
