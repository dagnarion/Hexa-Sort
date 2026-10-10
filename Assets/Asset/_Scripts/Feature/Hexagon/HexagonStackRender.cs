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
    private Vector3 targetUpPosition;
    private Vector3 targetDownPosition;
    
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

    public Sequence LerpToTargetPosition(Vector3 position)
    {
        Sequence sq = DOTween.Sequence();
        sq.Append(transform.DOMove(position, lerpDuration).SetEase(Ease.OutSine));
        return sq;
    }

    public void DropToTargetPosition(Vector3 position)
    {
        this.transform.position = position.With(y: position.y + .2f);
        SetUpAndDownPosition(position);
    }

    public void SetUpAndDownPosition(Vector3 position)
    {
        targetUpPosition = position + new Vector3(0, upHeight, 0);
        targetDownPosition = position;
    }
    public Sequence Appear()
    {
        transform.DOKill();
        transform.localScale = Vector3.zero;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(
            transform.DOScale(Vector3.one,0.4f).SetEase(Ease.OutBounce)
        );
        return sequence;
    }
    
    public Sequence MoveUp()
    {
        Sequence sq = DOTween.Sequence();
        transform.DOKill();
        sq.Append(transform.DOMove(targetUpPosition, updownDuration).SetEase(Ease.OutSine));
        return sq;
    }

    public Sequence MoveDown()
    {
        Sequence sq = DOTween.Sequence();
        transform.DOKill();
        sq.Append(transform.DOMove(targetDownPosition, updownDuration).SetEase(Ease.OutSine));
        return sq;
    }
}