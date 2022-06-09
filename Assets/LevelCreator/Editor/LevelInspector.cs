using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(Level))]
public class LevelInspector : Editor
{
    public enum Mode {
        View,
        Paint,
        Edit,
        EditWalkArea
    }

    private Mode currentMode;
    private Mode selectedMode;

    private PaletteItem.Category currentCategory;
    private PaletteItem.Category selectedCategory;

    private List<string> modeLabels;
    private List<string> categoryLabels;

    private Level level;
    private int newTotalColumns;
    private int newTotalRows;

    private PaletteItem paletteItemSelected;
    private PaletteItem paletteItemEdited; 
    private Texture2D itemPreview;
    private SpriteRenderer spriteRendererInspected;

    private float alpha = 1f;

    private int originalPosX;
    private int originalPosY;

    private void OnEnable() {
        level = target as Level;
        level.transform.hideFlags = HideFlags.NotEditable;
        ResetResizeValues();
        InitMode();
        PaletteWindow.ItemSelectedEvent += UpdateCurrentPieceInstance;
    }

    private void OnDisable() {
        PaletteWindow.ItemSelectedEvent -= UpdateCurrentPieceInstance;
    }

    #region Init
    private void ResetResizeValues() {
        newTotalColumns = level.TotalColumns;
        newTotalRows = level.TotalRows;
    }

    private void InitMode() {
        categoryLabels = EditorUtils.GetEnumName(level.Categories);
        List<Mode> modes = MyTools.GetListFromEnum<Mode>();
        modeLabels = EditorUtils.GetEnumName(modes);
    }

    private void UpdateCurrentPieceInstance(PaletteItem item, Texture2D preview) {
        paletteItemSelected = item;
        itemPreview = preview;
        Repaint();
    }
    #endregion

    #region OnInspectorGUI
    public override void OnInspectorGUI() {
        DrawLevelSizeGUI();
        DrawPieceSelectedGUI();
        DrawPieceEditedGUI();
    }

    private void DrawLevelSizeGUI() {
        EditorGUILayout.LabelField("Size", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope("box")) {
            using (new EditorGUILayout.VerticalScope()) {
                newTotalColumns = EditorGUILayout.IntField("Columns", Mathf.Max(1, newTotalColumns));
                newTotalRows = EditorGUILayout.IntField("Rows", Mathf.Max(1, newTotalRows));
            }

            using (new EditorGUILayout.VerticalScope()) {
                bool oldEnabled = GUI.enabled;
                GUI.enabled = newTotalColumns != level.TotalColumns || newTotalRows != level.TotalRows;
                if (GUILayout.Button("Resize", GUILayout.Height(2 * EditorGUIUtility.singleLineHeight))) {
                    if (EditorUtility.DisplayDialog("Level Creator", "是否重设关卡长宽？", "是的", "取消")) {
                        ResizeLevel();
                    }
                    GUIUtility.ExitGUI();
                }
                if (GUILayout.Button("Reset")) {
                    ResetResizeValues();
                }
                GUI.enabled = oldEnabled;
            }
        }
    }

    private void ResizeLevel() {
        int[] newWalkArea = new int[newTotalColumns * newTotalRows];
        for (int i = 0; i < level.TotalColumns; i++) {
            for (int j = 0; j < level.TotalRows; j++) {
                int currentIndex = i + j * level.TotalColumns;
                if (i < newTotalColumns && j < newTotalRows) {
                    int newIndex = i + j * newTotalColumns;
                    newWalkArea[newIndex] = level.WalkArea[currentIndex];
                }
            }
        }

        var categories = new List<PaletteItem.Category>(level.CategoryPieces.Keys);
        for (int c = 0; c < categories.Count; c++) {
            PaletteItem.Category key = categories[c];
            SpriteRenderer[] current = level.CategoryPieces[key];
            SpriteRenderer[] newOne = new SpriteRenderer[newTotalColumns * newTotalRows];
            for (int i = 0; i < level.TotalColumns; i++) {
                for (int j = 0; j < level.TotalRows; j++) {
                    int index = i + j * level.TotalColumns;
                    if (i < newTotalColumns && j < newTotalRows) {
                        int newIndex = i + j * newTotalColumns;
                        newOne[newIndex] = current[index];
                    } else {
                        SpriteRenderer piece = current[index];
                        if (piece != null) {
                            DestroyImmediate(piece.gameObject);
                        }
                        RemoveTileOffset(key, index);
                    }
                }
            }
            level.CategoryPieces[key] = newOne;
        }
        level.WalkArea = newWalkArea;
        level.TotalColumns = newTotalColumns;
        level.TotalRows = newTotalRows;
        level.Save();
        SceneView.RepaintAll();
    }

