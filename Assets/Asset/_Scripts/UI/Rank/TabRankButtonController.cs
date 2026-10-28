using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TabRankButtonController : MonoBehaviour
{
    [SerializeField] private Button currentButton;
    [SerializeField] private Button targetButton;
    [SerializeField] private GameObject selected;
    [SerializeField] private GameObject unselected;
    [SerializeField] private RectTransform showIcon;
    [SerializeField] private RectTransform hideIcon;
    [SerializeField] private Vector3 punchVel;
    [SerializeField] private float duration;
    [SerializeField] private int vibrato;
    [SerializeField] private float elasticity;
    private Sequence sequence;
    public void Show()
    {
        currentButton.interactable = false;
        targetButton.interactable = true;
        selected.gameObject.SetActive(true);
        unselected.gameObject.SetActive(false);
        currentButton.transform.SetAsLastSibling();
        sequence.Kill();
        sequence.Append(showIcon.DOPunchScale(punchVel, duration, vibrato, elasticity).SetEase(Ease.OutBack));
    }

    public void Hide()
    {
        currentButton.interactable = true;
        targetButton.interactable = false;
        selected.gameObject.SetActive(false);
        unselected.gameObject.SetActive(true);
        targetButton.transform.SetAsLastSibling();
        sequence.Kill();
        sequence.Append(hideIcon.DOPunchScale(punchVel, duration, vibrato, elasticity).SetEase(Ease.OutBack));
    }

}
