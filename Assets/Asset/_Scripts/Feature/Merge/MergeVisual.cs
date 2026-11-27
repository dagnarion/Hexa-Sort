using System.Collections.Generic;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MergeVisual
{
    public async UniTask VisualHandler(List<Hexagon> hexagons, Transform targetStack, int firstLandingIndex)
    {
        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < hexagons.Count; i++)
        {
            Vector3 landingPosition = HexagonStackArranger.GetLocalPosition(firstLandingIndex + i);
            sequence.Insert(0.07f * i, hexagons[i].render.GotoTargetPosition(landingPosition, targetStack));
        }
        await sequence.ToUniTask();
    }

    public async UniTask ReleaseHexagon(List<Hexagon> hexagons)
    {
        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < hexagons.Count; i++)
        {
            sequence.Insert(0.05f * i, hexagons[i].render.ReleaseHexagon());
        }
        await sequence.ToUniTask();
        for (int i = 0; i < hexagons.Count; i++)
        {
            if (hexagons[i] != null)
                hexagons[i].ReleaseHexagon();
        }
    }
    
}
