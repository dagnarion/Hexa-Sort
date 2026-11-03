
using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public int LevelID;
    public int Gold;
    public List<int> RewardedLevelIDs = new List<int>();
    public int BreakBoosterRemain;
    public int SwapBoosterRemain;
    public int ShuffleBoosterRemain;

    public void Normalize()
    {
        if (RewardedLevelIDs == null) RewardedLevelIDs = new List<int>();
        Gold = Math.Max(0, Gold);
        BreakBoosterRemain = Math.Max(0, BreakBoosterRemain);
        SwapBoosterRemain = Math.Max(0, SwapBoosterRemain);
        ShuffleBoosterRemain = Math.Max(0, ShuffleBoosterRemain);
    }
}
