using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform holder;
    [SerializeField] private ComponentPoolSO<Hexagon> hexagonPool;
    [SerializeField] private ComponentPoolSO<HexagonStack> hexagonStackPool;

    private void Start()
    {
        hexagonPool.InitPool(holder);
        hexagonStackPool.InitPool(holder);
    }
}