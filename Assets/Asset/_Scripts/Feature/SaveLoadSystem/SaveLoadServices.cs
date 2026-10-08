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
       string json = JsonUtility.ToJson(gameData,true); 
       File.WriteAllText(path,json);
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
       }

       foreach (var system in saveLoadAbstract)
       {
           system?.Load(gameData);
       }
   }
   
}
