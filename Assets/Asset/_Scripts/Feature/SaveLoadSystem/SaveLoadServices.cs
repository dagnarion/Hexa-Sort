using System;
using System.IO;
using UnityEngine;

public class SaveLoadServices : MonoBehaviour
{
    [SerializeField] private SaveLoadAbstract[] saveLoadAbstract;
    private GameData gameData;
    private string _path;
    public event Action<int> GoldChanged;
    public int Gold => CurrentData.Gold;
    public int LevelNumber => CurrentData.LevelID + 1;
    public int WheelWins => CurrentData.WheelWins;
    public int WheelSpins => CurrentData.WheelSpins;
    public event Action WheelChanged;
    public int DailyRewardsInCycle => CurrentData.DailyRewardsInCycle;
    public bool CanClaimDailyReward => LocalDay > CurrentData.LastDailyRewardDay;
    private static long LocalDay => DateTime.Today.Ticks / TimeSpan.TicksPerDay;
    public long InfiniteLifeSeconds => Math.Max(0, CurrentData.InfiniteLivesUntilUtc - DateTimeOffset.UtcNow.ToUnixTimeSeconds());

    private GameData CurrentData => gameData ?? (gameData = ReadSave());

    public static SaveLoadServices FindForMenu(GameObject menu)
    {
        var service = FindFirstObjectByType<SaveLoadServices>(FindObjectsInactive.Include);
        return service != null ? service : menu.AddComponent<SaveLoadServices>();
    }

