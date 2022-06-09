﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public static class MyTools
{
    public static void EnqueueAfterCheckNull<T>(this Queue<T> queue, T item) {
        if (item != null) {
            queue.Enqueue(item);
        }
    }

    // 四舍五入
    public static int GetRound(float value) {
        float v = (value + 0.5f) * 10;
        return Mathf.FloorToInt(v) / 10;
    }

    public static List<T> Clone<T>(List<T> target) {
        var temp = new List<T>();
        temp.AddRange(target);
        return temp;
    }

    public static Transform FindChildByName(this Transform parent, string name) {
        var queue = new Queue<Transform>();
        queue.Enqueue(parent);
        while (queue.Count != 0) {
            Transform item = queue.Dequeue();
            for (int i = 0; i < item.childCount; i++) {
                Transform child = item.GetChild(i);
                if (child.name == name) {
                    return child;
                } else {
                    queue.Enqueue(child);
                }
            }
        }
        return null;
    }

    public static List<T> GetListFromEnum<T>() {
        var result = new List<T>();
        Array enums = Enum.GetValues(typeof(T));
        foreach (T e in enums) {
            result.Add(e);
        }
        return result;
    }

    public static string GetSerializeJson(SaveItem saveItem) => JsonConvert.SerializeObject(saveItem);

    public static T DeserializeObject<T>(string s) => JsonConvert.DeserializeObject<T>(s);
}
