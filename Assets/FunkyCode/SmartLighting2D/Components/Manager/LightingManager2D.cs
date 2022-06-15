﻿using UnityEngine;
using FunkyCode.LightingSettings;

namespace FunkyCode {
    [ExecuteInEditMode]
    public class LightingManager2D : LightingMonoBehaviour {
        private static LightingManager2D instance;

        [SerializeField]
        public LightingCameras cameras = new LightingCameras();

        public int version = 0;
        public string version_string = "";

        public Profile setProfile;
        public Profile profile;

        private SceneView sceneView = new SceneView();

        // editor foldouts (avoid reseting after compiling script)
        public bool[] foldout_cameras = new bool[10];

        public bool[,] foldout_lightmapPresets = new bool[10, 10];
        public bool[,] foldout_lightmapMaterials = new bool[10, 10];

        // Sets Lighting Main Profile Settings for Lighting2D at the start of the scene
        private static bool initialized = false;

        public Camera GetCamera(int id) {
            if (cameras.Length <= id) {
                return null;
            }

            return cameras.Get(id).GetCamera();
        }

        public static void ForceUpdate() { }

        static public LightingManager2D Get() {
            if (instance != null) {
                return instance;
            }

            foreach (LightingManager2D manager in FindObjectsOfType(typeof(LightingManager2D))) {
                instance = manager;
                return instance;
            }

            // create new light manager
            var gameObject = new GameObject("Lighting Manager 2D");
            instance = gameObject.AddComponent<LightingManager2D>();
            instance.transform.position = Vector3.zero;
            instance.version = Lighting2D.VERSION;
            instance.version_string = Lighting2D.VERSION_STRING;

            return instance;
        }

        public void Awake() {
            if (cameras == null) {
                cameras = new LightingCameras();
            }

            if (Application.isPlaying) {
                Lighting2D.ProjectSettings.shaderPreview = ShaderPreview.Disabled;
            }

            CameraTransform.List.Clear();

            if (instance != null && instance != this) {
                switch (Lighting2D.ProjectSettings.managerInstance) {
                    case ManagerInstance.Static:
                    case ManagerInstance.DontDestroyOnLoad:

                        Debug.LogWarning("Smart Lighting2D: Lighting Manager duplicate was found, new instance destroyed.", gameObject);

                        foreach (LightingManager2D manager in FindObjectsOfType(typeof(LightingManager2D))) {
                            if (manager != instance) {
                                manager.DestroySelf();
                            }
                        }

                        return; // cancel initialization

                    case ManagerInstance.Dynamic:

                        instance = this;

                        Debug.LogWarning("Smart Lighting2D: Lighting Manager duplicate was found, old instance destroyed.", gameObject);

                        foreach (LightingManager2D manager in FindObjectsOfType(typeof(LightingManager2D))) {
                            if (manager != instance) {
                                manager.DestroySelf();
                            }
                        }

                        break;
                }
            }

            initialized = false;

            SetupProfile();

            if (Application.isPlaying) {
                if (Lighting2D.ProjectSettings.managerInstance == LightingSettings.ManagerInstance.DontDestroyOnLoad) {
                    DontDestroyOnLoad(instance.gameObject);
                }
            }
        }

        // use only late update?
        private void Update() {
            if (Lighting2D.disable) {
                return;
            }

            ForceUpdate(); // for late update method?

            if (profile != null) {
                if (Lighting2D.Profile != profile) {
                    Lighting2D.UpdateByProfile(profile);
                }
            }

            FixTransform();
        }

        public void FixTransform() {
            if (transform.lossyScale != Vector3.one) {
                Vector3 scale = Vector3.one;

                Transform parent = transform.parent;

                if (parent != null) {
                    scale.x /= parent.lossyScale.x;
                    scale.y /= parent.lossyScale.y;
                    scale.z /= parent.lossyScale.z;
                }

                transform.localScale = Vector3.one;
            }

            if (transform.position != Vector3.one) {
                transform.position = Vector3.zero;
            }

            if (transform.rotation != Quaternion.identity) {
                transform.rotation = Quaternion.identity;
            }
        }

