using System;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
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

    private void Awake()
    {
        baseScale = transform.localScale;
        hexagonTransform = this.transform.parent;
    }

    public void Init()
    {
        this.transform.localScale = baseScale;
    }

    public Color32 color
    {
        get { return render.material.color; }
        set { render.material.color = value; }
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
            transform.DOScale(0f,disappearDuration).SetEase(Ease.InBack)
        );
        sequence.OnComplete(() =>
        {
            OnHexagonCollected.Raise(1);
            hexagon.ReleaseHexagon();
        });
        return sequence;
    }
}