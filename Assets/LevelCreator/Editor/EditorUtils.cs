using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public static class EditorUtils {

    public static List<T> GetAssets<T>(string path) where T : Object {
        var result = new List<T>();
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new string[] { path });
        for (int i = 0; i < guids.Length; i++) {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            Object[] asset = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int j = 0; j < asset.Length; j++) {
                Object item = asset[j];
                T child = item as T;
                if (child != null) {
                    result.Add(child);
                }
            }
        }
        return result;
    }

    public static List<string> GetEnumName<T>(List<T> enums) {
        var result = new List<string>();
        foreach (T item in enums) {
            result.Add(item.ToString());
        }
        return result;
    }

    public static T GetScriptableObjectAsset<T>(string path) where T : ScriptableObject {
        T item = AssetDatabase.LoadAssetAtPath<T>(path);
        if (item == null) {
            T newItem = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(newItem, path);
            return newItem;
        }
        return item;
    }

    public static PaletteItem GetPaletteItemBySprite(this PaletteCollection paletteCollection, Sprite sprite) {
        return paletteCollection.list.Where(t => t.sprite == sprite).FirstOrDefault();
    }
}
