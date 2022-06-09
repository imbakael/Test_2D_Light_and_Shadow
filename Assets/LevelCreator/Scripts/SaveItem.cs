using System.Collections.Generic;

public class SaveItem
{
    public int levelId; // 关卡id
    public int col;
    public int row;
    public int[] walkArea; // 可行走区域
    public Dictionary<PaletteItem.Category, string[]> allTiles; // key：层，value：guid数组（用来获得sprite）
    public Dictionary<PaletteItem.Category, List<TileOffset>> allOffsets;

    public static SaveItem GetDefaultSaveItem(int col, int row) {
        List<PaletteItem.Category> categories = MyTools.GetListFromEnum<PaletteItem.Category>();
        return
            new SaveItem {
                levelId = 1,
                col = col,
                row = row,
                walkArea = new int[col * row],
                allTiles = GetDefaultAllTiles(col, row, categories),
                allOffsets = GetDefaultAllOffsets(categories)
            };
    }

    private static Dictionary<PaletteItem.Category, string[]> GetDefaultAllTiles(int col, int row, List<PaletteItem.Category> categories) {
        var result = new Dictionary<PaletteItem.Category, string[]>();
        foreach (PaletteItem.Category item in categories) {
            result.Add(item, new string[col * row]);
        }
        return result;
    }

    private static Dictionary<PaletteItem.Category, List<TileOffset>> GetDefaultAllOffsets(List<PaletteItem.Category> categories) {
        var result = new Dictionary<PaletteItem.Category, List<TileOffset>>();
        foreach (PaletteItem.Category item in categories) {
            result.Add(item, new List<TileOffset>());
        }
        return result;
    }
}
