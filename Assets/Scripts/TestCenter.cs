using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCenter : MonoBehaviour
{
    private void Start() {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float height = sr.size.y;
        transform.localPosition = new Vector3(0, -height * 0.5f, 0);
    }
}
