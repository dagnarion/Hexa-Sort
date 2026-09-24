using System;
using System.Collections.Generic;
using UnityEngine;

public class HexagonStack : MonoBehaviour
{
   [SerializeField] private List<Hexagon> hexagons = new List<Hexagon>();
    private HexagonStackArranger stackArranger;
    public bool IsEmpty => hexagons.Count <= 0;
    private Vector3 oldPosition;

    private void Awake()
    {
        stackArranger = new HexagonStackArranger(this);
    }

    private void Start()
    {
        oldPosition = this.transform.position;
    }

    public void Init(Vector3 oldPosition)
    {
        this.oldPosition = oldPosition;
    }

    #region Position
    public Vector3 GetTopPosition() => stackArranger.GetTopPosition();
    
    public void ReturnToOriginPosition() => this.transform.position = oldPosition;
    
    public void MoveToTargetPosition(Vector3 position)
    {
        this.transform.position = position.With(y: transform.position.y);
    }

    public void DropToTargetPosition(Vector3 position)
    {
        this.transform.position = position.With(y: position.y + .2f);
    }
    #endregion

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