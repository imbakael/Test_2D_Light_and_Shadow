using UnityEngine;

// ������ʾ��PaletteWindow�еĶ�����������anything
public class PaletteItem : ScriptableObject {

    public string itemName = string.Empty;
    public Sprite sprite;
    public Category category = Category.Ground;

    public enum Category {
        Ground, // ����; order = 0
        BuildingBack, // ��������������ϵĲݡ�ʯͷ���������ľͷ�ѵ�; order = 2
        Building, // �������ɵ���Ҳ�ɱ��˵�; order = 4
        BuildingFront, // װ�Σ��ɵ���Ҳ�ɱ��˵�, �罨��ǰ��·��; order = 6
        Decoration // װ��ǰ��װ�Σ��ɵ���Ҳ�ɱ��˵�������; order = 8
    }
}
