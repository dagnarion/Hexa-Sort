using System;
using UnityEngine;
using DG.Tweening;
using Sequence = DG.Tweening.Sequence;

public class HexagonRender : MonoBehaviour
{
    [SerializeField] private Hexagon hexagon;
    [SerializeField] private MeshRenderer render;
    [SerializeField] private EventChannel<int> OnHexagonCollected;
    [SerializeField] private float JumpPower;
    [SerializeField] private float jumpDuration;
    [SerializeField] private float disappearDuration;
    private Vector3 baseScale;
    private Transform hexagonTransform;
    private Material materialInstance;

    private void Awake()
    {
        baseScale = transform.localScale;
        hexagonTransform = this.transform.parent;
        if (render != null)
        {
            materialInstance = render.material;
        }
    }

    public void Init()
    {
        this.transform.localScale = baseScale;
        hexagonTransform.localScale = Vector3.one;
        hexagonTransform.localRotation = Quaternion.identity;
        this.transform.rotation = Quaternion.Euler(0f, 30f, 0f);
    }

    public Color32 color
    {
        get
        {
            if (materialInstance == null && render != null)
                materialInstance = render.material;
            return materialInstance != null ? (Color32)materialInstance.color : default;
        }
        set
        {
            if (materialInstance == null && render != null)
                materialInstance = render.material;
            if (materialInstance != null)
                materialInstance.color = value;
        }
    }

    public void SetPosition(Vector3 pos)
    {
        hexagonTransform.position = pos;
    }

    public Sequence GotoTargetPosition(Vector3 target)
    {
        Sequence sequence = DOTween.Sequence();

        Vector3 start = hexagonTransform.position;
        Vector3 middle = Vector3.Lerp(start, target, 0.5f);
        middle.y += JumpPower;

        sequence.Append(
            hexagonTransform.DOPath(
                new[] { start, middle, target },
                jumpDuration,
                PathType.CatmullRom
            ).SetEase(Ease.InOutQuad)
        );

        sequence.Join(
            transform.DORotate(
                new Vector3(0f, 0f, 180f),
                jumpDuration,
                RotateMode.WorldAxisAdd
            ).SetEase(Ease.Linear)
        );
        sequence.Append(
            transform.DOPunchScale(
                Vector3.one * 0.15f,
                0.15f
            )
        );

        return sequence;
    }

    public Sequence ReleaseHexagon()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(
            transform.DOScale(0f, disappearDuration).SetEase(Ease.InBack)
        );
        sequence.OnComplete(() =>
        {
            OnHexagonCollected.Raise(1);
            hexagon.gameObject.SetActive(false);
        });
        return sequence;
    }

    private void OnDestroy()
    {
        transform.DOKill();
        if (hexagonTransform != null)
            hexagonTransform.DOKill();
        if (materialInstance != null)
        {
            Destroy(materialInstance);
            materialInstance = null;
        }
    }
}