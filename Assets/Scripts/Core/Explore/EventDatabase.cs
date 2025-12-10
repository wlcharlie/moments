using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 事件資料庫 - 存放地圖上可觸發的事件列表
/// </summary>
[CreateAssetMenu(fileName = "EventDatabase", menuName = "Game/Event Database")]
public class EventDatabase : ScriptableObject
{
    [Header("事件列表")]
    public List<EventData> events = new();

    /// <summary>
    /// 根據名稱取得事件
    /// </summary>
    public EventData GetEventByName(string name)
    {
        return events.Find(e => e.name == name);
    }

    /// <summary>
    /// 取得指定模式可用的事件
    /// Story 模式：優先取得故事專屬事件 (canStoryMode && !canEventMode)，最多 6 個
    /// Event 模式：取得所有 canEventMode 的事件
    /// </summary>
    public List<EventData> GetEventsForMode(GameMode mode)
    {
        if (mode == GameMode.Story)
        {
            List<EventData> result = new();

            // 優先加入故事專屬事件 (只有 StoryMode，沒有 EventMode)
            List<EventData> storyOnly = events.FindAll(e => e.canStoryMode && !e.canEventMode);
            result.AddRange(storyOnly);

            // 如果不夠 6 個，再從兩邊都有的事件補充
            if (result.Count < 6)
            {
                List<EventData> shared = events.FindAll(e => e.canStoryMode && e.canEventMode);
                ShuffleList(shared);
                int remaining = 6 - result.Count;
                for (int i = 0; i < shared.Count && i < remaining; i++)
                {
                    result.Add(shared[i]);
                }
            }

            // 限制最多 6 個
            if (result.Count > 6)
            {
                result = result.GetRange(0, 6);
            }

            // 打亂順序
            ShuffleList(result);

            return result;
        }
        else
        {
            return events.FindAll(e => e.canEventMode);
        }
    }

    /// <summary>
    /// 隨機取得一個指定模式可用的事件
    /// </summary>
    public EventData GetRandomEvent(GameMode mode)
    {
        List<EventData> available = GetEventsForMode(mode);
        if (available.Count == 0) return null;
        return available[Random.Range(0, available.Count)];
    }

    private static void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
