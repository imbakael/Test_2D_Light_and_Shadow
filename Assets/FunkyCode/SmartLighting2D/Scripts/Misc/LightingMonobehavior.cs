﻿using UnityEngine;

namespace FunkyCode {
    public class LightingMonoBehaviour : MonoBehaviour {
        public void DestroySelf() {
            if (Application.isPlaying) {
                Destroy(gameObject);
            } else {
                if (this != null && gameObject != null) {
                    DestroyImmediate(gameObject);
                }
            }
        }
    }
}