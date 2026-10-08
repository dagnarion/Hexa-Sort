using System;
using NaughtyAttributes;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform holder;
    [SerializeField] private ComponentPoolSO<Hexagon> hexagonPool;
    [SerializeField] private ComponentPoolSO<HexagonStack> hexagonStackPool;
    [SerializeField] private ComponentPoolSO<Slot> slotPool;
    [SerializeField] private EventChannel<GameData> GameDataEventChannel;
    [SerializeField] private GameData gameData;
    private void Start()
    {
        hexagonPool.InitPool(holder);
        hexagonStackPool.InitPool(holder);
        slotPool.InitPool(holder);
    }

    [Button]
    private void LoadLevel()
    {
        GameDataEventChannel.Raise(gameData);
    }
    
}