    private void DrawPieceSelectedGUI() {
        EditorGUILayout.LabelField("Piece Selected", EditorStyles.boldLabel);
        if (paletteItemSelected == null) {
            EditorGUILayout.HelpBox("No piece selected!", MessageType.Info);
        } else {
            using (new EditorGUILayout.VerticalScope("box")) {
                EditorGUILayout.LabelField(new GUIContent(itemPreview), GUILayout.Height(40));
                EditorGUILayout.LabelField(paletteItemSelected.itemName);
            }
        }
    }

    private void DrawPieceEditedGUI() {
        if (currentMode != Mode.Edit) {
            return;
        }
        EditorGUILayout.LabelField("Piece Edited", EditorStyles.boldLabel);
        if (paletteItemEdited != null) {
            using (new EditorGUILayout.VerticalScope("box")) {
                EditorGUILayout.LabelField("Name: " + paletteItemEdited.itemName);
            }
        } else {
            EditorGUILayout.HelpBox("No piece to edit!", MessageType.Info);
        }
    }
    #endregion

    #region OnSceneGUI
    // 1.handles the events from the Scene View
    // 2.you must place your GUI code between BeginGUI and EndGUI
    private void OnSceneGUI() {
        DrawModeGUI();
        ModeHandler();
        EventHandler();
        DrawAlphaGUI();
        DrawPaletteItemCategoryGUI();
        DrawSaveAndLoadGUI();
        DrawShowGridGUI();
    }

