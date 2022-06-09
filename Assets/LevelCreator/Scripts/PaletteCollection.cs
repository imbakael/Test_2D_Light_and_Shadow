using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PaletteCollection", menuName = "CreatePaletteCollection")]
public class PaletteCollection : ScriptableObject
{
    public List<PaletteItem> list = new List<PaletteItem>();
}
