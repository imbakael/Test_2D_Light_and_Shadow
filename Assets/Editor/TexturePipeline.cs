using UnityEngine;
using UnityEditor;

public class TexturePipeline : AssetPostprocessor {

    private void OnPreprocessTexture() {
        Debug.LogFormat("OnPrepTexture, The path is {0}", assetPath);
        if (assetPath.StartsWith("Assets/AnimationRes/BuildAnimation")) {
            PreprocessBg();
        } else if (assetPath.StartsWith("Assets/Resources/MapRes")) {
            PreprocessMapRes();
        }
    }

    private void PreprocessMapRes() {
        TextureImporter importer = assetImporter as TextureImporter;
        if (importer == null) {
            return;
        }
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 64;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        var textureSettings = new TextureImporterSettings();
        importer.ReadTextureSettings(textureSettings);
        textureSettings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
        textureSettings.mipmapEnabled = false;
        textureSettings.filterMode = FilterMode.Point;
        importer.SetTextureSettings(textureSettings);
    }

    //private void OnPostprocessTexture(Texture2D texture) {
    //    Debug.LogFormat("OnPostTexture, The path is {0}", assetPath);
    //}

    private void PreprocessBg() {
        TextureImporter importer = assetImporter as TextureImporter;
        if (importer == null) {
            return;
        }
        importer.maxTextureSize = 256;
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 64;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        var textureSettings = new TextureImporterSettings();
        importer.ReadTextureSettings(textureSettings);
        textureSettings.spriteAlignment = (int)SpriteAlignment.Center;
        textureSettings.mipmapEnabled = false;
        textureSettings.filterMode = FilterMode.Point;
        importer.SetTextureSettings(textureSettings);
    }

}
