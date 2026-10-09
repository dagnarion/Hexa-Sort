using System;
using System.IO;
using UnityEngine;

public class SaveLoadServices : MonoBehaviour
{
   [SerializeField] private SaveLoadAbstract[] saveLoadAbstract;
   private GameData gameData;
   private string path = Application.dataPath + "/Asset/_Scripts/Config/Json/gamedata.data";

   public void Save()
   {
       foreach (var tmp in saveLoadAbstract)
       {
           tmp?.Save(gameData);
       }
        JsonSave();;
   }

   public void Load()
   {
       if (File.Exists(path))
       {
           string json = File.ReadAllText(path);
           gameData = JsonUtility.FromJson<GameData>(json);
       }
       else
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
       string json = JsonUtility.ToJson(gameData,true); 
       File.WriteAllText(path,json);
   }
}
