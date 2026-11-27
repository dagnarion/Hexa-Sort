
using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public const int MaxLives = 5;
    public const int LifeIntervalSeconds = 300;
    // Older profiles have zero missing lives and therefore start full.
    public int MissingLives;
    public long NextLifeUtcSeconds;

    public void RecoverLives(long now)
    {
        MissingLives = Math.Max(0, Math.Min(MaxLives, MissingLives));
        if (MissingLives == 0)
        {
            NextLifeUtcSeconds = 0;
            return;
        }
        if (NextLifeUtcSeconds <= 0 || NextLifeUtcSeconds > now + LifeIntervalSeconds)
            NextLifeUtcSeconds = now + LifeIntervalSeconds;
        if (now < NextLifeUtcSeconds) return;

        int recovered = (int)Math.Min(MissingLives, 1 + (now - NextLifeUtcSeconds) / LifeIntervalSeconds);
        MissingLives -= recovered;
        NextLifeUtcSeconds = MissingLives == 0 ? 0 : NextLifeUtcSeconds + recovered * LifeIntervalSeconds;
    }

    public int LevelID;
    public const int WinsPerSpin = 5;
    public int WheelWins;
    public int WheelSpins;
    public bool WheelProgressInitialized;
    public int Gold;
    public List<int> RewardedLevelIDs = new List<int>();
    public int BreakBoosterRemain;
    public int SwapBoosterRemain;
    public int ShuffleBoosterRemain;

    public void Normalize()
    {
        if (RewardedLevelIDs == null) RewardedLevelIDs = new List<int>();
        if (!WheelProgressInitialized)
        {
            // Previous saves only recorded the first completion of each level.
            WheelWins = RewardedLevelIDs.Count % WinsPerSpin;
            WheelSpins = RewardedLevelIDs.Count / WinsPerSpin;
            WheelProgressInitialized = true;
        }
        Gold = Math.Max(0, Gold);
        BreakBoosterRemain = Math.Max(0, BreakBoosterRemain);
        SwapBoosterRemain = Math.Max(0, SwapBoosterRemain);
        ShuffleBoosterRemain = Math.Max(0, ShuffleBoosterRemain);
    }
}
