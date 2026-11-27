using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SlotLevelData
{
    #region ForTool
    public bool isActive = true;
    public SlotType slotType = SlotType.Nozmal;
    public LockType lockType = LockType.None;
    [Range(1, 3)] public int breakHitCount = 3;
    public int taskTargetScore = 50;
    public bool hasStack = false;
    #endregion
    
    public Vector2Int gridPosition;
    
    public List<Color> stackColors = new List<Color>();

    public SlotLevelData()
    {
        stackColors = new List<Color>();
    }

    public SlotLevelData(Vector2Int position, bool active = true)
    {
        gridPosition = position;
        isActive = active;
        slotType = SlotType.Nozmal;
        lockType = LockType.None;
        breakHitCount = 3;
        taskTargetScore = 50;
        hasStack = false;
        stackColors = new List<Color>();
    }
}
