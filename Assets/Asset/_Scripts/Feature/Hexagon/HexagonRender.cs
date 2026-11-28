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
    private bool isReleasing;

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
        isReleasing = false;
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

    public Sequence GotoTargetPosition(Vector3 target, Transform destinationStack = null)
    {
        Sequence sequence = DOTween.Sequence();

        if (destinationStack != null)
            hexagonTransform.SetParent(destinationStack, true);
        Vector3 start = destinationStack != null ? hexagonTransform.localPosition : hexagonTransform.position;
        Vector3 middle = Vector3.Lerp(start, target, 0.5f);
        middle.y += JumpPower;

        Vector3[] path = { start, middle, target };
        Tween flight = destinationStack != null
            ? hexagonTransform.DOLocalPath(path, jumpDuration, PathType.CatmullRom)
            : hexagonTransform.DOPath(path, jumpDuration, PathType.CatmullRom);
        sequence.Append(flight.SetEase(Ease.InOutQuad));

        sequence.Join(
            transform.DOLocalRotate(
                new Vector3(0f, 0f, 180f),
                jumpDuration,
                RotateMode.LocalAxisAdd
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

    public void CreditCollection(int amount) => OnHexagonCollected?.Raise(amount);

    public Sequence ReleaseHexagon(bool countAsCollected = true)
    {
        if (isReleasing) return DOTween.Sequence();
        isReleasing = true;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(
            transform.DOScale(0f, disappearDuration).SetEase(Ease.InBack)
        );
        sequence.OnComplete(() =>
        {
            if (countAsCollected)
            {
                OnHexagonCollected?.Raise(1);
            }

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
