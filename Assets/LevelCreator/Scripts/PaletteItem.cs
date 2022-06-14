using UnityEngine;

// 可以显示在PaletteWindow中的东西，可以是anything
public class PaletteItem : ScriptableObject {

    public string itemName = string.Empty;
    public Sprite sprite;
    public Category category = Category.Ground;

    public enum Category {
        Ground, // 地面; layer = 0
        BuildingBack, // 被建筑挡，如地上的草、石头、建筑后的木头堆等; layer = 2
        Building, // 建筑，可挡人也可被人挡; layer = 4
        BuildingFront, // 装饰，可挡人也可被人挡, 如建筑前的路标; layer = 6
        Decoration // 装饰前的装饰，可挡人也可被人挡，如树; layer = 8
    }
}
