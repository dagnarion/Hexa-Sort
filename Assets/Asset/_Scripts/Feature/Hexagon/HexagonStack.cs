using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HexagonStack : MonoBehaviour
{
    [SerializeField] private List<Hexagon> hexagons = new List<Hexagon>();
    [field:SerializeField] public HexagonStackRender Render { get; private set; }
    private HexagonStackArranger stackArranger;
    private ComponentPoolSO<HexagonStack> pool;
    public bool IsEmpty => hexagons.Count <= 0;

    public void SetPool(ComponentPoolSO<HexagonStack> stackPool) => pool = stackPool;

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
            if (hexa != null) hexa.UnSelect();
        }
    }

    public Hexagon GetElement(int index)
    {
        if (IsEmpty || index < 0 || index >= hexagons.Count) return null;
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
        if (IsEmpty || index < 0 || index >= hexagons.Count) return;
        hexagons.RemoveAt(index);
    }

    public void RemoveElement(Hexagon hexagonData)
    {
        if (!hexagons.Contains(hexagonData)) return;
        hexagons.Remove(hexagonData);
    }

    public void Clear()
    {
        hexagons.Clear();
    }

    public void Release()
    {
        transform.DOKill();
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        hexagons.Clear();
        if (pool != null)
        {
            pool.Release(this);
        }
        else
        {
            transform.SetParent(null);
            gameObject.SetActive(false);
        }
    }

    public int GetNumberOfElement() => hexagons.Count;
}