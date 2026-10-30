using UnityEngine;
using DG.Tweening;
public class SlideTo : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private float duration;
    private Sequence sequence;
    
    public void SlideToPosition(RectTransform position)
    {
        sequence.Kill();
        sequence.Append(rectTransform.DOAnchorPos(position.anchoredPosition, duration).SetEase(Ease.OutQuint));
    }
}
