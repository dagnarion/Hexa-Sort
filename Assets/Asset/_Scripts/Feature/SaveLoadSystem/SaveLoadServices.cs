using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class SaveLoadServices : MonoBehaviour
{
    [SerializeField] private SaveLoadAbstract[] saveLoadAbstract;
    private GameData gameData;
    private string _path;

    private string PathFile
    {
        get
        {
            if (string.IsNullOrEmpty(_path))
            {
#if UNITY_EDITOR
                _path = Application.dataPath + "/Asset/_Scripts/Config/Json/gamedata.data";
#else
                _path = System.IO.Path.Combine(Application.persistentDataPath, "gamedata.data");
#endif
            }
            return _path;
        }
    }

    public void Save()
    {
        if (gameData == null) gameData = new GameData();
        gameData.Normalize();
        if (saveLoadAbstract != null)
        {
            foreach (var tmp in saveLoadAbstract) tmp?.Save(gameData);
        }
        JsonSave();
    }

    public bool CompleteLevelAndSave(int levelID, int reward)
    {
        if (gameData == null) gameData = new GameData();
        gameData.Normalize();
        if (levelID < 0) return false;

        bool rewardGranted = !gameData.RewardedLevelIDs.Contains(levelID);
        if (rewardGranted)
        {
            gameData.RewardedLevelIDs.Add(levelID);
            gameData.Gold = (int)Math.Min(int.MaxValue, (long)gameData.Gold + Math.Max(0, reward));
        }

        if (saveLoadAbstract != null)
        {
            foreach (var system in saveLoadAbstract) system?.SaveCompletedLevel(gameData);
        }
        JsonSave();
        return rewardGranted;
    }

    public void Load()
    {
        gameData = null;
        if (File.Exists(PathFile))
        {
            try
            {
                string json = File.ReadAllText(PathFile);
                if (!string.IsNullOrWhiteSpace(json)) gameData = JsonUtility.FromJson<GameData>(json);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[SaveLoadServices] Save file could not be read. Starting from the first level. {exception.Message}", this);
            }
        }

        if (gameData == null)
        {
            gameData = new GameData();
        }

        gameData.Normalize();

        if (saveLoadAbstract != null)
        {
            foreach (var system in saveLoadAbstract) system?.Load(gameData);
        }
        JsonSave();
    }

    private void JsonSave()
    {
        string dir = System.IO.Path.GetDirectoryName(PathFile);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        try
        {
            string json = JsonUtility.ToJson(gameData, true);
            File.WriteAllText(PathFile, json);
        }
        catch (Exception exception)
        {
            Debug.LogError($"[SaveLoadServices] Save file could not be written. {exception.Message}", this);
        }
    }
}
