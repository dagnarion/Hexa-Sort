#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public enum EditorToolMode
{
    Select = 0,
    ToggleSlot = 1,
    PaintActive = 2,
    PaintInactive = 3,
    PaintLock = 4,
    PaintStack = 5,
    Eraser = 6
}

public class LevelEditorWindow : EditorWindow
{
    [SerializeField] private LevelDataSO currentLevel;
    private SerializedObject serializedLevel;
    
    private EditorToolMode currentTool = EditorToolMode.Select;
    private Vector2Int selectedSlotPos = new Vector2Int(0, 0);
    private bool hasSelection = false;

    private LockType brushLockType = LockType.BreakLock;
    private int brushBreakHits = 2;
    private int brushTaskTarget = 50;
    private SlotLevelData clipboardSlotData = null;

    private Vector2 gridScrollPos;
    private Vector2 inspectorScrollPos;
    private float cellDisplaySize = 46f;
    private bool showSceneHandles = true;
    private bool show3DScenePreview = true;
    private bool showGridLabels = true;
    
    private static readonly Color[] PresetColors = new Color[]
    {
        new Color(1f, 0f, 0f), // Red
        new Color(0f, 0.2f, 1f), // Blue
        new Color(1f, 0.85f, 0f), // Yellow
        new Color(0.1f, 0.8f, 0.2f), // Green
        new Color(1f, 0.5f, 0f), // Orange
        new Color(0.7f, 0.1f, 0.9f), // Purple
        new Color(0f, 0.85f, 0.9f), // Cyan
        new Color(1f, 0.4f, 0.7f), // Pink
        new Color(0.95f, 0.95f, 0.95f), // White
        new Color(0.3f, 0.3f, 0.3f) // Gray
    };

    private Color brushColor = new Color(1f, 0f, 0f);
    private Vector2Int lastDraggedCell = new Vector2Int(-1, -1);
    private int batchAddCount = 5;
    private Color batchAddColor = new Color(1f, 0f, 0f);

    [MenuItem("Tools/Hexa Sort/Level Editor")]
    public static void OpenWindow()
    {
        var window = GetWindow<LevelEditorWindow>("Hexa Sort Level Editor");
        window.minSize = new Vector2(900, 600);
        window.Show();
    }

    [MenuItem("Tools/Hexa Sort/Create Sample Level")]
    public static void CreateSampleLevelMenu()
    {
        string dir = "Assets/Asset/_Scripts/Config/SO/Levels";
        if (!System.IO.Directory.Exists(dir))
        {
            System.IO.Directory.CreateDirectory(dir);
        }

        string soPath = $"{dir}/Level_Sample_01.asset";
        string jsonPath = $"{dir}/Level_Sample_01.json";

        LevelDataSO sampleLevel = ScriptableObject.CreateInstance<LevelDataSO>();
        sampleLevel.levelNumber = 1;
        sampleLevel.levelName = "Sample Level 1";
        sampleLevel.gridSize = new Vector2Int(5, 5);
        sampleLevel.EnsureSlotsInitialized();

        Vector2 center = new Vector2(2, 2);
        foreach (var slot in sampleLevel.slots)
        {
            float dist = Vector2.Distance(center, new Vector2(slot.gridPosition.x, slot.gridPosition.y));
            slot.isActive = dist <= 2.1f;
        }

        var obstSlot = sampleLevel.GetSlot(new Vector2Int(2, 0));
        if (obstSlot != null)
        {
            obstSlot.isActive = true;
            obstSlot.slotType = SlotType.Block;
        }

        var breakSlot = sampleLevel.GetSlot(new Vector2Int(1, 1));
        if (breakSlot != null)
        {
            breakSlot.isActive = true;
            breakSlot.lockType = LockType.BreakLock;
            breakSlot.breakHitCount = 3;
        }

        var taskSlot = sampleLevel.GetSlot(new Vector2Int(3, 3));
        if (taskSlot != null)
        {
            taskSlot.isActive = true;
            taskSlot.lockType = LockType.TaskLock;
            taskSlot.taskTargetScore = 50;
        }

        var stackSlot1 = sampleLevel.GetSlot(new Vector2Int(2, 2));
        if (stackSlot1 != null)
        {
            stackSlot1.isActive = true;
            stackSlot1.hasStack = true;
            stackSlot1.stackColors = new List<Color>
            {
                new Color(1f, 0f, 0f),
                new Color(1f, 0.85f, 0f),
                new Color(0f, 0.2f, 1f),
                new Color(0.1f, 0.8f, 0.2f)
            };
        }

        var stackSlot2 = sampleLevel.GetSlot(new Vector2Int(1, 3));
        if (stackSlot2 != null)
        {
            stackSlot2.isActive = true;
            stackSlot2.hasStack = true;
            stackSlot2.stackColors = new List<Color>
            {
                new Color(0.7f, 0.1f, 0.9f),
                new Color(1f, 0.5f, 0f),
                new Color(1f, 0.85f, 0f)
            };
        }

        AssetDatabase.CreateAsset(sampleLevel, soPath);
        LevelDataJsonHelper.SaveToFile(jsonPath, sampleLevel);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        OpenWithLevel(sampleLevel);
        EditorUtility.DisplayDialog("Hexa Sort", $"Đã tạo thành công Level mẫu:\n- SO: {soPath}\n- JSON: {jsonPath}",
            "OK");
    }

    public static void OpenWithLevel(LevelDataSO level)
    {
        var window = GetWindow<LevelEditorWindow>("Hexa Sort Level Editor");
        window.minSize = new Vector2(900, 600);
        window.SetCurrentLevel(level);
        window.Show();
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        if (currentLevel != null)
        {
            serializedLevel = new SerializedObject(currentLevel);
            currentLevel.EnsureSlotsInitialized();
        }
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    public void SetCurrentLevel(LevelDataSO level)
    {
        currentLevel = level;
        if (currentLevel != null)
        {
            currentLevel.EnsureSlotsInitialized();
            serializedLevel = new SerializedObject(currentLevel);
            hasSelection = false;
        }

        Repaint();
    }

    private void OnGUI()
    {
        DrawTopToolbar();

        if (currentLevel == null)
        {
            DrawNoLevelNotice();
            return;
        }

        EditorGUILayout.Space(2);
        EditorGUILayout.BeginHorizontal();
        DrawLeftGridArea();
        DrawRightInspectorArea();

        EditorGUILayout.EndHorizontal();
    }

    #region Top Toolbar & File Management

    private void DrawTopToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("New Level", EditorStyles.toolbarButton, GUILayout.Width(75)))
        {
            CreateNewLevel();
        }

        EditorGUI.BeginChangeCheck();
        currentLevel =
            (LevelDataSO)EditorGUILayout.ObjectField(currentLevel, typeof(LevelDataSO), false, GUILayout.Width(220));
        if (EditorGUI.EndChangeCheck())
        {
            if (currentLevel != null)
            {
                currentLevel.EnsureSlotsInitialized();
                serializedLevel = new SerializedObject(currentLevel);
            }
        }

