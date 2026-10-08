using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : SaveLoadAbstract
{
    [SerializeField] private EventChannel<LevelData> LevelLoadEventChannel;
    [SerializeField] private LevelData[] levelDatas;
    private int CurrentLevelID;
    private Dictionary<int, LevelData> levels = new Dictionary<int, LevelData>();
    private void Awake()
    {
        for (int i = 0; i < levelDatas.Length; i++)
        {
            if(!levels.ContainsKey(i)) levels.Add(i,levelDatas[i]);
        }
    }
    
    
    public override void Save(GameData gameData)
    {
        if(CurrentLevelID >= levelDatas.Length)
        {
            gameData.LevelID = 0;
            return;
        }
        gameData.LevelID = CurrentLevelID + 1;
    }

    public override void Load(GameData gameData)
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

        CurrentLevelID = gameData.LevelID;
        LevelLoadEventChannel?.Raise(level);
    }
}