    public int GetLives(out int secondsRemaining)
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var data = CurrentData;
        int previousMissing = data.MissingLives;
        long previousDeadline = data.NextLifeUtcSeconds;
        data.RecoverLives(now);
        if (previousMissing != data.MissingLives || previousDeadline != data.NextLifeUtcSeconds)
            JsonSave();
        secondsRemaining = data.MissingLives == 0 ? 0 : (int)(data.NextLifeUtcSeconds - now);
        return data.InfiniteLivesUntilUtc > now ? GameData.MaxLives : GameData.MaxLives - data.MissingLives;
    }

    public void LoseLife()
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var data = CurrentData;
        if (data.InfiniteLivesUntilUtc > now) return;
        data.RecoverLives(now);
        if (data.MissingLives == 0) data.NextLifeUtcSeconds = now + GameData.LifeIntervalSeconds;
        data.MissingLives = Math.Min(GameData.MaxLives, data.MissingLives + 1);
        JsonSave();
    }

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
        var data = CurrentData;
        if (saveLoadAbstract != null)
        {
            foreach (var tmp in saveLoadAbstract) tmp?.Save(data);
        }
        JsonSave();
    }

    public bool CompleteLevelAndSave(int levelID, int reward)
    {
        if (levelID < 0) return false;

        int previousGold = Gold;
        var data = CurrentData.CopyForReward();
        if (++data.WheelWins >= GameData.WinsPerSpin)
        {
            data.WheelWins = 0;
            data.WheelSpins = AddReward(data.WheelSpins, 1);
        }
        data.Gold = AddReward(data.Gold, Math.Max(0, reward));

        if (saveLoadAbstract != null)
        {
            foreach (var system in saveLoadAbstract) system?.SaveCompletedLevel(data);
        }
        if (!WriteSave(data)) return false;
        gameData = data;
        if (Gold != previousGold) GoldChanged?.Invoke(Gold);
        WheelChanged?.Invoke();
        return true;
    }

    public void CreditGold(int amount)
    {
        if (amount <= 0) return;
        int balance = Gold;
        gameData.Gold = (int)Math.Min(int.MaxValue, (long)balance + amount);
        JsonSave();
        if (Gold != balance) GoldChanged?.Invoke(Gold);
    }

    public void SaveInventory(SaveLoadAbstract booster)
    {
        booster.Save(CurrentData);
        JsonSave();
    }

    public enum RewardKind { Coins, Swap, Reshuffle, Break }

    public bool TryClaimDailyReward()
    {
        long today = LocalDay;
        var current = CurrentData;
        if (today <= current.LastDailyRewardDay) return false;
        var data = current.CopyForReward();
        int day = data.DailyRewardsInCycle % 7 + 1;
        switch (day)
        {
            case 1:
            case 7:
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                data.InfiniteLivesUntilUtc = Math.Max(now, data.InfiniteLivesUntilUtc) + (day == 1 ? 1800 : 3600);
                break;
            case 2: ApplyReward(data, RewardKind.Break, 1); break;
            case 3: ApplyReward(data, RewardKind.Coins, 100); break;
            case 4: ApplyReward(data, RewardKind.Reshuffle, 1); break;
            case 5: ApplyReward(data, RewardKind.Coins, 200); break;
            case 6: ApplyReward(data, RewardKind.Swap, 1); break;
        }
        data.DailyRewardsInCycle = day;
        data.LastDailyRewardDay = today;
        if (!WriteSave(data)) return false;
        gameData = data;
        if (current.Gold != data.Gold) GoldChanged?.Invoke(data.Gold);
        if (day == 2 || day == 4 || day == 6) RefreshInventory(data);
        return true;
    }

    public bool TryCompleteWheelSpin(RewardKind kind, int amount)
    {
        var data = CurrentData;
        if (amount <= 0 || data.WheelSpins <= 0) return false;
        ApplyReward(data, kind, amount);
        data.WheelSpins--;
        JsonSave();
        if (kind == RewardKind.Coins) GoldChanged?.Invoke(data.Gold);
        else RefreshInventory(data);
        WheelChanged?.Invoke();
        return true;
    }

    private static void ApplyReward(GameData data, RewardKind kind, int amount)
    {
        switch (kind)
        {
            case RewardKind.Coins:
                data.Gold = AddReward(data.Gold, amount);
                break;
            case RewardKind.Swap:
                data.SwapBoosterRemain = AddReward(data.SwapBoosterRemain, amount);
                break;
            case RewardKind.Reshuffle:
                data.ShuffleBoosterRemain = AddReward(data.ShuffleBoosterRemain, amount);
                break;
            case RewardKind.Break:
                data.BreakBoosterRemain = AddReward(data.BreakBoosterRemain, amount);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }

    private void RefreshInventory(GameData data)
    {
        if (saveLoadAbstract != null)
            foreach (var system in saveLoadAbstract) system?.RefreshInventory(data);
    }

    private static int AddReward(int balance, int amount) =>
        (int)Math.Min(int.MaxValue, (long)balance + amount);

#if UNITY_EDITOR
    public bool SetTestLevel(int levelID)
    {
        var data = CurrentData;
        int previous = data.LevelID;
        data.LevelID = levelID;
        if (WriteSave(data)) return true;
        data.LevelID = previous;
        return false;
    }

    public void GrantTestBooster(RewardKind kind, int amount)
    {
        if (amount <= 0 || kind == RewardKind.Coins) return;
        var data = CurrentData;
        ApplyReward(data, kind, amount);
        JsonSave();
        RefreshInventory(data);
    }

    public void RefillTestLives()
    {
        var data = CurrentData;
        data.MissingLives = 0;
        data.NextLifeUtcSeconds = 0;
        JsonSave();
    }

    public void AddTestSpin()
    {
        CurrentData.WheelSpins = AddReward(CurrentData.WheelSpins, 1);
        JsonSave();
        WheelChanged?.Invoke();
    }
#endif

    private GameData ReadSave()
    {
        GameData data = null;
        if (File.Exists(PathFile))
        {
            try
            {
                string json = File.ReadAllText(PathFile);
                if (!string.IsNullOrWhiteSpace(json)) data = JsonUtility.FromJson<GameData>(json);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[SaveLoadServices] Save file could not be read. Starting from the first level. {exception.Message}", this);
            }
        }

        data ??= new GameData();
        data.Normalize();
        return data;
    }

    public void Load()
    {
        gameData = ReadSave();
        if (saveLoadAbstract != null)
        {
            foreach (var system in saveLoadAbstract) system?.Load(gameData);
        }
        JsonSave();
        GoldChanged?.Invoke(Gold);
        WheelChanged?.Invoke();
    }

    private void JsonSave() => WriteSave(gameData);

    private bool WriteSave(GameData data)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PathFile));
            string pending = PathFile + ".tmp";
            File.WriteAllText(pending, JsonUtility.ToJson(data, true));
            if (File.Exists(PathFile)) File.Replace(pending, PathFile, null);
            else File.Move(pending, PathFile);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"[SaveLoadServices] Save file could not be written. {exception.Message}", this);
            return false;
        }
    }
}
