using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TaskLockRender : MonoBehaviour
{
    [SerializeField] private Color lockColor;
    [SerializeField] private MeshRenderer render;
    [SerializeField] private SpriteRenderer icon;
    [SerializeField] private TextMeshPro label;
    [SerializeField] private LineRenderer ring;
    [SerializeField] private ParticleSystem stars;
    [SerializeField] private AudioSource sound;
    [SerializeField] private AudioClip click, pop, success;

    [Range(.4f, .7f)] public float unlockDuration = .62f;
    [Range(1f, 1.1f)] public float anticipationScale = 1.065f;
    [Range(1f, 1.2f)] public float popScale = 1.1f;
    [Range(0f, .1f)] public float shakeStrength = .035f;
    [Min(0f)] public float lockFlyHeight = .58f;
    [Range(-45f, 45f)] public float lockRotation = -18f;
    [Range(6, 12)] public int sparkleCount = 9;
    [Range(.3f, .6f)] public float sparkleLifetime = .33f;
    [Range(0f, .6f)] public float glowIntensity = .30f;
    [Range(.08f, .24f)] public float bounceDuration = .15f;
    [Range(0f, 1f)] public float unlockAudioVolume = .5f;

    private Sequence sequence;
    private Vector3 iconPosition, iconScale, topPosition;
    private Quaternion iconRotation;
    private Transform topVisual;
    private MaterialPropertyBlock tint;
    private Color plateColor;
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        iconPosition = icon.transform.localPosition;
        iconScale = icon.transform.localScale;
        iconRotation = icon.transform.localRotation;
        tint = new MaterialPropertyBlock();
        var particles = stars.main;
        particles.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
        for (int i = 0; i < ring.positionCount; i++)
        {
            float angle = 2f * Mathf.PI * i / ring.positionCount;
            ring.SetPosition(i, new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)));
        }
    }

    public void Lock(int target)
    {
        ResetRender();
        label.SetText("{0}", target);
        SetStack(null);
    }

    public void SetStack(HexagonStack stack)
    {
        var top = stack != null ? stack.GetTopElement() : null;
        topVisual = top != null ? top.render.transform : null;
        if (topVisual != null) topPosition = topVisual.localPosition;
        plateColor = top != null ? top.ColorType : lockColor;
        SetGlow(0f);
    }

    private void SetGlow(float amount)
    {
        tint.SetColor(BaseColor, Color.Lerp(plateColor, Color.white, amount));
        render.SetPropertyBlock(tint);
    }

    public void ResetRender()
    {
        sequence.Kill();
        sequence = null;
        if (topVisual != null) topVisual.localPosition = topPosition;
        topVisual = null;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        icon.transform.SetLocalPositionAndRotation(iconPosition, iconRotation);
        icon.transform.localScale = iconScale;
        label.transform.localScale = Vector3.one;
        ring.enabled = false;
        stars.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public async UniTask PlayUnlockEffect()
    {
        float t = unlockDuration;
        float trigger = t * .25f;
        var fx = sequence = DOTween.Sequence().SetLink(gameObject);
        stars.Play(false);
        Play(click);
        fx.Insert(0, transform.DOScale(anticipationScale, trigger).SetEase(Ease.OutBack));
        fx.Insert(0, transform.DOShakePosition(trigger, new Vector3(shakeStrength, 0, 0), 4));
        fx.Insert(0, DOVirtual.Float(0f, glowIntensity, trigger, SetGlow));
        fx.Insert(0, label.transform.DOScale(0, trigger).SetEase(Ease.InBack));
        fx.Insert(0, icon.transform.DOLocalRotateQuaternion(iconRotation * Quaternion.Euler(0, 0, lockRotation), trigger));
        fx.InsertCallback(trigger, () => { Play(pop); Emit(sparkleCount - 3, sparkleLifetime); });
        fx.Insert(trigger, icon.transform.DOLocalMoveY(iconPosition.y + lockFlyHeight, .38f*t).SetEase(Ease.OutQuad));
        fx.Insert(trigger, icon.transform.DOScale(iconScale * popScale, .10f*t));
        fx.Insert(.35f*t, icon.transform.DOScale(0, .30f*t).SetEase(Ease.InBack));
        fx.Insert(trigger, DOVirtual.Float(0, 1, .67f*t, DrawRing));
        fx.Insert(trigger, DOVirtual.Float(glowIntensity, 0, .4f*t, SetGlow));
        fx.Insert(.48f*t, transform.DOScale(0, .24f*t).SetEase(Ease.InBack));
        float bounce = Mathf.Min(bounceDuration, t * .33f);
        if (topVisual != null)
        {
            fx.Insert(t-bounce, topVisual.DOLocalMoveY(topPosition.y + .07f, bounce*.4f).SetEase(Ease.OutQuad));
            fx.Insert(t-bounce*.6f, topVisual.DOLocalMoveY(topPosition.y, bounce*.6f).SetEase(Ease.OutBack));
        }
        fx.InsertCallback(.69f*t, () => { Emit(3, .30f*t); Play(success); });
        fx.AppendInterval(Mathf.Max(0f, Mathf.Max(t, trigger + sparkleLifetime) - fx.Duration()));
        await fx.ToUniTask();
    }

    private void DrawRing(float progress)
    {
        ring.enabled = progress < 1f;
        ring.transform.localScale = Vector3.one * Mathf.Lerp(.24f, .96f, progress);
        var color = new Color(1f,.87f,.55f,.7f*(1-progress));
        ring.startColor = ring.endColor = color;
    }

    private void Emit(int count, float lifetime)
    {
        float offset = Random.value * 2f * Mathf.PI;
        for (int i = 0; i < count; i++)
        {
            float angle = offset + i * 2f * Mathf.PI / count;
            Vector3 radial = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            stars.Emit(new ParticleSystem.EmitParams {
                position = stars.transform.TransformPoint(radial * .44f + Vector3.up * .08f),
                velocity = stars.transform.TransformDirection(radial * Random.Range(.8f,1.3f) + Vector3.up * .55f),
                startSize = Random.Range(.32f,.48f), startLifetime = lifetime,
                startColor = Color.Lerp(new Color(1f,.78f,.38f), Color.white, Random.value)
            }, 1);
        }
    }

    private void Play(AudioClip clip)
    {
        if (clip != null) sound.PlayOneShot(clip, unlockAudioVolume);
    }
}
