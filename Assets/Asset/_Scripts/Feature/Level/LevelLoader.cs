using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : SaveLoadAbstract
{
    [SerializeField] private EventChannel<LevelData> LevelLoadEventChannel;
    [SerializeField] private LevelData[] levelDatas;
    private int currentLevelID = -1;
    private int currentLevelReward;
    public int CurrentLevelID => currentLevelID;
    public int CurrentLevelReward => currentLevelReward;
    public int LevelCount => levelDatas == null ? 0 : levelDatas.Length;
    private Dictionary<int, LevelData> levels = new Dictionary<int, LevelData>();
    private void Awake()
    {
        if (levelDatas == null) return;
        for (int i = 0; i < levelDatas.Length; i++)
        {
            if(!levels.ContainsKey(i)) levels.Add(i,levelDatas[i]);
        }
    }
    
    
    public override void Save(GameData gameData)
    {
        if (gameData == null || levelDatas == null || currentLevelID < 0 || currentLevelID >= levelDatas.Length) return;
        gameData.LevelID = currentLevelID;
    }

    public override void SaveCompletedLevel(GameData gameData)
    {
        if (gameData == null || levelDatas == null || currentLevelID < 0 || currentLevelID >= levelDatas.Length) return;
        gameData.LevelID = currentLevelID + 1 < levelDatas.Length ? currentLevelID + 1 : currentLevelID;
    }

    public override void Load(GameData gameData)
    {
        if (gameData == null || levels.Count == 0)
        {
            Debug.LogError("[LevelLoader] No level data is available to load.", this);
            return;
        }

        int requestedLevelID = gameData.LevelID;
        if (requestedLevelID < 0 || !levels.ContainsKey(requestedLevelID))
        {
            Debug.LogWarning($"[LevelLoader] Saved LevelID {requestedLevelID} is outside the configured range. Loading the first level.", this);
            requestedLevelID = 0;
            gameData.LevelID = requestedLevelID;
        }

        LevelData level = levels[requestedLevelID];
        if (level.Level == null)
        {
            Debug.LogError("Level Not Found");
            return;
        }

        currentLevelID = requestedLevelID;
        currentLevelReward = level.Reward;
        LevelLoadEventChannel?.Raise(level);
    }
}
