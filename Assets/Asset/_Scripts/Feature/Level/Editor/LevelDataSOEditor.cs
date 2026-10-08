#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelDataSO))]
public class LevelDataSOEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        LevelDataSO levelData = (LevelDataSO)target;
        if (levelData != null)
        {
            levelData.PruneAndSynchronizeSlots();
        }

        EditorGUILayout.Space(5);
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter
        };
        EditorGUILayout.LabelField($"Hexa Sort - {levelData.levelName}", titleStyle);
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Level Summary", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Level Number: {levelData.levelNumber}");
        EditorGUILayout.LabelField($"Grid Size: {levelData.gridSize.x} x {levelData.gridSize.y}");

        int activeSlots = levelData.GetActiveSlots().Count;
        int lockedSlots = levelData.slots.FindAll(s => s != null && s.isActive && s.lockType != LockType.None).Count;
        int stackedSlots = levelData.slots
            .FindAll(s => s != null && s.isActive && s.hasStack && s.stackColors.Count > 0).Count;
        int obstacleSlots = levelData.slots.FindAll(s => s != null && s.isActive && s.slotType == SlotType.Block).Count;

        EditorGUILayout.LabelField($"Active Slots: {activeSlots}");
        EditorGUILayout.LabelField($"Locked Slots: {lockedSlots}");
        EditorGUILayout.LabelField($"Obstacle (Block) Slots: {obstacleSlots}");
        EditorGUILayout.LabelField($"Pre-placed Stack Slots: {stackedSlots}");
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        GUI.backgroundColor = new Color(0.2f, 0.8f, 0.4f);
        if (GUILayout.Button("Open In Level Editor Window", GUILayout.Height(35)))
        {
            LevelEditorWindow.OpenWithLevel(levelData);
        }

        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(5);
        if (GUILayout.Button("Reset All Slots In Grid"))
        {
            if (EditorUtility.DisplayDialog("Reset Slots", "Bạn có chắc muốn reset toàn bộ slot về trạng thái ban đầu?",
                    "Đồng ý", "Hủy"))
            {
                Undo.RecordObject(levelData, "Reset All Slots");
                levelData.ResetSlots();
                EditorUtility.SetDirty(levelData);
                AssetDatabase.SaveAssets();
            }
        }


        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("Default Inspector", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck())
        {
            levelData.PruneAndSynchronizeSlots();
            EditorUtility.SetDirty(levelData);
        }
    }
}
#endif