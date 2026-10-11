using System;
using System.IO;
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
        foreach (var tmp in saveLoadAbstract)
        {
            tmp?.Save(gameData);
        }
        JsonSave();
    }

    public void Load()
    {
        if (File.Exists(PathFile))
        {
            string json = File.ReadAllText(PathFile);
            gameData = JsonUtility.FromJson<GameData>(json);
        }

        if (gameData == null)
        {
            gameData = new GameData();
            JsonSave();
        }

        foreach (var system in saveLoadAbstract)
        {
            system?.Load(gameData);
        }
    }

    private void JsonSave()
    {
        string dir = System.IO.Path.GetDirectoryName(PathFile);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        string json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(PathFile, json);
    }
}