        if (currentLevel != null)
        {
            if (GUILayout.Button("Save SO", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                SaveCurrentSO();
            }

            if (GUILayout.Button("Save As...", EditorStyles.toolbarButton, GUILayout.Width(70)))
            {
                SaveAsNewSO();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Export JSON", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                ExportLevelJson();
            }

            if (GUILayout.Button("Import JSON", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                ImportLevelJson();
            }

            GUILayout.FlexibleSpace();

            showSceneHandles = GUILayout.Toggle(showSceneHandles, "Scene Handles", EditorStyles.toolbarButton);
            show3DScenePreview = GUILayout.Toggle(show3DScenePreview, "3D Preview", EditorStyles.toolbarButton);
            showGridLabels = GUILayout.Toggle(showGridLabels, "Labels", EditorStyles.toolbarButton);
        }
        else
        {
            GUILayout.FlexibleSpace();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawNoLevelNotice()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Space(20);
        EditorGUILayout.LabelField("Chưa có Level nào được mở!", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            "Hãy bấm 'New Level' ở góc trên để tạo level mới hoặc kéo thả file LevelDataSO vào ô trên toolbar.");
        GUILayout.Space(10);
        if (GUILayout.Button("Tạo Level Mới Ngay", GUILayout.Height(30), GUILayout.Width(180)))
        {
            CreateNewLevel();
        }

        GUILayout.Space(20);
        EditorGUILayout.EndVertical();
    }

    private void CreateNewLevel()
    {
        string dir = "Assets/Asset/_Scripts/Config/SO/Levels";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string path = AssetDatabase.GenerateUniqueAssetPath($"{dir}/Level_01.asset");
        LevelDataSO newLevel = ScriptableObject.CreateInstance<LevelDataSO>();
        newLevel.levelNumber = 1;
        newLevel.levelName = "Level 1";
        newLevel.gridSize = new Vector2Int(5, 5);
        newLevel.EnsureSlotsInitialized();

        foreach (var slot in newLevel.slots)
        {
            slot.isActive = true;
        }

        AssetDatabase.CreateAsset(newLevel, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        SetCurrentLevel(newLevel);
        Selection.activeObject = newLevel;
    }

    private void SaveCurrentSO()
    {
        if (currentLevel == null) return;
        EditorUtility.SetDirty(currentLevel);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ShowNotification(new GUIContent("Đã lưu Level thành công!"));
    }

    private void SaveAsNewSO()
    {
        if (currentLevel == null) return;
        string defaultName = $"{currentLevel.name}_Copy.asset";
        string path =
            EditorUtility.SaveFilePanelInProject("Save Level As", defaultName, "asset", "Chọn nơi lưu LevelDataSO");
        if (string.IsNullOrEmpty(path)) return;

        LevelDataSO clone = Instantiate(currentLevel);
        AssetDatabase.CreateAsset(clone, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        SetCurrentLevel(clone);
        Selection.activeObject = clone;
        ShowNotification(new GUIContent("Đã tạo bản sao Level mới!"));
    }

    private void ExportLevelJson()
    {
        if (currentLevel == null) return;
        string defaultName = $"{currentLevel.name}.json";
        string path = EditorUtility.SaveFilePanel("Export Level JSON", "Assets", defaultName, "json");
        if (string.IsNullOrEmpty(path)) return;

        LevelDataJsonHelper.SaveToFile(path, currentLevel);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Export JSON", $"Đã xuất thành công JSON ra file:\n{path}", "OK");
    }

    private void ImportLevelJson()
    {
        if (currentLevel == null) return;
        string path = EditorUtility.OpenFilePanel("Import Level JSON", "Assets", "json");
        if (string.IsNullOrEmpty(path)) return;

        Undo.RecordObject(currentLevel, "Import Level JSON");
        if (LevelDataJsonHelper.LoadFromFile(path, currentLevel))
        {
            currentLevel.EnsureSlotsInitialized();
            EditorUtility.SetDirty(currentLevel);
            AssetDatabase.SaveAssets();
            Repaint();
            EditorUtility.DisplayDialog("Import JSON", "Đã nạp dữ liệu từ file JSON thành công!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Import JSON", "Không thể đọc file JSON!", "OK");
        }
    }

    #endregion

    #region Left Column: Grid Layout & Tools

    private void DrawLeftGridArea()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.6f));

        DrawGridHeaderAndTools();

        DrawHexMatrixGUI();

        EditorGUILayout.EndVertical();
    }

    private void DrawGridHeaderAndTools()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Grid Size:", EditorStyles.boldLabel, GUILayout.Width(70));

        EditorGUI.BeginChangeCheck();
        int newW = EditorGUILayout.IntField(currentLevel.gridSize.x, GUILayout.Width(45));
        EditorGUILayout.LabelField("x", GUILayout.Width(15));
        int newH = EditorGUILayout.IntField(currentLevel.gridSize.y, GUILayout.Width(45));
        if (EditorGUI.EndChangeCheck())
        {
            newW = Mathf.Clamp(newW, 1, 15);
            newH = Mathf.Clamp(newH, 1, 15);
            if (newW != currentLevel.gridSize.x || newH != currentLevel.gridSize.y)
            {
                Undo.RecordObject(currentLevel, "Change Grid Size & Reset Slots");
                currentLevel.gridSize = new Vector2Int(newW, newH);
                currentLevel.ResetSlots();
                EditorUtility.SetDirty(currentLevel);
                Repaint();
                SceneView.RepaintAll();
            }
        }

        GUILayout.Space(15);
        EditorGUILayout.LabelField("Zoom:", GUILayout.Width(40));
        cellDisplaySize = EditorGUILayout.Slider(cellDisplaySize, 32f, 70f, GUILayout.Width(120));

        GUILayout.FlexibleSpace();
        int activeCount = currentLevel.GetActiveSlots().Count;
        EditorGUILayout.LabelField($"Active Slots: {activeCount}", EditorStyles.miniBoldLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(3);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Form Layout:", EditorStyles.boldLabel, GUILayout.Width(85));

        bool isPointy = currentLevel.formLayout == GridFormLayout.PointyTopped;
        bool isFlat = currentLevel.formLayout == GridFormLayout.FlatTopped;

        GUI.backgroundColor = isPointy ? Color.cyan : Color.white;
        if (GUILayout.Button("🔀 So le (Đỉnh dọc)", EditorStyles.miniButtonLeft, GUILayout.Width(150)))
        {
            if (currentLevel.formLayout != GridFormLayout.PointyTopped)
            {
                Undo.RecordObject(currentLevel, "Change Form Layout & Reset Slots");
                currentLevel.formLayout = GridFormLayout.PointyTopped;
                currentLevel.ResetSlots();
                EditorUtility.SetDirty(currentLevel);
                Repaint();
                SceneView.RepaintAll();
            }
        }

        GUI.backgroundColor = isFlat ? Color.cyan : Color.white;
        if (GUILayout.Button("📏 Thẳng hàng (Đỉnh ngang)", EditorStyles.miniButtonRight, GUILayout.Width(160)))
        {
            if (currentLevel.formLayout != GridFormLayout.FlatTopped)
            {
                Undo.RecordObject(currentLevel, "Change Form Layout & Reset Slots");
                currentLevel.formLayout = GridFormLayout.FlatTopped;
                currentLevel.ResetSlots();
                EditorUtility.SetDirty(currentLevel);
                Repaint();
                SceneView.RepaintAll();
            }
        }

        GUI.backgroundColor = Color.white;

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tool:", EditorStyles.boldLabel, GUILayout.Width(45));

        GUI.backgroundColor = currentTool == EditorToolMode.Select ? Color.cyan : Color.white;
        if (GUILayout.Button("Select", EditorStyles.miniButtonLeft)) currentTool = EditorToolMode.Select;

        GUI.backgroundColor = currentTool == EditorToolMode.ToggleSlot ? Color.cyan : Color.white;
        if (GUILayout.Button("Toggle Slot", EditorStyles.miniButtonMid)) currentTool = EditorToolMode.ToggleSlot;

        GUI.backgroundColor = currentTool == EditorToolMode.PaintActive ? Color.cyan : Color.white;
        if (GUILayout.Button("Add Slot (+)", EditorStyles.miniButtonMid)) currentTool = EditorToolMode.PaintActive;

        GUI.backgroundColor = currentTool == EditorToolMode.PaintInactive ? Color.cyan : Color.white;
        if (GUILayout.Button("Remove Slot (-)", EditorStyles.miniButtonMid)) currentTool = EditorToolMode.PaintInactive;

        GUI.backgroundColor = currentTool == EditorToolMode.PaintLock ? Color.cyan : Color.white;
        if (GUILayout.Button("Paint Lock", EditorStyles.miniButtonMid)) currentTool = EditorToolMode.PaintLock;

        GUI.backgroundColor = currentTool == EditorToolMode.PaintStack ? Color.cyan : Color.white;
        if (GUILayout.Button("Paint Stack", EditorStyles.miniButtonMid)) currentTool = EditorToolMode.PaintStack;

        GUI.backgroundColor = currentTool == EditorToolMode.Eraser ? Color.cyan : Color.white;
        if (GUILayout.Button("Eraser", EditorStyles.miniButtonRight)) currentTool = EditorToolMode.Eraser;

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        if (currentTool == EditorToolMode.PaintLock)
        {
            EditorGUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Brush Lock:", GUILayout.Width(80));
            brushLockType = (LockType)EditorGUILayout.EnumPopup(brushLockType, GUILayout.Width(100));
            if (brushLockType == LockType.BreakLock)
            {
                EditorGUILayout.LabelField("Hits:", GUILayout.Width(35));
                brushBreakHits = EditorGUILayout.IntSlider(brushBreakHits, 1, 10, GUILayout.Width(120));
            }
            else if (brushLockType == LockType.TaskLock)
            {
                EditorGUILayout.LabelField("Target:", GUILayout.Width(45));
                brushTaskTarget = EditorGUILayout.IntSlider(brushTaskTarget, 10, 200, GUILayout.Width(120));
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(4);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Shape Tools:", EditorStyles.miniBoldLabel, GUILayout.Width(75));

        if (GUILayout.Button("◀ Left", EditorStyles.miniButtonLeft, GUILayout.Width(50)))
        {
            ShiftGrid(-1, 0);
        }

        if (GUILayout.Button("▶ Right", EditorStyles.miniButtonMid, GUILayout.Width(50)))
        {
            ShiftGrid(1, 0);
        }

        if (GUILayout.Button("▲ Up", EditorStyles.miniButtonMid, GUILayout.Width(45)))
        {
            ShiftGrid(0, 1);
        }

        if (GUILayout.Button("▼ Down", EditorStyles.miniButtonRight, GUILayout.Width(50)))
        {
            ShiftGrid(0, -1);
        }

        GUILayout.Space(8);
        if (GUILayout.Button("Center Shape", EditorStyles.miniButton, GUILayout.Width(85)))
        {
            CenterShape();
        }

        if (GUILayout.Button("Auto Crop", EditorStyles.miniButton, GUILayout.Width(75)))
        {
            AutoCropBounds();
        }

        if (GUILayout.Button("+1 Size", EditorStyles.miniButton, GUILayout.Width(55)))
        {
            currentLevel.ResizeGrid(new Vector2Int(currentLevel.gridSize.x + 1, currentLevel.gridSize.y + 1));
            EditorUtility.SetDirty(currentLevel);
        }

        if (GUILayout.Button("All Active", EditorStyles.miniButton))
        {
            ApplyShapePreset(p => true);
        }

        if (GUILayout.Button("Clear All", EditorStyles.miniButton))
        {
            Undo.RecordObject(currentLevel, "Clear All Slots");
            currentLevel.ResetSlots();
            EditorUtility.SetDirty(currentLevel);
            Repaint();
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Invert", EditorStyles.miniButton))
        {
            ApplyInvertShapePreset();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawHexMatrixGUI()
    {
        gridScrollPos = EditorGUILayout.BeginScrollView(gridScrollPos, EditorStyles.helpBox);

        int width = currentLevel.gridSize.x;
        int height = currentLevel.gridSize.y;

        bool isFlatTopped = currentLevel.formLayout == GridFormLayout.FlatTopped;
        float radius = cellDisplaySize * 0.5f;

        float colStepX;
        float rowStepY;

        if (isFlatTopped)
        {
            colStepX = radius * 1.5f + 4f;
            rowStepY = radius * Mathf.Sqrt(3f) + 3f;
        }
        else
        {
            colStepX = radius * Mathf.Sqrt(3f) + 3f;
            rowStepY = radius * 1.5f + 4f;
        }

        float totalWidth = (width + 2) * colStepX + 80f;
        float totalHeight = (height + 2) * rowStepY + 80f;

        Rect matrixRect = GUILayoutUtility.GetRect(totalWidth, totalHeight);

        Event e = Event.current;

        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0 &&
            matrixRect.Contains(e.mousePosition))
        {
            Vector2 mousePos = e.mousePosition;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 cellCenter = GetCellCenterGUI(matrixRect, x, y, height, colStepX, rowStepY, isFlatTopped);
                    Vector3[] corners = GetHexCornersGUI(cellCenter, radius, isFlatTopped);

                    if (IsPointInHex(mousePos, corners))
                    {
                        Vector2Int cellPos = new Vector2Int(x, y);
                        if (cellPos != lastDraggedCell)
                        {
                            lastDraggedCell = cellPos;
                            var slot = currentLevel.GetOrCreateSlot(cellPos);
                            OnCellClicked(slot);
                        }

                        break;
                    }
                }
            }
        }

        if (e.type == EventType.MouseUp)
        {
            lastDraggedCell = new Vector2Int(-1, -1);
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                SlotLevelData slot = currentLevel.GetOrCreateSlot(pos);

                Vector2 cellCenter = GetCellCenterGUI(matrixRect, x, y, height, colStepX, rowStepY, isFlatTopped);
                DrawHexCellGUI(cellCenter, radius, slot, isFlatTopped);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private static Vector2 GetCellCenterGUI(Rect matrixRect, int x, int y, int height, float colStepX, float rowStepY,
        bool isFlatTopped)
    {
        int visualY = (height - 1) - y;
        if (isFlatTopped)
        {
            float yOffset = (x % 2 != 0) ? (rowStepY * 0.5f) : 0f;
            float centerX = matrixRect.x + 45f + x * colStepX;
            float centerY = matrixRect.y + 45f + visualY * rowStepY + yOffset;
            return new Vector2(centerX, centerY);
        }
        else
        {
            float xOffset = (Mathf.Abs(y) % 2 != 0) ? (colStepX * 0.5f) : 0f;
            float centerX = matrixRect.x + 45f + x * colStepX + xOffset;
            float centerY = matrixRect.y + 45f + visualY * rowStepY;
            return new Vector2(centerX, centerY);
        }
    }

    private static Vector3[] GetHexCornersGUI(Vector2 center, float radius, bool isFlatTopped)
    {
        Vector3[] corners = new Vector3[6];
        float angleOffset = isFlatTopped ? 0f : 30f;
        for (int i = 0; i < 6; i++)
        {
            float angleDeg = 60f * i + angleOffset;
            float rad = angleDeg * Mathf.Deg2Rad;
            corners[i] = new Vector3(
                center.x + radius * Mathf.Cos(rad),
                center.y + radius * Mathf.Sin(rad),
                0f
            );
        }

        return corners;
    }

    private static bool IsPointInHex(Vector2 p, Vector3[] corners)
    {
        bool inside = false;
        for (int i = 0, j = 5; i < 6; j = i++)
        {
            if (((corners[i].y > p.y) != (corners[j].y > p.y)) &&
                (p.x < (corners[j].x - corners[i].x) * (p.y - corners[i].y) / (corners[j].y - corners[i].y) +
                    corners[i].x))
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private void DrawHexCellGUI(Vector2 center, float radius, SlotLevelData slot, bool isFlatTopped)
    {
        bool isSelected = hasSelection && selectedSlotPos == slot.gridPosition;
        Vector3[] corners = GetHexCornersGUI(center, radius, isFlatTopped);

        Event e = Event.current;
        if (e.type == EventType.MouseDown && IsPointInHex(e.mousePosition, corners))
        {
            OnCellClicked(slot);
            e.Use();
        }

        Color bgColor;
        if (!slot.isActive)
        {
            bgColor = new Color(0.18f, 0.18f, 0.2f, 0.45f);
        }
        else if (slot.slotType == SlotType.Block)
        {
            bgColor = new Color(0.55f, 0.22f, 0.22f, 0.95f);
        }
        else if (slot.slotType == SlotType.Advertising)
        {
            bgColor = new Color(0.58f, 0.42f, 0.68f, 0.95f);
        }
        else if (slot.lockType == LockType.BreakLock)
        {
            bgColor = new Color(0.28f, 0.48f, 0.72f, 0.95f);
        }
        else if (slot.lockType == LockType.TaskLock)
        {
            bgColor = new Color(0.72f, 0.52f, 0.18f, 0.95f);
        }
        else
        {
            bgColor = new Color(0.9f, 0.9f, 0.92f, 1f);
        }

        Handles.BeginGUI();
        Handles.color = bgColor;
        Handles.DrawAAConvexPolygon(corners);

        Vector3[] closedOutline = new Vector3[7];
        for (int i = 0; i < 6; i++) closedOutline[i] = corners[i];
        closedOutline[6] = corners[0];

        if (isSelected)
        {
            Handles.color = Color.yellow;
            Handles.DrawAAPolyLine(4f, closedOutline);
        }
        else
        {
            Color borderColor = slot.isActive
                ? new Color(0.15f, 0.15f, 0.2f, 0.8f)
                : new Color(0.35f, 0.35f, 0.35f, 0.35f);
            Handles.color = borderColor;
            Handles.DrawAAPolyLine(1.8f, closedOutline);
        }

        Handles.EndGUI();

        if (!slot.isActive)
        {
            GUIStyle inactiveStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9,
                normal = { textColor = new Color(0.5f, 0.5f, 0.5f, 0.55f) }
            };
            Rect inactiveRect = new Rect(center.x - radius, center.y - 8f, radius * 2f, 16f);
            GUI.Label(inactiveRect, $"{slot.gridPosition.x},{slot.gridPosition.y}", inactiveStyle);
            return;
        }

        if (slot.hasStack && slot.stackColors != null && slot.stackColors.Count > 0)
        {
            DrawMiniHexStackGUI(center, radius, slot.stackColors, isFlatTopped);
        }

        GUIStyle coordStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.UpperCenter,
            fontSize = 9,
            fontStyle = FontStyle.Bold,
            normal =
            {
                textColor = slot.slotType == SlotType.Block || slot.slotType == SlotType.Advertising
                    ? Color.white
                    : new Color(0.15f, 0.15f, 0.15f, 0.85f)
            }
        };
        Rect coordRect = new Rect(center.x - radius, center.y - radius * 0.88f, radius * 2f, 14f);
        GUI.Label(coordRect, $"{slot.gridPosition.x},{slot.gridPosition.y}", coordStyle);

        if (slot.slotType == SlotType.Block)
        {
            GUIStyle blockStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
                normal = { textColor = Color.yellow }
            };
            Rect blockRect = new Rect(center.x - radius, center.y - 8f, radius * 2f, 16f);
            GUI.Label(blockRect, "BLOCK", blockStyle);
        }
        else if (slot.slotType == SlotType.Advertising)
        {
            GUIStyle adStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                normal = { textColor = Color.white }
            };
            Rect adRect = new Rect(center.x - radius, center.y - 8f, radius * 2f, 18f);
            GUI.Label(adRect, "🎬 AD", adStyle);
        }
        else if (slot.lockType == LockType.BreakLock)
        {
            GUIStyle lockStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
                normal = { textColor = Color.white }
            };
            float offsetY = slot.hasStack ? -radius * 0.35f : 0f;
            Rect lockRect = new Rect(center.x - radius, center.y + offsetY - 8f, radius * 2f, 16f);
            GUI.Label(lockRect, $"🛡️ {slot.breakHitCount}", lockStyle);
        }
        else if (slot.lockType == LockType.TaskLock)
        {
            GUIStyle lockStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
                normal = { textColor = Color.white }
            };
            float offsetY = slot.hasStack ? -radius * 0.35f : 0f;
            Rect lockRect = new Rect(center.x - radius, center.y + offsetY - 8f, radius * 2f, 16f);
            GUI.Label(lockRect, $"🎯 {slot.taskTargetScore}", lockStyle);
        }
    }

    private void DrawMiniHexStackGUI(Vector2 center, float hexRadius, List<Color> colors, bool isFlatTopped)
    {
        int count = colors.Count;
        if (count == 0) return;

        float miniRadius = hexRadius * 0.54f;
        float layerSpacing = Mathf.Min(3.2f, (hexRadius * 0.45f) / count);
        float startOffsetY = (count - 1) * layerSpacing * 0.4f;
        Vector2 stackBase = center + new Vector2(0f, hexRadius * 0.08f + startOffsetY);

        Handles.BeginGUI();
        Vector2 topLayerCenter = stackBase;
        for (int i = 0; i < count; i++)
        {
            Vector2 layerCenter = stackBase - new Vector2(0f, i * layerSpacing);
            if (i == count - 1) topLayerCenter = layerCenter;

            Vector3[] layerCorners = GetHexCornersGUI(layerCenter, miniRadius, isFlatTopped);

            Handles.color = colors[i];
            Handles.DrawAAConvexPolygon(layerCorners);

            Vector3[] closedOutline = new Vector3[7];
            for (int v = 0; v < 6; v++) closedOutline[v] = layerCorners[v];
            closedOutline[6] = layerCorners[0];

            Handles.color = new Color(0f, 0f, 0f, 0.7f);
            Handles.DrawAAPolyLine(1.2f, closedOutline);
        }

        Handles.EndGUI();

        Color topColor = colors[count - 1];
        float brightness = topColor.r * 0.299f + topColor.g * 0.587f + topColor.b * 0.114f;
        Color textNumberColor = brightness > 0.6f ? new Color(0.12f, 0.12f, 0.12f) : Color.white;

        GUIStyle numberStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            normal = { textColor = textNumberColor }
        };
        Rect numberRect = new Rect(topLayerCenter.x - miniRadius, topLayerCenter.y - miniRadius, miniRadius * 2f,
            miniRadius * 2f);
        GUI.Label(numberRect, count.ToString(), numberStyle);
    }

