using System.IO;
using UnityEditor;
using UnityEngine;

public class LoadLevelWindow : ScriptableWizard {

    private FileInfo[] fileInfos;

    public static void LoadLevel() {
        DisplayWizard<LoadLevelWindow>("读取地图", "读取");
    }

    private void OnEnable() {
        string path = Application.persistentDataPath + Level.DIRECTORY;
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
        var folder = new DirectoryInfo(path);
        fileInfos = folder.GetFiles("*.txt");
    }

    private void OnGUI() {
        for (int i = 0; i < fileInfos.Length; i++) {
            FileInfo fileInfo = fileInfos[i];
            if (GUILayout.Button(fileInfo.Name)) {
                Load(fileInfo.Name);
                Close();
            }
        }
    }

    private void Load(string fileName) {
        var go = new GameObject("Level");
        var level = go.AddComponent<Level>();
        level.InitLevel(fileName);
        Selection.activeObject = go;
    }
}
