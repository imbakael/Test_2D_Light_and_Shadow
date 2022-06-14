using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class Level : MonoBehaviour
{
    public const string DIRECTORY = "/SaveData/";
    public const float GRID_CELL_SIZE = 1f;

    public PaletteItem.Category CurrentCategory { get; set; } = PaletteItem.Category.Ground;
    public Transform[] Layers { get; set; }
    public int TotalColumns { get; set; } = 15;
    public int TotalRows { get; set; } = 10;
    public Dictionary<PaletteItem.Category, SpriteRenderer[]> CategoryPieces { get; set; }
    public Dictionary<PaletteItem.Category, List<TileOffset>> AllOffsets { get; set; }
    private List<PaletteItem.Category> categories;
    public List<PaletteItem.Category> Categories {
        get {
            if (categories == null) {
                categories = MyTools.GetListFromEnum<PaletteItem.Category>();
            }
            return categories;
        }
    }
    public bool ShowGrid { get; set; } = true;
    public int[] WalkArea { get; set; }
    public bool ShowWalkArea { get; set; } = false;

    private string fileName = "";
    private readonly Color normalColor = Color.white;
    private readonly Color selectedColor = Color.yellow;
    private readonly Color canWalkColor = new Color(0, 255, 255);

    #region DrawGridGizmos
    private void OnDrawGizmos() {
        if (ShowGrid) {
            var oldColor = Gizmos.color;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.color = normalColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            GridGizmo(TotalColumns, TotalRows);
            GridBorderGizmo(TotalColumns, TotalRows);
            Gizmos.color = oldColor;
            Gizmos.matrix = oldMatrix;
        }

        if (CategoryPieces != null && CategoryPieces.Count > 0) {
            var oldColor = Gizmos.color;
            Gizmos.color = Color.green;
            foreach (SpriteRenderer sr in CategoryPieces[CurrentCategory]) {
                if (sr != null) {
                    Vector3 position = sr.transform.position;
                    Vector3Int gridPos = WorldToGridCoordinates(position);
                    GridTileBorderGizmo(gridPos.x, gridPos.y);
                }
            }
            Gizmos.color = oldColor;
        }

        if (ShowWalkArea && WalkArea != null && WalkArea.Length > 0) {
            var oldColor = Gizmos.color;
            for (int i = 0; i < TotalColumns; i++) {
                for (int j = 0; j < TotalRows; j++) {
                    int currentIndex = i + j * TotalColumns;
                    int value = WalkArea[currentIndex];
                    Vector3 pos = GridToWorldCoordinates(i, j);
                    Gizmos.color = value == 0 ? Color.red : canWalkColor;
                    Gizmos.DrawWireCube(pos, 0.5f * GRID_CELL_SIZE * Vector3.one);
                }
            }
            Gizmos.color = oldColor;
        }
    }

    private void OnDrawGizmosSelected() {
        var oldColor = Gizmos.color;
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.color = selectedColor;
        Gizmos.matrix = transform.localToWorldMatrix;
        GridBorderGizmo(TotalColumns, TotalRows);
        Gizmos.color = oldColor;
        Gizmos.matrix = oldMatrix;
    }

    private void GridBorderGizmo(int col, int row) {
        Gizmos.DrawLine(Vector3.zero, new Vector3(0, row * GRID_CELL_SIZE, 0));
        Gizmos.DrawLine(Vector3.zero, new Vector3(col * GRID_CELL_SIZE, 0, 0));
        Gizmos.DrawLine(new Vector3(col * GRID_CELL_SIZE, row * GRID_CELL_SIZE, 0), new Vector3(0, row * GRID_CELL_SIZE, 0));
        Gizmos.DrawLine(new Vector3(col * GRID_CELL_SIZE, row * GRID_CELL_SIZE, 0), new Vector3(col * GRID_CELL_SIZE, 0, 0));
    }

    private void GridGizmo(int col, int row) {
        for (int i = 1; i < col; i++) {
            float x = i * GRID_CELL_SIZE;
            float y = row * GRID_CELL_SIZE;
            Gizmos.DrawLine(new Vector3(x, 0, 0), new Vector3(x, y, 0));
        }
        for (int i = 1; i < row; i++) {
            float x = col * GRID_CELL_SIZE;
            float y = i * GRID_CELL_SIZE;
            Gizmos.DrawLine(new Vector3(0, y, 0), new Vector3(x, y, 0));
        }
    }

    private void GridTileBorderGizmo(int col, int row) {
        var leftDown = new Vector3(col * GRID_CELL_SIZE, row * GRID_CELL_SIZE, 0);
        var leftUp = new Vector3(col * GRID_CELL_SIZE, (row + 1) * GRID_CELL_SIZE, 0);
        var rightDown = new Vector3((col + 1) * GRID_CELL_SIZE, row * GRID_CELL_SIZE, 0);
        var rightUp = new Vector3((col + 1) * GRID_CELL_SIZE, (row + 1) * GRID_CELL_SIZE, 0);
        Gizmos.DrawLine(leftDown, leftUp);
        Gizmos.DrawLine(leftDown, rightDown);
        Gizmos.DrawLine(rightUp, leftUp);
        Gizmos.DrawLine(rightUp, rightDown);
    }
    #endregion

    #region Tool function
    public Vector3Int WorldToGridCoordinates(Vector3 point) {
        float deltaX = point.x - transform.position.x;
        float deltaY = point.y - transform.position.y;
        int x = Mathf.FloorToInt(deltaX / GRID_CELL_SIZE);
        int y = Mathf.FloorToInt(deltaY / GRID_CELL_SIZE);
        return new Vector3Int(x, y, 0);
    }

    public Vector3 GridToWorldCoordinates(int col, int row) {
        float x = transform.position.x + col * GRID_CELL_SIZE + GRID_CELL_SIZE * 0.5f;
        float y = transform.position.y + row * GRID_CELL_SIZE + GRID_CELL_SIZE * 0.5f;
        return new Vector3(x, y, 0);
    }

    public bool IsInsideGridBounds(Vector3 point) {
        float minX = transform.position.x;
        float maxX = minX + TotalColumns * GRID_CELL_SIZE;
        float minY = transform.position.y;
        float maxY = minY + TotalRows * GRID_CELL_SIZE;
        return point.x >= minX && point.x <= maxX && point.y >= minY && point.y <= maxY;
    }

    public bool IsInsideGridBounds(int col, int row) => col >= 0 && col < TotalColumns && row >= 0 && row < TotalRows;
    #endregion

    public void InitLevel(string fileName) {
        this.fileName = fileName;
        for (int i = 0; i < Categories.Count; i++) {
            string name = Categories[i].ToString();
            var child = new GameObject(name);
            child.transform.SetParent(transform);
            SortingGroup sortingGroup = child.AddComponent<SortingGroup>();
            sortingGroup.sortingLayerName = "MySprites";
            sortingGroup.sortingOrder = i * 2;
        }
        transform.hideFlags = HideFlags.NotEditable;
        Load();
    }

    public void SortTiles(PaletteItem.Category category) {
        SpriteRenderer[] spriteRenderers = CategoryPieces[category];
        List<SpriteRenderer> sorts = spriteRenderers.Where(t => t != null).OrderByDescending(t => GetIndex(t.name)).ToList();
        for (int i = 0; i < sorts.Count; i++) {
            sorts[i].transform.SetSiblingIndex(i);
        }
    }

    private int GetIndex(string name) {
        string[] splits = name.Split('|');
        string first = splits[0];
        return Convert.ToInt32(first);
    }

    public void ChangeAlpha(float alpha) {
        var alphaColor = new Color(1f, 1f, 1f, alpha);
        foreach (KeyValuePair<PaletteItem.Category, SpriteRenderer[]> item in CategoryPieces) {
            foreach (SpriteRenderer sr in item.Value) {
                if (sr != null) {
                    sr.color = alphaColor;
                }
            }
        }
    }

    #region Save
    public void Save() {
        Debug.Log("Save!, fileName = " + fileName);
        string dir = Application.persistentDataPath + DIRECTORY;
        if (!Directory.Exists(dir)) {
            Directory.CreateDirectory(dir);
        }

        var saveItem = new SaveItem {
            levelId = 1,
            col = TotalColumns,
            row = TotalRows,
            walkArea = WalkArea,
            allTiles = GetAllTiles(),
            allOffsets = AllOffsets
        };

        string json = MyTools.GetSerializeJson(saveItem);
        File.WriteAllText(dir + fileName, json);
    }

    private Dictionary<PaletteItem.Category, string[]> GetAllTiles() {
        var result = new Dictionary<PaletteItem.Category, string[]>();
        if (CategoryPieces == null || CategoryPieces.Count == 0) {
            foreach (PaletteItem.Category item in Categories) {
                result.Add(item, new string[TotalColumns * TotalColumns]);
            }
            return result;
        }
        foreach (KeyValuePair<PaletteItem.Category, SpriteRenderer[]> item in CategoryPieces) {
            SpriteRenderer[] spriteRenderers = item.Value;
            result.Add(item.Key, GetSpriteGuids(spriteRenderers));
        }
        return result;
    }

    private string[] GetSpriteGuids(SpriteRenderer[] spriteRenders) {
        string[] result = new string[TotalColumns * TotalRows];
        PaletteCollection paletteCollection = AssetDatabase.LoadAssetAtPath<PaletteCollection>("Assets/Resources/PaletteCollection.asset");
        for (int i = 0; i < spriteRenders.Length; i++) {
            if (spriteRenders[i] != null) {
                Sprite sprite = spriteRenders[i].sprite;
                PaletteItem paletteItem = paletteCollection.list.Where(t => t.sprite == sprite).FirstOrDefault();
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(paletteItem, out string guid, out long localId)) {
                    result[i] = guid;
                } else {
                    Debug.LogError("未查找到guid");
                }
            }
        }
        return result;
    }
    #endregion

    #region Load
    private void Load() {
        Debug.Log("Load!, fileName = " + fileName);
        InitLayers();
        SaveItem saveItem = GetSaveItem();
        TotalColumns = saveItem.col;
        TotalRows = saveItem.row;
        WalkArea = saveItem.walkArea;
        AllOffsets = saveItem.allOffsets;
        CategoryPieces = new Dictionary<PaletteItem.Category, SpriteRenderer[]>();
        foreach (PaletteItem.Category item in Categories) {
            SpriteRenderer[] spriteRenderers = GetSriteRenders(saveItem.allTiles[item], item);
            CategoryPieces.Add(item, spriteRenderers);
            SortTiles(item);
        }
    }

    private void InitLayers() {
        int length = Categories.Count;
        Layers = new Transform[length];
        SortingGroup[] sortingGroup = transform.GetComponentsInChildren<SortingGroup>();
        Debug.Assert(length == sortingGroup.Length, "sortingGroup长度不等于层数");
        for (int i = 0; i < length; i++) {
            Layers[i] = sortingGroup[i].transform;
        }
    }

    private SaveItem GetSaveItem() {
        string fullPath = Application.persistentDataPath + DIRECTORY + fileName;
        if (File.Exists(fullPath)) {
            string json = File.ReadAllText(fullPath);
            return MyTools.DeserializeObject<SaveItem>(json);
        }
        return SaveItem.GetDefaultSaveItem(2, 2);
    }

    private SpriteRenderer[] GetSriteRenders(string[] guids, PaletteItem.Category category) {
        var result = new SpriteRenderer[guids.Length];
        for (int i = 0; i < TotalColumns; i++) {
            for (int j = 0; j < TotalRows; j++) {
                int index = i + j * TotalColumns;
                string guid = guids[index];
                if (!string.IsNullOrEmpty(guid)) {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    PaletteItem paletteItem = AssetDatabase.LoadAssetAtPath<PaletteItem>(path);
                    Debug.Assert(paletteItem != null, string.Format("位置 {0} 无法生成PaletteItem", path));
                    var go = new GameObject();
                    SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = paletteItem.sprite;
                    go.transform.SetParent(Layers[(int)category].transform);
                    go.name = string.Format("{0}|[{1}, {2}] [{3}], {4}", index, i, j, paletteItem.itemName, category);
                    TileOffset tileOffset = GetTileOffset(category, index);
                    go.transform.position = tileOffset == null ? GridToWorldCoordinates(i, j) : (GridToWorldCoordinates(i, j) + GetOffset(tileOffset));
                    //go.hideFlags = HideFlags.HideInHierarchy;
                    result[index] = spriteRenderer;
                }
            }
        }
        return result;
    }

    private TileOffset GetTileOffset(PaletteItem.Category category, int index) {
        List<TileOffset> tileOffsets = AllOffsets[category];
        return tileOffsets.Where(t => t.index == index).FirstOrDefault();
    }

    private Vector3 GetOffset(TileOffset tileOffset) => new Vector3(tileOffset.x, tileOffset.y, 0f);
    #endregion
}