        private void LateUpdate() {
            if (Lighting2D.disable) {
                return;
            }

            UpdateInternal();

            if (Lighting2D.Profile.qualitySettings.updateMethod == UpdateMethod.LateUpdate) {
                Rendering.Manager.Main.Render();
            }
        }

        public void SetupProfile() {
            if (!initialized) {
                initialized = true;

                Profile profile = Lighting2D.Profile;

                Lighting2D.UpdateByProfile(profile);

                Lighting2D.materials.Reset();
            }
        }

        public void UpdateInternal() {
            if (Lighting2D.disable) {
                return;
            }

            SetupProfile();

            Rendering.Manager.Main.InternalUpdate();
        }

        private void OnDisable() {
            sceneView.OnDisable();

            if (profile == null) {
                return;
            }

            if (Application.isPlaying && setProfile != profile && Lighting2D.Profile == profile) {
                Lighting2D.RemoveProfile();
            }
        }

        public void UpdateProfile() {
            if (setProfile == null) {
                setProfile = Lighting2D.ProjectSettings.Profile;
            }
            profile = Application.isPlaying ? Instantiate(setProfile) : setProfile;
        }

        private void OnEnable() {
            sceneView.OnEnable();

            foreach (OnRenderMode onRenderMode in UnityEngine.Object.FindObjectsOfType(typeof(OnRenderMode))) {
                onRenderMode.DestroySelf();
            }

            Scriptable.LightSprite2D.List.Clear();

            UpdateProfile();

            Rendering.Manager.Main.UpdateMaterials();

            Update();
            LateUpdate();
        }

        private void OnRenderObject() {
            if (Lighting2D.RenderingMode != RenderingMode.OnPostRender) {
                return;
            }

            foreach (LightMainBuffer2D buffer in LightMainBuffer2D.List) {
                Rendering.LightMainBuffer.DrawPost(buffer);
            }
        }

        private void OnDrawGizmos() {
            if (Lighting2D.ProjectSettings.gizmos.drawGizmos != EditorDrawGizmos.Always) {
                return;
            }

            DrawGizmos();
        }

        private void DrawGizmos() {
            if (!isActiveAndEnabled) {
                return;
            }

            UnityEngine.Gizmos.color = new Color(0, 1f, 1f);

            if (Lighting2D.ProjectSettings.gizmos.drawGizmosBounds == EditorGizmosBounds.Enabled) {
                for (int i = 0; i < cameras.Length; i++) {
                    CameraSettings cameraSetting = cameras.Get(i);

                    Camera camera = cameraSetting.GetCamera();

                    if (camera != null) {
                        Rect cameraRect = CameraTransform.GetWorldRect(camera);

                        GizmosHelper.DrawRect(transform.position, cameraRect);
                    }
                }
            }

            for (int i = 0; i < Scriptable.LightSprite2D.List.Count; i++) {
                Scriptable.LightSprite2D light = Scriptable.LightSprite2D.List[i];

                Rect rect = light.lightSpriteShape.GetWorldRect();

                UnityEngine.Gizmos.color = new Color(1f, 0.5f, 0.25f);

                GizmosHelper.DrawPolygon(light.lightSpriteShape.GetSpriteWorldPolygon(), transform.position);

                UnityEngine.Gizmos.color = new Color(0, 1f, 1f);
                GizmosHelper.DrawRect(transform.position, rect);
            }
        }
    }
}



/*

for(int i = 0; i < cameras.Length; i++)
{
    CameraSettings cameraSetting = cameras.Get(i);

    for(int b = 0; b < cameraSetting.Lightmaps.Length; b++)
    {
        CameraLightmap cameraLightmap = cameraSetting.GetLightmap(b);

        foreach(Material material in cameraLightmap.GetMaterials().materials)
        {
            if (material == null)
            {
                continue;
            }

            Camera camera = cameraSetting.GetCamera();

            if (cameraSetting.cameraType == CameraSettings.CameraType.SceneView)
            {
                // why?
                LightmapMaterials.ClearMaterial(material);
            }
        }
    }
}

*/