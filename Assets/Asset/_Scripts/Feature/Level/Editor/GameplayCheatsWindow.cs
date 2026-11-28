using UnityEditor;
using UnityEngine;

public sealed class GameplayCheatsWindow : EditorWindow
{
    [SerializeField] private int level = 1;
    [SerializeField] private int coins = 100;
    [SerializeField] private int boosters = 1;
    private string status;

    [MenuItem("Tools/Hexa Sort/Gameplay Cheats")]
    public static void Open() => GetWindow<GameplayCheatsWindow>("Gameplay Cheats");

    private void OnInspectorUpdate() => Repaint();

    private void OnGUI()
    {
        if (!EditorApplication.isPlaying || !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            status = null;
            EditorGUILayout.HelpBox("Mở scene GamePlay và vào Play Mode để dùng tool.", MessageType.Info);
            return;
        }

        var game = FindFirstObjectByType<GameManager>();
        if (game == null || game.StateMachine?.CurrentState == null)
        {
            EditorGUILayout.HelpBox("Chờ GameManager trong scene GamePlay.", MessageType.Info);
            return;
        }

        var save = game.SaveLoadServices;
        var loader = game.LevelLoader;
        EditorGUILayout.LabelField("Level", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Trạng thái", game.StateMachine.CurrentState.GetType().Name);
        EditorGUILayout.LabelField("Level hiện tại", loader.CurrentLevelID < 0 ? "Menu" : (loader.CurrentLevelID + 1).ToString());
        level = EditorGUILayout.IntField($"Đi tới level (1–{loader.LevelCount})", level);
        using (new EditorGUI.DisabledScope(!game.CanJumpToTestLevel || level < 1 || level > loader.LevelCount))
            if (GUILayout.Button("Vào level"))
                status = game.JumpToTestLevel(level) ? $"Đang tải level {level}." : "Không thể tải level.";

        using (new EditorGUI.DisabledScope(!game.CanForceResult))
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Thắng ngay"))
            {
                game.StateMachine.ChangeState(game.WinState);
                status = "Đã kích hoạt luồng thắng.";
            }
            if (GUILayout.Button("Thua ngay"))
            {
                game.StateMachine.ChangeState(game.LoseState);
                status = "Đã kích hoạt luồng thua.";
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Ví và vật phẩm", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Số dư", $"{save.Gold:N0} xu");
        coins = EditorGUILayout.IntField("Số xu thêm", coins);
        using (new EditorGUI.DisabledScope(coins <= 0))
            if (GUILayout.Button("Cộng xu"))
            {
                save.CreditGold(coins);
                status = $"Số dư hiện tại: {save.Gold:N0} xu.";
            }

        boosters = EditorGUILayout.IntField("Số booster thêm", boosters);
        using (new EditorGUI.DisabledScope(boosters <= 0))
        using (new EditorGUILayout.HorizontalScope())
        {
            BoosterButton(save, SaveLoadServices.RewardKind.Swap);
            BoosterButton(save, SaveLoadServices.RewardKind.Reshuffle);
            BoosterButton(save, SaveLoadServices.RewardKind.Break);
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Hồi đầy tim"))
            {
                save.RefillTestLives();
                status = "Đã hồi đầy tim.";
            }
            if (GUILayout.Button("+1 lượt Lucky Wheel"))
            {
                save.AddTestSpin();
                status = $"Lượt quay hiện tại: {save.WheelSpins}.";
            }
        }
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Các thay đổi được lưu vào hồ sơ đang chơi.", EditorStyles.wordWrappedLabel);
        if (!string.IsNullOrEmpty(status)) EditorGUILayout.HelpBox(status, MessageType.None);
    }

    private void BoosterButton(SaveLoadServices save, SaveLoadServices.RewardKind kind)
    {
        if (!GUILayout.Button(kind.ToString())) return;
        save.GrantTestBooster(kind, boosters);
        status = $"Đã cộng {boosters} {kind}.";
    }
}
