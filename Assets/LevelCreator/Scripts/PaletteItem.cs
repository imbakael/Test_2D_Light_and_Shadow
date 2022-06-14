using UnityEngine;

// ������ʾ��PaletteWindow�еĶ�����������anything
public class PaletteItem : ScriptableObject {

    public string itemName = string.Empty;
    public Sprite sprite;
    public Category category = Category.Ground;

    public enum Category {
        Ground, // ����; layer = 0
        BuildingBack, // ��������������ϵĲݡ�ʯͷ���������ľͷ�ѵ�; layer = 2
        Building, // �������ɵ���Ҳ�ɱ��˵�; layer = 4
        BuildingFront, // װ�Σ��ɵ���Ҳ�ɱ��˵�, �罨��ǰ��·��; layer = 6
        Decoration // װ��ǰ��װ�Σ��ɵ���Ҳ�ɱ��˵�������; layer = 8
    }
}