    private void OnCellClicked(SlotLevelData slot)
    {
        Undo.RecordObject(currentLevel, "Modify Slot");

        switch (currentTool)
        {
            case EditorToolMode.Select:
                selectedSlotPos = slot.gridPosition;
                hasSelection = true;
                break;

            case EditorToolMode.ToggleSlot:
                slot.isActive = !slot.isActive;
                selectedSlotPos = slot.gridPosition;
                hasSelection = true;
                break;

            case EditorToolMode.PaintActive:
                slot.isActive = true;
                selectedSlotPos = slot.gridPosition;
                hasSelection = true;
                break;

            case EditorToolMode.PaintInactive:
                slot.isActive = false;
                break;

            case EditorToolMode.PaintLock:
                slot.isActive = true;
                slot.lockType = brushLockType;
                slot.breakHitCount = brushBreakHits;
                slot.taskTargetScore = brushTaskTarget;
                selectedSlotPos = slot.gridPosition;
                hasSelection = true;
                break;

            case EditorToolMode.PaintStack:
                slot.isActive = true;
                if (clipboardSlotData != null && clipboardSlotData.hasStack)
                {
                    slot.hasStack = true;
                    slot.stackColors = new List<Color>(clipboardSlotData.stackColors);
                }
                else
                {
                    slot.hasStack = true;
                    if (slot.stackColors == null) slot.stackColors = new List<Color>();
                    if (slot.stackColors.Count == 0) slot.stackColors.Add(brushColor);
                }

                selectedSlotPos = slot.gridPosition;
                hasSelection = true;
                break;

            case EditorToolMode.Eraser:
                slot.lockType = LockType.None;
                slot.slotType = SlotType.Nozmal;
                slot.ClearStack();
                break;
        }

        EditorUtility.SetDirty(currentLevel);
        Repaint();
        SceneView.RepaintAll();
    }

