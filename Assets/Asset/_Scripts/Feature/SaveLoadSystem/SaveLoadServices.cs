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
    public int WheelWins => CurrentData.WheelWins;
    public int WheelSpins => CurrentData.WheelSpins;
    public event Action WheelChanged;

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
        return GameData.MaxLives - data.MissingLives;
    }

    public void LoseLife()
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var data = CurrentData;
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
        if (++gameData.WheelWins >= GameData.WinsPerSpin)
        {
            gameData.WheelWins = 0;
            gameData.WheelSpins = AddReward(gameData.WheelSpins, 1);
        }
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
        if (Gold != previousGold) GoldChanged?.Invoke(Gold);
        WheelChanged?.Invoke();
        return rewardGranted;
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

    public bool TryCompleteWheelSpin(RewardKind kind, int amount)
    {
        var data = CurrentData;
        if (amount <= 0 || data.WheelSpins <= 0) return false;
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
        data.WheelSpins--;
        JsonSave();
        if (kind == RewardKind.Coins) GoldChanged?.Invoke(data.Gold);
        else if (saveLoadAbstract != null)
            foreach (var system in saveLoadAbstract) system?.RefreshInventory(data);
        WheelChanged?.Invoke();
        return true;
    }

    private static int AddReward(int balance, int amount) =>
        (int)Math.Min(int.MaxValue, (long)balance + amount);

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

    private void JsonSave()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PathFile));
            string pending = PathFile + ".tmp";
            File.WriteAllText(pending, JsonUtility.ToJson(gameData, true));
            if (File.Exists(PathFile)) File.Replace(pending, PathFile, null);
            else File.Move(pending, PathFile);
        }
        catch (Exception exception)
        {
            Debug.LogError($"[SaveLoadServices] Save file could not be written. {exception.Message}", this);
        }
    }
}
