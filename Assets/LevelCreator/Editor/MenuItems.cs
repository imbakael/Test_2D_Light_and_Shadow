using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class MenuItems {

    [MenuItem("Tools/Level Creator/Show Palette #_p")]
    public static void ShowPalette() {
        PaletteWindow.ShowPalette();
    }

    [MenuItem("Tools/Create PaletteItems")]
    public static void CreateAsset() {
        PaletteCollection paletteCollection = EditorUtils.GetScriptableObjectAsset<PaletteCollection>("Assets/Resources/PaletteCollection.asset");
        List<PaletteItem.Category> categories = MyTools.GetListFromEnum<PaletteItem.Category>();
        foreach (PaletteItem.Category item in categories) {
            List<Sprite> sprites = EditorUtils.GetAssets<Sprite>("Assets/Resources/MapRes/" + item);
            for (int i = 0; i < sprites.Count; i++) {
                Sprite sprite = sprites[i];
                PaletteItem p = paletteCollection.GetPaletteItemBySprite(sprite);
                if (p == null) {
                    var paletteItem = ScriptableObject.CreateInstance<PaletteItem>();
                    paletteItem.sprite = sprites[i];
                    paletteItem.category = item;
                    paletteItem.itemName = sprites[i].name;
                    AssetDatabase.CreateAsset(paletteItem, "Assets/Resources/MapTiles/" + paletteItem.itemName + ".asset");
                    paletteCollection.list.Add(paletteItem);
                }
            }
        }
        EditorUtility.SetDirty(paletteCollection);
    }

    [MenuItem("Test/Just Test")]
    public static void JustTest() {
        PaletteCollection p = EditorUtils.GetScriptableObjectAsset<PaletteCollection>("Assets/Resources/PaletteCollection.asset");
        PaletteItem first = p.list[0];
        string path = AssetDatabase.GetAssetPath(first);
        Debug.Log("first palitem.path = " + path);
        Debug.Log("guid ==    " + AssetDatabase.AssetPathToGUID(path));
        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(first, out string guid, out long localId)) {
            Debug.Log("111 guid ==    " + guid + ", localId = " + localId);
        }
    }
}
