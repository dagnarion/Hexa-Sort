using UnityEngine;

public abstract class SaveLoadAbstract : MonoBehaviour
{
    public abstract void Save(GameData gameData);
    public abstract void Load(GameData gameData);
}