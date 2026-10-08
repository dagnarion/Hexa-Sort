using System;
using UnityEngine;

public class ReshuffleBooster : SaveLoadAbstract
{
    private int remain;
    [SerializeField] private HexagonStackSpawner hexagonStackSpawner;
    
    public void Reshuffle()
    {
        if(remain <= 0) return;
        hexagonStackSpawner.Release();
        hexagonStackSpawner.Spawn();
        remain--;
    }

    public override void Save(GameData gameData)
    {
        gameData.ShuffleBoosterRemain = remain;
    }

    public override void Load(GameData gameData)
    {
        remain = gameData.ShuffleBoosterRemain;
    }
}
