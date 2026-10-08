using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private EventChannel<GameData> GameDataEventChannel;
    [SerializeField] private EventChannel<LevelData> LevelLoadEventChannel;
    [SerializeField] private LevelData[] levelDatas;
    private Dictionary<int, LevelData> levels = new Dictionary<int, LevelData>();
    private void Awake()
    {
        for (int i = 0; i < levelDatas.Length; i++)
        {
            if(!levels.ContainsKey(i)) levels.Add(i,levelDatas[i]);
        }
    }

    private void OnEnable()
    {
        GameDataEventChannel.OnEventRaise += LoadLevel;
    }

    private void OnDisable()
    {
        GameDataEventChannel.OnEventRaise -= LoadLevel;
    }

    private void LoadLevel(GameData gameData)
    {
        if(!levels.ContainsKey(gameData.LevelID))
        {
            Debug.LogError("Level Not Found");
            return;
        }

        LevelData level = levels[gameData.LevelID];
        if (level.Level == null)
        {
            Debug.LogError("Level Not Found");
            return;
        }
        LevelLoadEventChannel?.Raise(level);
    }
    
    
    
}