/// <summary>
/// 探索地圖存檔資料
/// </summary>
[System.Serializable]
public class ExploreMapSaveData
{
    /// <summary>
    /// 遊戲模式
    /// </summary>
    public GameMode mode;

    /// <summary>
    /// 隨機種子 (用於重現相同的事件順序)
    /// </summary>
    public int seed;

    /// <summary>
    /// 玩家當前所在節點索引 (0 = 起點, 1 = 第一個事件節點, ...)
    /// </summary>
    public int playerNodeIndex;

    /// <summary>
    /// 是否有有效的存檔資料
    /// </summary>
    public bool isValid;
}
