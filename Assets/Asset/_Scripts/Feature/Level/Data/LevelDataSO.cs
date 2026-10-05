using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Level_01", menuName = "Level/LevelDataSO")]
public class LevelDataSO : ScriptableObject
{
    public int levelNumber = 1;
    public string levelName = "Level 1";
    public Vector2Int gridSize = new Vector2Int(5, 5);
    public GridFormLayout formLayout = GridFormLayout.FlatTopped;
    public List<SlotLevelData> slots = new List<SlotLevelData>();
}