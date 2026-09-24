using System.Collections.Generic;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MergeVisual
{
    public async UniTask VisualHandler(List<Hexagon> hexagon,Vector3 target)
    {
        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < hexagon.Count; i++)
        {
            sequence.Insert(0.1f * i, hexagon[i].render.GotoTargetPosition(target.With(y:target.y+i*.2f)));
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
    }
    
}