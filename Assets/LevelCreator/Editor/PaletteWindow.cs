using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PaletteWindow : EditorWindow
{
    public delegate void itemSelectedDelegate(PaletteItem item, Texture2D preview);
    public static event itemSelectedDelegate ItemSelectedEvent;

    private List<PaletteItem.Category> categories; // 类别
    private List<string> categoryLabels; // 类别文本
    private PaletteItem.Category categorySelected; // 当前选中类别

    private string path = "Assets/Resources/MapRes/";
    private List<PaletteItem> items; // 所有item
    private Dictionary<PaletteItem.Category, List<PaletteItem>> categorizedItems; // 将所有item分类
    private Dictionary<PaletteItem, Texture2D> previews; // 每个item对应的预览
    private Vector2 scrollPosition;
    private const float BUTTON_WIDTH = 80;
    private const float BUTTON_HEIGHT = 90;

    public static void ShowPalette() => GetWindow<PaletteWindow>("Palette");

    private void OnEnable() {
        if (categories == null) {
            InitCategories();
        }
        if (categorizedItems == null) {
            InitContent();
        }
    }

    private void Update() {
        if (previews.Count != items.Count) {
            GeneratePreviews();
        }
    }

    private void InitCategories() {
        categories = MyTools.GetListFromEnum<PaletteItem.Category>();
        categoryLabels = EditorUtils.GetEnumName(categories);
    }

    private void InitContent() {
        Debug.Log("init content");
        items = new List<PaletteItem>();
        categorizedItems = new Dictionary<PaletteItem.Category, List<PaletteItem>>();
        previews = new Dictionary<PaletteItem, Texture2D>();

        PaletteCollection paletteCollection = EditorUtils.GetScriptableObjectAsset<PaletteCollection>("Assets/Resources/PaletteCollection.asset");
        foreach (PaletteItem.Category category in categories) {
            List<Sprite> sprites = EditorUtils.GetAssets<Sprite>(path + category);
            var paletteItems = new List<PaletteItem>();
            for (int i = 0; i < sprites.Count; i++) {
                PaletteItem pItem = paletteCollection.GetPaletteItemBySprite(sprites[i]);
                if (pItem == null) {
                    Debug.LogError("pItem == null, i = " + i + ", paletteCollection.list.Count = " + paletteCollection.list.Count);
                    continue;
                }
                paletteItems.Add(pItem);
                items.Add(pItem);
            }
            categorizedItems.Add(category, paletteItems);
        }
    }

    private void OnGUI() {
        DrawTabs();
        DrawScroll();
    }

    private void DrawTabs() {
        int index = (int)categorySelected;
        index = GUILayout.Toolbar(index, categoryLabels.ToArray());
        categorySelected = categories[index];
    }

    private void DrawScroll() {
        if (categorizedItems[categorySelected].Count == 0) {
            EditorGUILayout.HelpBox("This category is empty!", MessageType.Info);
            return;
        }
        int rowCapacity = Mathf.FloorToInt(position.width / BUTTON_WIDTH);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        int selectionGridIndex = -1;
        selectionGridIndex = GUILayout.SelectionGrid(selectionGridIndex, GetGUIContentsFromItems(), rowCapacity, GetGUIStyle());
        GetSelectedItem(selectionGridIndex);
        GUILayout.EndScrollView();
    }

    private GUIContent[] GetGUIContentsFromItems() {
        var guiContents = new List<GUIContent>();
        if (previews.Count == items.Count) {
            int totalItems = categorizedItems[categorySelected].Count;
            for (int i = 0; i < totalItems; i++) {
                var guiContent = new GUIContent();
                PaletteItem paletteItem = categorizedItems[categorySelected][i];
                guiContent.text = paletteItem.itemName;
                guiContent.image = previews[paletteItem];
                guiContents.Add(guiContent);
            }
        }
        return guiContents.ToArray();
    }

    private GUIStyle GetGUIStyle() {
        var guiStyle = new GUIStyle(GUI.skin.button) {
            alignment = TextAnchor.LowerCenter,
            imagePosition = ImagePosition.ImageAbove,
            fixedWidth = BUTTON_WIDTH,
            fixedHeight = BUTTON_HEIGHT
        };
        return guiStyle;
    }

    private void GetSelectedItem(int index) {
        if (index != -1) {
            PaletteItem selectedItem = categorizedItems[categorySelected][index];
            ItemSelectedEvent?.Invoke(selectedItem, previews[selectedItem]);
        }
    }

    private void GeneratePreviews() {
        foreach (PaletteItem item in items) {
            if (item == null) {
                continue;
            }
            if (!previews.ContainsKey(item)) {
                Texture2D preview = AssetPreview.GetAssetPreview(item.sprite);
                if (preview != null) {
                    previews.Add(item, preview);
                }
            }
        }
    }
}
