using UnityEngine;

// ������ʾ��PaletteWindow�еĶ�����������anything
public class PaletteItem : ScriptableObject {

    public string itemName = string.Empty;
    public Sprite sprite;
    public Category category = Category.Misc;

    public enum Category {
        Misc, // Ĭ��������; order = 0
        Ground, // ����; order = 3
        BuildingBack, // ��������������ϵĲݡ�ʯͷ���������ľͷ�ѵ�; order = 6
        // ����͵�orderҲ��������; order > 6
        Building, // �������ɵ���Ҳ�ɱ��˵�; order = 9
        BuildingFront, // װ�Σ��ɵ���Ҳ�ɱ��˵�, �罨��ǰ��·��; order = 12
        Decoration // װ��ǰ��װ�Σ��ɵ���Ҳ�ɱ��˵�������; order = 15
        // �����order���⣬�ڵ�һ��; order > 15
    }
}
