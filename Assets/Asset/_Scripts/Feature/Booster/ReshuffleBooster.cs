using System;
using UnityEngine;

public class ReshuffleBooster : MonoBehaviour
{
    private int remain;
    [SerializeField] private HexagonStackSpawner hexagonStackSpawner;
    [SerializeField] private EventChannel<GameData> GameDataEventChannel;

    private void OnEnable()
    {
        GameDataEventChannel.OnEventRaise += Init;
    }

    private void OnDisable()
    {
        GameDataEventChannel.OnEventRaise -= Init;
    }

    private void Init(GameData data)
    {
        remain = data.ShuffleBoosterRemain;
    }
    
    public void Reshuffle()
    {
        if(remain <= 0) return;
        hexagonStackSpawner.Release();
        hexagonStackSpawner.Spawn();
        remain--;
    }
}
