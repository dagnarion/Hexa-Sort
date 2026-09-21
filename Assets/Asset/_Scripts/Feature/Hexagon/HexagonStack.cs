using System;
using System.Collections.Generic;
using UnityEngine;

public class HexagonStack : MonoBehaviour
{
    private List<Hexagon> hexagons = new List<Hexagon>();
    public bool IsEmpty => hexagons.Count <= 0;
    private Vector3 oldPosition;

    private void Start()
    {
        oldPosition = this.transform.position;
    }


    public void ReturnToOriginPosition() => this.transform.position = oldPosition;
    
    
    public void MoveToTargetPosition(Vector3 position)
    {
        this.transform.position = position;
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

    public void AddElement(Hexagon hex) => hexagons.Add(hex);
    
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