    private void DrawRectOutline(Rect r, Color color, float thickness)
    {
        EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, thickness), color);
        EditorGUI.DrawRect(new Rect(r.x, r.yMax - thickness, r.width, thickness), color);
        EditorGUI.DrawRect(new Rect(r.x, r.y, thickness, r.height), color);
        EditorGUI.DrawRect(new Rect(r.xMax - thickness, r.y, thickness, r.height), color);
    }

    #endregion

    #region Right Column: Slot Inspector & Stack Designer

    private void DrawRightInspectorArea()
    {
        inspectorScrollPos = EditorGUILayout.BeginScrollView(inspectorScrollPos, EditorStyles.helpBox,
            GUILayout.Width(position.width * 0.4f - 15f));

        if (!hasSelection)
        {
            EditorGUILayout.LabelField("Slot Inspector", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Click chọn một ô trên lưới để chỉnh sửa chi tiết các thuộc tính Lock, Obstacle và Hexagon Stack.",
                MessageType.Info);
            EditorGUILayout.EndScrollView();
            return;
        }

        SlotLevelData slot = currentLevel.GetSlot(selectedSlotPos);
        if (slot == null)
        {
            hasSelection = false;
            EditorGUILayout.EndScrollView();
            return;
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"Slot Properties [{slot.gridPosition.x}, {slot.gridPosition.y}]",
            EditorStyles.boldLabel);
        slot.isActive = EditorGUILayout.Toggle("Is Active Slot (Grid Form)", slot.isActive);
        slot.slotType = (SlotType)EditorGUILayout.EnumPopup("Slot Type", slot.slotType);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        DrawLockInspector(slot);

        EditorGUILayout.Space(5);

        DrawStackDesignerInspector(slot);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(currentLevel, "Edit Slot Properties");
            EditorUtility.SetDirty(currentLevel);
            Repaint();
            SceneView.RepaintAll();
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawLockInspector(SlotLevelData slot)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Obstacle & Lock Settings", EditorStyles.boldLabel);

        slot.lockType = (LockType)EditorGUILayout.EnumPopup("Lock Type", slot.lockType);

        if (slot.lockType == LockType.BreakLock)
        {
            slot.breakHitCount = EditorGUILayout.IntSlider("Hit Count (Remain)", slot.breakHitCount, 1, 20);
            EditorGUILayout.HelpBox("BreakLock cần N lần merge lục giác kế bên để phá khóa.", MessageType.None);
        }
        else if (slot.lockType == LockType.TaskLock)
        {
            slot.taskTargetScore = EditorGUILayout.IntSlider("Target Score", slot.taskTargetScore, 5, 500);
            EditorGUILayout.HelpBox("TaskLock cần thu thập đủ N hexagon để tự động mở khóa.", MessageType.None);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawStackDesignerInspector(SlotLevelData slot)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Pre-placed Hexagon Stack Designer", EditorStyles.boldLabel);

        slot.hasStack = EditorGUILayout.Toggle("Has Pre-placed Stack", slot.hasStack);

        if (!slot.hasStack)
        {
            EditorGUILayout.HelpBox("Ô này hiện tại rỗng (người chơi có thể thả stack mới vào đây).", MessageType.None);
            EditorGUILayout.EndVertical();
            return;
        }

        if (slot.stackColors == null)
        {
            slot.stackColors = new List<Color>();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Copy Stack", EditorStyles.miniButtonLeft))
        {
            clipboardSlotData = slot.Clone();
            ShowNotification(new GUIContent("Đã copy Stack!"));
        }

        if (GUILayout.Button("Paste Stack", EditorStyles.miniButtonMid))
        {
            if (clipboardSlotData != null && clipboardSlotData.hasStack)
            {
                slot.stackColors = new List<Color>(clipboardSlotData.stackColors);
                slot.hasStack = true;
            }
        }

        if (GUILayout.Button("Clear Stack", EditorStyles.miniButtonRight))
        {
            slot.ClearStack();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField("Quick Color Palette (Click để chọn màu):", EditorStyles.miniBoldLabel);
        DrawColorPaletteGUI();

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Batch Add (Thêm nhiều lớp nhanh như trong ảnh):", EditorStyles.miniBoldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Count:", GUILayout.Width(42));
        batchAddCount = EditorGUILayout.IntSlider(batchAddCount, 1, 15, GUILayout.Width(110));
        batchAddColor = EditorGUILayout.ColorField(batchAddColor, GUILayout.Width(45));
        if (GUILayout.Button($"+ Add {batchAddCount} Layers", EditorStyles.miniButton))
        {
            for (int i = 0; i < batchAddCount; i++) slot.AddLayer(batchAddColor);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Quick Add:", GUILayout.Width(65));
        if (GUILayout.Button("+1", EditorStyles.miniButton)) slot.AddLayer(brushColor);
        if (GUILayout.Button("+3", EditorStyles.miniButton))
        {
            for (int i = 0; i < 3; i++) slot.AddLayer(brushColor);
        }

        if (GUILayout.Button("+5", EditorStyles.miniButton))
        {
            for (int i = 0; i < 5; i++) slot.AddLayer(brushColor);
        }

        if (GUILayout.Button("+7", EditorStyles.miniButton))
        {
            for (int i = 0; i < 7; i++) slot.AddLayer(brushColor);
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"Stack Layers (Total: {slot.stackColors.Count}) - [Top to Bottom]:",
            EditorStyles.boldLabel);

        int layerCount = slot.stackColors.Count;
        for (int i = layerCount - 1; i >= 0; i--)
        {
            EditorGUILayout.BeginHorizontal();

            string tag = (i == layerCount - 1) ? "[TOP]" : (i == 0 ? "[BOTTOM]" : $"L{i}");
            EditorGUILayout.LabelField(tag, GUILayout.Width(60));

            slot.stackColors[i] = EditorGUILayout.ColorField(slot.stackColors[i]);

            GUI.backgroundColor = brushColor;
            if (GUILayout.Button("Apply", GUILayout.Width(45)))
            {
                slot.stackColors[i] = brushColor;
            }

            GUI.backgroundColor = Color.white;

            GUI.enabled = (i < layerCount - 1);
            if (GUILayout.Button("▲", GUILayout.Width(22)))
            {
                Color temp = slot.stackColors[i];
                slot.stackColors[i] = slot.stackColors[i + 1];
                slot.stackColors[i + 1] = temp;
            }

            GUI.enabled = (i > 0);
            if (GUILayout.Button("▼", GUILayout.Width(22)))
            {
                Color temp = slot.stackColors[i];
                slot.stackColors[i] = slot.stackColors[i - 1];
                slot.stackColors[i - 1] = temp;
            }

            GUI.enabled = true;

            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("X", GUILayout.Width(22)))
            {
                slot.RemoveLayer(i);
                break;
            }

            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawColorPaletteGUI()
    {
        EditorGUILayout.BeginHorizontal();
        foreach (var col in PresetColors)
        {
            GUI.backgroundColor = col;
            if (GUILayout.Button("", GUILayout.Width(24), GUILayout.Height(24)))
            {
                brushColor = col;
            }
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Current Brush Color:", GUILayout.Width(130));
        brushColor = EditorGUILayout.ColorField(brushColor);
        EditorGUILayout.EndHorizontal();
    }

    #endregion

    #region Shape Presets & Transform Tools

    public static int HexDistance(Vector2Int a, Vector2Int b)
    {
        int qa = a.x - (a.y - (a.y & 1)) / 2;
        int ra = a.y;
        int sa = -qa - ra;

        int qb = b.x - (b.y - (b.y & 1)) / 2;
        int rb = b.y;
        int sb = -qb - rb;

        return (Mathf.Abs(qa - qb) + Mathf.Abs(ra - rb) + Mathf.Abs(sa - sb)) / 2;
    }

    private void ApplyShapePreset(Func<Vector2Int, bool> condition)
    {
        if (currentLevel == null) return;
        Undo.RecordObject(currentLevel, "Apply Shape Preset");

        foreach (var slot in currentLevel.slots)
        {
            if (slot != null)
            {
                slot.isActive = condition(slot.gridPosition);
            }
        }

        EditorUtility.SetDirty(currentLevel);
        Repaint();
        SceneView.RepaintAll();
    }

    private void ApplyHexRadiusPreset(int radius)
    {
        if (currentLevel == null) return;
        Undo.RecordObject(currentLevel, $"Apply Hex Radius {radius}");

        int requiredSize = radius * 2 + 3;
        if (currentLevel.gridSize.x < requiredSize || currentLevel.gridSize.y < requiredSize)
        {
            currentLevel.ResizeGrid(new Vector2Int(Mathf.Max(currentLevel.gridSize.x, requiredSize),
                Mathf.Max(currentLevel.gridSize.y, requiredSize)));
        }

        Vector2Int center = new Vector2Int(currentLevel.gridSize.x / 2, currentLevel.gridSize.y / 2);

        foreach (var slot in currentLevel.slots)
        {
            if (slot == null) continue;
            slot.isActive = HexDistance(slot.gridPosition, center) <= radius;
        }

        EditorUtility.SetDirty(currentLevel);
        Repaint();
        SceneView.RepaintAll();
    }

    private void ApplyLevel1Preset()
    {
        if (currentLevel == null) return;
        Undo.RecordObject(currentLevel, "Apply Level 1 Preset (From Screenshot)");

        currentLevel.levelNumber = 1;
        currentLevel.levelName = "Level 1";
        currentLevel.ResizeGrid(new Vector2Int(7, 7));

        foreach (var s in currentLevel.slots)
        {
            s.isActive = false;
            s.slotType = SlotType.Nozmal;
            s.lockType = LockType.None;
            s.ClearStack();
        }

        Vector2Int[] activeCoords = new Vector2Int[]
        {
            new Vector2Int(3, 5), new Vector2Int(4, 5),
            new Vector2Int(2, 4), new Vector2Int(3, 4), new Vector2Int(4, 4),
            new Vector2Int(2, 3), new Vector2Int(3, 3), new Vector2Int(4, 3),
            new Vector2Int(2, 2), new Vector2Int(3, 2), new Vector2Int(4, 2),
            new Vector2Int(2, 1), new Vector2Int(3, 1),
            new Vector2Int(3, 0) // Ô thò ra ở đáy (nơi ngón tay chỉ vào)
        };

        foreach (var pos in activeCoords)
        {
            var slot = currentLevel.GetOrCreateSlot(pos);
            slot.isActive = true;
        }

        var blueSlot = currentLevel.GetSlot(new Vector2Int(2, 3));
        if (blueSlot != null)
        {
            blueSlot.hasStack = true;
            blueSlot.stackColors = new List<Color>();
            Color blue = new Color(0.1f, 0.45f, 0.95f);
            for (int i = 0; i < 5; i++) blueSlot.stackColors.Add(blue);
        }

        var yellowSlot = currentLevel.GetSlot(new Vector2Int(3, 3));
        if (yellowSlot != null)
        {
            yellowSlot.hasStack = true;
            yellowSlot.stackColors = new List<Color>();
            Color yellow = new Color(1f, 0.85f, 0.1f);
            for (int i = 0; i < 6; i++) yellowSlot.stackColors.Add(yellow);
        }

        var redSlot = currentLevel.GetSlot(new Vector2Int(4, 3));
        if (redSlot != null)
        {
            redSlot.hasStack = true;
            redSlot.stackColors = new List<Color>();
            Color red = new Color(0.95f, 0.2f, 0.25f);
            for (int i = 0; i < 5; i++) redSlot.stackColors.Add(red);
        }

        EditorUtility.SetDirty(currentLevel);
        Repaint();
        SceneView.RepaintAll();
        ShowNotification(new GUIContent("Đã tạo Shape Level 1 chuẩn như ảnh!"));
    }

    private void ApplyLevel6Preset()
    {
        if (currentLevel == null) return;
        Undo.RecordObject(currentLevel, "Apply Level 6 Preset (From Screenshot)");

        currentLevel.levelNumber = 6;
        currentLevel.levelName = "Level 6";
        currentLevel.ResizeGrid(new Vector2Int(7, 7));

        foreach (var s in currentLevel.slots)
        {
            s.isActive = false;
            s.slotType = SlotType.Nozmal;
            s.lockType = LockType.None;
            s.ClearStack();
        }

        Vector2Int center = new Vector2Int(3, 3);
        foreach (var slot in currentLevel.slots)
        {
            slot.isActive = HexDistance(slot.gridPosition, center) <= 2;
        }

        var adSlot = currentLevel.GetSlot(center);
        if (adSlot != null)
        {
            adSlot.slotType = SlotType.Advertising;
        }

        Color orange = new Color(0.95f, 0.45f, 0.15f);
        Color red = new Color(0.95f, 0.2f, 0.25f);
        Color yellow = new Color(1f, 0.85f, 0.1f);
        Color green = new Color(0.15f, 0.75f, 0.2f);
        Color cyan = new Color(0.15f, 0.8f, 0.85f);
        Color purple = new Color(0.65f, 0.15f, 0.85f);

        var topSlot = currentLevel.GetSlot(new Vector2Int(3, 5));
        if (topSlot != null)
        {
            topSlot.hasStack = true;
            topSlot.stackColors = new List<Color> { purple, purple, green, green, yellow, yellow, orange, orange };
        }

        var redSlot1 = currentLevel.GetSlot(new Vector2Int(2, 4));
        if (redSlot1 != null)
        {
            redSlot1.hasStack = true;
            redSlot1.stackColors = new List<Color>();
            for (int i = 0; i < 5; i++) redSlot1.stackColors.Add(red);
        }

        var orangeSlot1 = currentLevel.GetSlot(new Vector2Int(1, 3));
        if (orangeSlot1 != null)
        {
            orangeSlot1.hasStack = true;
            orangeSlot1.stackColors = new List<Color> { green, green, cyan, cyan, orange, orange, orange };
        }

        var orangeSlot2 = currentLevel.GetSlot(new Vector2Int(4, 3));
        if (orangeSlot2 != null)
        {
            orangeSlot2.hasStack = true;
            orangeSlot2.stackColors = new List<Color>();
            for (int i = 0; i < 7; i++) orangeSlot2.stackColors.Add(orange);
        }

        var cyanSlot = currentLevel.GetSlot(new Vector2Int(4, 4));
        if (cyanSlot != null)
        {
            cyanSlot.hasStack = true;
            cyanSlot.stackColors = new List<Color>();
            for (int i = 0; i < 7; i++) cyanSlot.stackColors.Add(cyan);
        }

        var greenSlot1 = currentLevel.GetSlot(new Vector2Int(5, 4));
        if (greenSlot1 != null)
        {
            greenSlot1.hasStack = true;
            greenSlot1.stackColors = new List<Color>();
            for (int i = 0; i < 7; i++) greenSlot1.stackColors.Add(green);
        }

        var redSlot2 = currentLevel.GetSlot(new Vector2Int(5, 3));
        if (redSlot2 != null)
        {
            redSlot2.hasStack = true;
            redSlot2.stackColors = new List<Color>();
            for (int i = 0; i < 5; i++) redSlot2.stackColors.Add(red);
        }

        var greenSlot2 = currentLevel.GetSlot(new Vector2Int(3, 2));
        if (greenSlot2 != null)
        {
            greenSlot2.hasStack = true;
            greenSlot2.stackColors = new List<Color> { purple, purple, orange, orange, green, green, green };
        }

        EditorUtility.SetDirty(currentLevel);
        Repaint();
        SceneView.RepaintAll();
        ShowNotification(new GUIContent("Đã tạo Shape Level 6 chuẩn như ảnh!"));
    }

    private void ApplyInvertShapePreset()
    {
        if (currentLevel == null) return;
        Undo.RecordObject(currentLevel, "Invert Form");

        foreach (var slot in currentLevel.slots)
        {
            if (slot != null)
            {
                slot.isActive = !slot.isActive;
            }
        }

        EditorUtility.SetDirty(currentLevel);
        Repaint();
        SceneView.RepaintAll();
    }

    private void ShiftGrid(int deltaX, int deltaY)
    {
        if (currentLevel == null) return;
        Undo.RecordObject(currentLevel, "Shift Grid Shape");

        var oldSlots = new List<SlotLevelData>();
        foreach (var s in currentLevel.slots)
        {
            if (s != null) oldSlots.Add(s.Clone());
        }

        foreach (var s in currentLevel.slots)
        {
            s.isActive = false;
            s.slotType = SlotType.Nozmal;
            s.lockType = LockType.None;
            s.ClearStack();
        }

        foreach (var old in oldSlots)
        {
            if (!old.isActive && !old.hasStack && old.lockType == LockType.None && old.slotType == SlotType.Nozmal)
                continue;

            Vector2Int newPos = new Vector2Int(old.gridPosition.x + deltaX, old.gridPosition.y + deltaY);
            if (newPos.x >= 0 && newPos.x < currentLevel.gridSize.x &&
                newPos.y >= 0 && newPos.y < currentLevel.gridSize.y)
            {
                var targetSlot = currentLevel.GetOrCreateSlot(newPos);
                targetSlot.isActive = old.isActive;
                targetSlot.slotType = old.slotType;
                targetSlot.lockType = old.lockType;
                targetSlot.breakHitCount = old.breakHitCount;
                targetSlot.taskTargetScore = old.taskTargetScore;
                targetSlot.hasStack = old.hasStack;
                targetSlot.stackColors = new List<Color>(old.stackColors);
            }
        }

        EditorUtility.SetDirty(currentLevel);
        Repaint();
        SceneView.RepaintAll();
    }

    private void CenterShape()
    {
        if (currentLevel == null) return;
        var activeSlots = currentLevel.GetActiveSlots();
        if (activeSlots.Count == 0) return;

        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;
        foreach (var s in activeSlots)
        {
            if (s.gridPosition.x < minX) minX = s.gridPosition.x;
            if (s.gridPosition.x > maxX) maxX = s.gridPosition.x;
            if (s.gridPosition.y < minY) minY = s.gridPosition.y;
            if (s.gridPosition.y > maxY) maxY = s.gridPosition.y;
        }

        int currentCenterX = (minX + maxX) / 2;
        int currentCenterY = (minY + maxY) / 2;
        int gridCenterX = currentLevel.gridSize.x / 2;
        int gridCenterY = currentLevel.gridSize.y / 2;

        int deltaX = gridCenterX - currentCenterX;
        int deltaY = gridCenterY - currentCenterY;

        if (deltaX != 0 || deltaY != 0)
        {
            ShiftGrid(deltaX, deltaY);
        }
    }

    private void AutoCropBounds()
    {
        if (currentLevel == null) return;
        var activeSlots = currentLevel.GetActiveSlots();
        if (activeSlots.Count == 0) return;

        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;
        foreach (var s in activeSlots)
        {
            if (s.gridPosition.x < minX) minX = s.gridPosition.x;
            if (s.gridPosition.x > maxX) maxX = s.gridPosition.x;
            if (s.gridPosition.y < minY) minY = s.gridPosition.y;
            if (s.gridPosition.y > maxY) maxY = s.gridPosition.y;
        }

        if (minX > 0 || minY > 0)
        {
            ShiftGrid(-minX, -minY);
            maxX -= minX;
            maxY -= minY;
        }

        currentLevel.ResizeGrid(new Vector2Int(maxX + 1, maxY + 1));
        EditorUtility.SetDirty(currentLevel);
        Repaint();
        SceneView.RepaintAll();
    }

    #endregion

    #region Scene View GUI & 3D Live Preview

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!showSceneHandles || currentLevel == null) return;

        var gridObj = FindFirstObjectByType<UnityEngine.Grid>();
        Vector3 originPos = gridObj != null ? gridObj.transform.position : Vector3.zero;

        Event e = Event.current;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);

        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, originPos);

            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 worldHit = ray.GetPoint(enter);
                Vector2Int clickedCell;
                bool isFlatScene = currentLevel.formLayout == GridFormLayout.FlatTopped;

                if (isFlatScene)
                {
                    float R = gridObj != null ? gridObj.cellSize.y / 2f : 1.0f;
                    float colStepX = 1.5f * R;
                    float rowStepZ = Mathf.Sqrt(3f) * R;
                    Vector3 rel = worldHit - originPos;
                    int cellX = Mathf.RoundToInt(rel.x / colStepX);
                    float zOffset = (cellX % 2 == 0) ? (rowStepZ * 0.5f) : 0f;
                    int cellY = Mathf.RoundToInt((rel.z - zOffset) / rowStepZ);
                    clickedCell = new Vector2Int(cellX, cellY);
                }
                else
                {
                    clickedCell = gridObj != null
                        ? (Vector2Int)gridObj.WorldToCell(worldHit)
                        : new Vector2Int(Mathf.RoundToInt(worldHit.x / 1.732f), Mathf.RoundToInt(worldHit.z / 1.5f));
                }

                if (clickedCell.x >= 0 && clickedCell.x < currentLevel.gridSize.x &&
                    clickedCell.y >= 0 && clickedCell.y < currentLevel.gridSize.y)
                {
                    var slot = currentLevel.GetOrCreateSlot(clickedCell);
                    OnCellClicked(slot);
                    e.Use();
                }
            }
        }

        bool isFlatMode = currentLevel.formLayout == GridFormLayout.FlatTopped;
        foreach (var slot in currentLevel.slots)
        {
            if (slot == null) continue;

            Vector3 cellCenter;
            if (isFlatMode)
            {
                float R = gridObj != null ? gridObj.cellSize.y / 2f : 1.0f;
                float colStepX = 1.5f * R;
                float rowStepZ = Mathf.Sqrt(3f) * R;
                float zOffset = (slot.gridPosition.x % 2 == 0) ? (rowStepZ * 0.5f) : 0f;
                cellCenter = originPos + new Vector3(slot.gridPosition.x * colStepX, 0,
                    slot.gridPosition.y * rowStepZ + zOffset);
            }
            else
            {
                cellCenter = gridObj != null
                    ? gridObj.GetCellCenterWorld(new Vector3Int(slot.gridPosition.x, slot.gridPosition.y, 0))
                    : new Vector3(slot.gridPosition.x * 1.732f + (slot.gridPosition.y % 2 != 0 ? 0.866f : 0f), 0,
                        slot.gridPosition.y * 1.5f);
            }

            bool isSelected = hasSelection && selectedSlotPos == slot.gridPosition;

            Color handleColor;
            if (isSelected)
                handleColor = Color.yellow;
            else if (!slot.isActive)
                handleColor = new Color(0.4f, 0.4f, 0.4f, 0.2f);
            else if (slot.slotType == SlotType.Block)
                handleColor = new Color(1f, 0.2f, 0.2f, 0.85f);
            else
                handleColor = new Color(0.2f, 0.9f, 0.3f, 0.8f);

            Handles.color = handleColor;
            float radius = gridObj != null ? gridObj.cellSize.x / 2f : 1.0f;
            DrawSceneHexWire(cellCenter, radius, isFlatMode);

            if (showGridLabels && slot.isActive)
            {
                string info = $"{slot.gridPosition.x},{slot.gridPosition.y}";
                if (slot.slotType == SlotType.Block) info += "\n[BLOCK]";
                else if (slot.lockType == LockType.BreakLock) info += $"\n🛡️{slot.breakHitCount}";
                else if (slot.lockType == LockType.TaskLock) info += $"\n🎯{slot.taskTargetScore}";

                GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = isSelected ? Color.yellow : Color.white },
                    fontSize = 11,
                    fontStyle = FontStyle.Bold
                };
                Handles.Label(cellCenter + Vector3.up * 0.15f, info, labelStyle);
            }

            if (show3DScenePreview && slot.isActive && slot.hasStack && slot.stackColors != null &&
                slot.stackColors.Count > 0)
            {
                Draw3DStackPreview(cellCenter, slot.stackColors, radius * 0.85f, isFlatMode);
            }
        }
    }

    private void DrawSceneHexWire(Vector3 center, float radius, bool isFlatTopped)
    {
        Vector3[] vertices = new Vector3[6];
        float angleOffset = isFlatTopped ? 0f : (Mathf.PI / 6f);
        for (int i = 0; i < 6; i++)
        {
            float angle = (i * Mathf.PI * 2f / 6f) + angleOffset;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            vertices[i] = center + new Vector3(x, 0.02f, z);
        }

        for (int i = 0; i < 6; i++)
        {
            Handles.DrawLine(vertices[i], vertices[(i + 1) % 6]);
        }
    }

    private void Draw3DStackPreview(Vector3 basePos, List<Color> colors, float radius, bool isFlatTopped)
    {
        float layerHeight = 0.2f;
        float angleOffset = isFlatTopped ? 0f : (Mathf.PI / 6f);

        for (int i = 0; i < colors.Count; i++)
        {
            Vector3 layerCenter = basePos + Vector3.up * (0.05f + i * layerHeight);
            Handles.color = colors[i];

            Vector3[] verts = new Vector3[6];
            for (int v = 0; v < 6; v++)
            {
                float angle = (v * Mathf.PI * 2f / 6f) + angleOffset;
                verts[v] = layerCenter + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            }

            Handles.DrawAAConvexPolygon(verts);
            Handles.color = Color.black;
            for (int v = 0; v < 6; v++)
            {
                Handles.DrawLine(verts[v], verts[(v + 1) % 6]);
            }
        }
    }

    #endregion
}
#endif