    private void DrawModeGUI() {
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10f, 10f, 500f, 40f));
        selectedMode = (Mode)GUILayout.Toolbar((int)currentMode, modeLabels.ToArray(), GUILayout.ExpandHeight(true));
        GUILayout.EndArea();
        Handles.EndGUI();
    }

    private void ModeHandler() {
        Tools.current = selectedMode switch {
            Mode.Paint or Mode.Edit or Mode.EditWalkArea => Tool.None,
            _ => Tool.View,
        };
        if (selectedMode != currentMode) {
            currentMode = selectedMode;
            paletteItemEdited = null;
            Repaint();
        }
        level.ShowWalkArea = selectedMode == Mode.EditWalkArea;
        SceneView.currentDrawingSceneView.in2DMode = true;
    }

    private void EventHandler() {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        Vector3 mousePosition = Event.current.mousePosition;
        if (IsMouseOverUI(mousePosition)) {
            return;
        }
        Camera camera = SceneView.currentDrawingSceneView.camera;
        mousePosition = new Vector2(mousePosition.x, camera.pixelHeight - mousePosition.y);
        Vector3 worldPos = camera.ScreenToWorldPoint(mousePosition);
        Vector3Int gridPos = level.WorldToGridCoordinates(worldPos);
        int col = gridPos.x;
        int row = gridPos.y;
        switch (currentMode) {
            case Mode.View:
                break;
            case Mode.Paint:
                if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) {
                    Paint(col, row);
                }
                break;
            case Mode.Edit:
                if (Event.current.type == EventType.MouseDown) {
                    Edit(col, row);
                    originalPosX = col;
                    originalPosY = row;
                }
                if (spriteRendererInspected != null) {
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 1) {
                        spriteRendererInspected.transform.position = level.GridToWorldCoordinates(originalPosX, originalPosY);
                        RemoveTileOffset(currentCategory, originalPosX, originalPosY);
                    } else {
                        if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.Ignore) {
                            Move();
                        }
                        spriteRendererInspected.transform.position = Handles.FreeMoveHandle(
                            spriteRendererInspected.transform.position,
                            Quaternion.identity,
                            Level.GRID_CELL_SIZE / 2,
                            Level.GRID_CELL_SIZE / 2 * Vector3.one,
                            Handles.RectangleHandleCap
                        );
                    }
                }
                break;
            case Mode.EditWalkArea:
                if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) {
                    EditWalkArea(col, row);
                }
                break;
            default:
                break;
        }
    }

    private bool IsMouseOverUI(Vector3 mousePosition) =>
        new Rect(10f, 10f, 500f, 40f).Contains(mousePosition)
        || new Rect(Screen.safeArea.width - 500, 50, 480, 100).Contains(mousePosition)
        || new Rect(Screen.safeArea.width - 150f, 160f, 100f, 80f).Contains(mousePosition)
        || new Rect(Screen.safeArea.width - 500, 10, 480, 100).Contains(mousePosition)
        || new Rect(Screen.safeArea.width - 150f, 300f, 100f, 80f).Contains(mousePosition);

    private void Paint(int col, int row) {
        if (!level.IsInsideGridBounds(col, row)) {
            return;
        }

        int buttonValue = Event.current.button;
        if (buttonValue == 1) {
            Event.current.Use();
            DestoryGameObject(col, row);
            RemoveTileOffset(currentCategory, col, row);
        } else if (buttonValue == 0) {
            if (paletteItemSelected == null) {
                return;
            }
            DestoryGameObject(col, row);
            var go = new GameObject();
            SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = paletteItemSelected.sprite;
            go.transform.SetParent(level.Layers[(int)currentCategory].transform);
            int index = col + row * level.TotalColumns;
            go.name = string.Format("{0}|[{1}, {2}] [{3}], {4}", index, col, row, paletteItemSelected.itemName, currentCategory);
            go.transform.position = level.GridToWorldCoordinates(col, row);
            //go.hideFlags = HideFlags.HideInHierarchy;
            level.CategoryPieces[currentCategory][col + row * level.TotalColumns] = spriteRenderer;
            level.SortTiles(currentCategory);
        }
    }

    private void Edit(int col, int row) {
        if (!level.IsInsideGridBounds(col, row) || level.CategoryPieces[currentCategory][col + row * level.TotalColumns] == null) {
            paletteItemEdited = null;
            spriteRendererInspected = null;
        } else {
            spriteRendererInspected = level.CategoryPieces[currentCategory][col + row * level.TotalColumns];
            Sprite sprite = spriteRendererInspected.sprite;
            paletteItemEdited = EditorUtils.GetScriptableObjectAsset<PaletteCollection>("Assets/Resources/PaletteCollection.asset").GetPaletteItemBySprite(sprite);
        }
        Repaint();
    }

    private void Move() {
        Vector3Int gridPoint = level.WorldToGridCoordinates(spriteRendererInspected.transform.position);
        int col = gridPoint.x;
        int row = gridPoint.y;
        if (col == originalPosX && row == originalPosY) {
            if (spriteRendererInspected.transform.position != level.GridToWorldCoordinates(originalPosX, originalPosY)) {
                RemoveTileOffset(currentCategory, col, row);
                Vector3 offset = spriteRendererInspected.transform.position - level.GridToWorldCoordinates(col, row);
                AddTileOffset(currentCategory, col, row, offset);
            }
            return;
        }

        RemoveTileOffset(currentCategory, originalPosX, originalPosY);
        if (!level.IsInsideGridBounds(col, row) || level.CategoryPieces[currentCategory][col + row * level.TotalColumns] != null) {
            spriteRendererInspected.transform.position = level.GridToWorldCoordinates(originalPosX, originalPosY);
        } else {
            level.CategoryPieces[currentCategory][originalPosX + originalPosY * level.TotalColumns] = null;
            level.CategoryPieces[currentCategory][col + row * level.TotalColumns] = spriteRendererInspected;
            //spriteRendererInspected.transform.position = level.GridToWorldCoordinates(col, row);
            Vector3 offset = spriteRendererInspected.transform.position - level.GridToWorldCoordinates(col, row);
            AddTileOffset(currentCategory, col, row, offset);
        }
    }

    private void EditWalkArea(int col, int row) {
        int buttonValue = Event.current.button;
        if (buttonValue == 1) {
            Event.current.Use();
        }
        if ((buttonValue == 0 || buttonValue == 1) && level.IsInsideGridBounds(col, row)) {
            level.WalkArea[col + row * level.TotalColumns] = 1 - buttonValue;
            SceneView.RepaintAll();
        }
    }

    private void DrawAlphaGUI() {
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(Screen.safeArea.width - 500, 10, 480, 100));
        using (new EditorGUILayout.HorizontalScope("box")) {
            GUILayout.Label("全局alpha值", GUILayout.MaxWidth(150));
            float lastAlpha = alpha;
            alpha = GUILayout.HorizontalSlider(alpha, 0f, 1f);
            if (lastAlpha != alpha) {
                RefreshSpritesAlpha();
            }
        }
        GUILayout.EndArea();
        Handles.EndGUI();
    }

    private void RefreshSpritesAlpha() {
        var fullColor = new Color(1f, 1f, 1f, 1f);
        var alphaColor = new Color(1f, 1f, 1f, alpha);
        foreach (KeyValuePair<PaletteItem.Category, SpriteRenderer[]> item in level.CategoryPieces) {
            Color curColor = item.Key == currentCategory ? fullColor : alphaColor;
            foreach (SpriteRenderer sr in item.Value) {
                if (sr != null) {
                    sr.color = curColor;
                }
            }
        }
    }

    private void DrawPaletteItemCategoryGUI() {
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(Screen.safeArea.width - 500, 50, 480, 100));
        selectedCategory = (PaletteItem.Category)GUILayout.Toolbar((int)currentCategory, categoryLabels.ToArray(), GUILayout.ExpandHeight(true));
        GUILayout.EndArea();
        Handles.EndGUI();
        if (currentCategory != selectedCategory) {
            currentCategory = selectedCategory;
            RefreshSpritesAlpha();
            Repaint();
        }
        level.CurrentCategory = currentCategory;
    }

    private void DrawSaveAndLoadGUI() {
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(Screen.safeArea.width - 150f, 160f, 100f, 100f));
        using (new EditorGUILayout.VerticalScope("box")) {
            if (GUILayout.Button("保存关卡", GUILayout.MaxHeight(40))) {
                level.Save();
            }
            GUILayout.Space(20);
            if (GUILayout.Button("关闭", GUILayout.MaxHeight(40))) {
                Selection.activeGameObject = GameObject.Find("LevelMainMenu");
                DestroyImmediate(level.gameObject);
            }
        }
        GUILayout.EndArea();
        Handles.EndGUI();
    }

    private void DrawShowGridGUI() {
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(Screen.safeArea.width - 150f, 300f, 100f, 80f));
        using (new EditorGUILayout.VerticalScope("box")) {
            level.ShowGrid = GUILayout.Toggle(level.ShowGrid, "显示网格");
        }
        GUILayout.EndArea();
        Handles.EndGUI();
    }
    #endregion

    private void DestoryGameObject(int col, int row) {
        SpriteRenderer sr = level.CategoryPieces[currentCategory][col + row * level.TotalColumns];
        if (sr != null) {
            DestroyImmediate(sr.gameObject);
        }
    }

    private void AddTileOffset(PaletteItem.Category category, int col, int row, Vector3 offset) {
        var newOffset = new TileOffset {
            index = col + row * level.TotalColumns,
            x = offset.x,
            y = offset.y
        };
        TileOffset old = GetTileOffset(category, col + row * level.TotalColumns);
        Debug.Assert(old == null, "old offset tile != null");
        level.AllOffsets[currentCategory].Add(newOffset);
    }

    private void RemoveTileOffset(PaletteItem.Category category, int col, int row) {
        int index = col + row * level.TotalColumns;
        RemoveTileOffset(category, index);
    }

    private void RemoveTileOffset(PaletteItem.Category category, int index) {
        TileOffset tileOffset = GetTileOffset(category, index);
        if (tileOffset != null) {
            level.AllOffsets[category].Remove(tileOffset);
        }
    }

    private TileOffset GetTileOffset(PaletteItem.Category category, int index) =>
        level.AllOffsets[category].Where(t => t.index == index).FirstOrDefault();

}
