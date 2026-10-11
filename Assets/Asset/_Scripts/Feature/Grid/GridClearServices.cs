using System.Collections.Generic;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GridClearServices : MonoBehaviour
{
    [SerializeField] private GridController gridController;
    List<UniTask> uni = new List<UniTask>();

    public async UniTask Release()
    {
        if (gridController == null || gridController.grid == null) return;
        uni.Clear();
        gridController.grid.GridTraversal((pos, slot) =>
            {
                uni.Add(ReleaseASlot(slot));
            }
        );
        await UniTask.WhenAll(uni);
    }

    private async UniTask ReleaseASlot(Slot slot)
    {
        if (slot == null || slot.IsEmpty || slot.IsLocked) return;
        HexagonStack stack = slot.GetHexagonStack();
        if (stack == null) return;
        slot.ReleaseSlot();

        List<Hexagon> hexagons = new List<Hexagon>();
        for (int i = stack.GetNumberOfElement() - 1; i >= 0; i--)
        {
            Hexagon hexa = stack.GetElement(i);
            if (hexa != null)
            {
                hexa.SetParent(slot.transform.parent);
                hexagons.Add(hexa);
            }
        }
        stack.Clear();
        stack.Release();

        await ReleaseHexagon(hexagons);
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