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
        List<Hexagon> hexagons = new List<Hexagon>();
        HexagonStack stack = slot.GetHexagonStack();
        for (int i = stack.GetNumberOfElement() - 1; i >= 0; i--)
        {
            hexagons.Add(stack.GetElement(i));
        }

        await ReleaseHexagon(hexagons);

        for (int i = 0; i < stack.GetNumberOfElement(); i++)
        {
            stack.RemoveElement(i);
        }

        slot?.ReleaseSlot();
        stack.transform.SetParent(null);
        stack.gameObject.SetActive(false);
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