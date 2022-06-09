﻿using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelMainMenu))]
public class LevelMainMenuInspector : Editor
{
    private void OnSceneGUI() {
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(Screen.safeArea.width / 2 - 75f, Screen.safeArea.height / 2 - 75f, 150f, 150f));
        using (new EditorGUILayout.VerticalScope()) {
            if (GUILayout.Button("创 建 新 关 卡", GUILayout.MaxHeight(60))) {
                NewLevelWindow.CreateNewLevel();
            }
            GUILayout.Space(40);
            if (GUILayout.Button("读 取 关 卡", GUILayout.MaxHeight(60))) {
                Level oldLevel = FindObjectOfType<Level>();
                if (oldLevel != null) {
                    DestroyImmediate(oldLevel.gameObject);
                }
                LoadLevelWindow.LoadLevel();
            }
        }
        GUILayout.EndArea();
        Handles.EndGUI();
    }
}
