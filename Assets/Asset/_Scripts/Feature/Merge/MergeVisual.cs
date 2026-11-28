using System.Collections.Generic;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MergeVisual
{
    private readonly StackReleaseBurstPool releaseBurstPool;

    public MergeVisual(StackReleaseBurstPool releaseBurstPool)
    {
        this.releaseBurstPool = releaseBurstPool;
    }

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
        int clearedBeforeBurst = Mathf.CeilToInt(hexagons.Count / 3f);
        // The list clears from the top down. Place the ring on the newly exposed face.
        Vector3 burstPosition = hexagons[Mathf.Min(clearedBeforeBurst, hexagons.Count - 1)].transform.position;
        burstPosition += Vector3.up * (HexagonStackArranger.GetLocalPosition(1).y * 0.5f);
        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < hexagons.Count; i++)
        {
            sequence.Insert(0.05f * i, hexagons[i].render.ReleaseHexagon(false));
            if (i + 1 == clearedBeforeBurst)
                sequence.AppendCallback(() => releaseBurstPool?.Play(burstPosition));
        }
        await sequence.ToUniTask();
        var last = hexagons[hexagons.Count - 1];
        await TaskGoalSpawner.FlyPickups(TaskGoalKind.CollectHexagons, last.transform.position,
            last.ColorType, hexagons.Count, last.render.CreditCollection);
        for (int i = 0; i < hexagons.Count; i++)
        {
            if (hexagons[i] != null)
                hexagons[i].ReleaseHexagon();
        }
    }
    
}
