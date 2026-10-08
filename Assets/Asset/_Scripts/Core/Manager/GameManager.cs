using System;
using NaughtyAttributes;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform holder;
    [SerializeField] private ComponentPoolSO<Hexagon> hexagonPool;
    [SerializeField] private ComponentPoolSO<HexagonStack> hexagonStackPool;
    [SerializeField] private ComponentPoolSO<Slot> slotPool;
    [SerializeField] private SaveLoadServices saveLoadServices;
    private void Start()
    {
        hexagonPool.InitPool(holder);
        hexagonStackPool.InitPool(holder);
        slotPool.InitPool(holder);
        saveLoadServices.Load();
    }
    
}