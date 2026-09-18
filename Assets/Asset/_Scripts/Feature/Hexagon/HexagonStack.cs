using System.Collections.Generic;

public class HexagonStack
{
    private List<Hexagon> hexagons;
    public bool IsEmpty => hexagons.Count <= 0;
        
    public HexagonStack()
    {
        hexagons = new List<Hexagon>();
    }
        
    public Hexagon GetElement(int index)
    {
        if (IsEmpty || index >= hexagons.Count) return null;
        return hexagons[index];
    }

    public Hexagon GetTopElement()
    {
        if (hexagons.Count == 0) return null;
        return hexagons[^1];
    }

    public void AddElement(Hexagon hex)
    {
        hexagons.Add(hex);
    }

    public void RemoveElement(int index)
    {
        if (IsEmpty || index >= hexagons.Count) return;
        hexagons.RemoveAt(index);
    }

    public void RemoveElement(Hexagon hexagonData)
    {
        if (!hexagons.Contains(hexagonData)) return;
        hexagons.Remove(hexagonData);
    }

    public int GetNumberOfElement() => hexagons.Count;
}