using System;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

public class HexagonStackRender : MonoBehaviour
{
    [SerializeField] private int upHeight;
    [SerializeField] private float updownDuration;
    [SerializeField] private float lerpDuration;
    private HexagonStackArranger stackArranger;
    private Vector3 oldPosition;
    public static readonly Vector3 RestingLocalPosition = Vector3.up * 0.2f;
    private Vector3 RaisedLocalPosition => RestingLocalPosition + Vector3.up * upHeight;
    
    public void Init(HexagonStackArranger stackArranger)
    {
        this.stackArranger = stackArranger;
    }
    public void SetOriginPosition(Vector3 oldPosition)
    {
        this.oldPosition = oldPosition;
    }
    
    public Vector3 GetTopPosition() => stackArranger.GetTopPosition();
    
    public void ReturnToOriginPosition() => this.transform.position = oldPosition;
    
    public void MoveToTargetPosition(Vector3 position)
    {
        this.transform.position = position.With(y: transform.position.y);
    }

    public Tween MoveToRaisedSlot(Transform destinationSlot)
    {
        transform.SetParent(destinationSlot, true);
        return transform.DOLocalMove(RaisedLocalPosition, lerpDuration).SetEase(Ease.OutSine);
    }

    public Sequence Appear()
    {
        transform.DOKill();
        transform.localScale = Vector3.zero;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(
            transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBounce)
        );
        return sequence;
    }

    public Tween MoveUp()
    {
        transform.DOKill();
        return transform.DOLocalMove(RaisedLocalPosition, updownDuration).SetEase(Ease.OutSine);
    }

    public Tween MoveDown()
    {
        transform.DOKill();
        return transform.DOLocalMove(RestingLocalPosition, updownDuration).SetEase(Ease.OutSine);
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}
