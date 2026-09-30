using System;
using System.Collections.Generic;
using UnityEngine;

public class HexagonStack : MonoBehaviour
{
   [SerializeField] private List<Hexagon> hexagons = new List<Hexagon>();
   [field:SerializeField] public HexagonStackRender Render { get; private set; }
    private HexagonStackArranger stackArranger;
    public bool IsEmpty => hexagons.Count <= 0;

    private void Awake()
    {
        stackArranger = new HexagonStackArranger(this);
        Render.Init(stackArranger);
        Render.SetOriginPosition(this.transform.position);
    }

    public void DisableAllCollider()
    {
        foreach (Hexagon hexa in hexagons)
        {
            hexa.UnSelect();
        }
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