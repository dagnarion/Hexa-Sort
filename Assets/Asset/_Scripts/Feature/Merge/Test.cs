using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;

public class Test : MonoBehaviour
{
    [SerializeField] private HexagonRender[] hexas;
    [SerializeField] private Transform target;
    [Button]
    public async UniTask MoveCoin()
    {
        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < hexas.Length; i++)
        {
            sequence.Insert(0.1f * i, hexas[i].GotoTargetPosition(target.position.With(y:target.position.y+(i+1)*.2f)));
        }
        await sequence.ToUniTask();
    }
    
}