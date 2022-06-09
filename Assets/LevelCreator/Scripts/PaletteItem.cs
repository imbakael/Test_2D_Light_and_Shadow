using UnityEngine;

// 可以显示在PaletteWindow中的东西，可以是anything
public class PaletteItem : ScriptableObject {

    public string itemName = string.Empty;
    public Sprite sprite;
    public Category category = Category.Misc;

    public enum Category {
        Misc, // 默认是杂项; order = 0
        Ground, // 地面; order = 3
        BuildingBack, // 被建筑挡，如地上的草、石头、建筑后的木头堆等; order = 6
        // 人最低的order也是在这里; order > 6
        Building, // 建筑，可挡人也可被人挡; order = 9
        BuildingFront, // 装饰，可挡人也可被人挡, 如建筑前的路标; order = 12
        Decoration // 装饰前的装饰，可挡人也可被人挡，如树; order = 15
        // 人最高order在这，遮挡一切; order > 15
    }
}
