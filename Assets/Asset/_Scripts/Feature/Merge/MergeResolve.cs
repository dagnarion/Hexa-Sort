using System.Collections.Generic;
using Cysharp.Threading.Tasks;


public class MergeResolve
{
    private MergeVisual mergeVisual;

    public MergeResolve(MergeVisual mergeVisual)
    {
        this.mergeVisual = mergeVisual;
    }
    
    public async UniTask Resolve(Slot slot)
    {
        HexagonStack hexagonStack = slot.GetHexagonStack();
        if(hexagonStack == null || hexagonStack.IsEmpty) return;
        List<Hexagon> hexagons = new List<Hexagon>();
        Hexagon topHexagon = hexagonStack.GetTopElement();
        hexagons.Add(topHexagon);
        for (int i = hexagonStack.GetNumberOfElement() - 2; i >= 0; i--)
        {
            if (hexagonStack.GetElement(i).ColorType == topHexagon.ColorType) hexagons.Add(hexagonStack.GetElement(i));
            else break;
        }
        if(hexagonStack.IsEmpty) return;
        if(hexagons.Count < 10) return;
        foreach (var it in hexagons)
        {
            it.SetParent(null);
            hexagonStack.RemoveElement(it);
        }
        await mergeVisual.ReleaseHexagon(hexagons);
        if (hexagonStack.IsEmpty)
        {
            slot?.ReleaseSlot();
            hexagonStack.transform.SetParent(null);
            hexagonStack.gameObject.SetActive(false);
        }
    }
}