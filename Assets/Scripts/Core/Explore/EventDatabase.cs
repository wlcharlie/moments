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

    // /// <summary>
    // /// 設定指定事件的啟用狀態
    // /// </summary>
    // public bool SetEventAble(string name, bool isAble)
    // {
    //     EventData eventData = events.Find(e => e.name == name);
    //     if (eventData != null)
    //     {
    //         eventData.isAble = isAble;
    //         return true;
    //     }
    //     return false;
    // }

    /// <summary>
    /// 根據 conversationTitle 設定事件的啟用狀態
    /// </summary>
    public bool SetEventAbleByConversation(string conversationTitle, bool isAble)
    {
        EventData eventData = events.Find(e => e.conversationTitle == conversationTitle);
        if (eventData != null)
        {
            eventData.isAble = isAble;
            return true;
        }
        return false;
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
            List<EventData> storyOnly = events.FindAll(e =>
            {
                Debug.Log($"[EventDatabase] 檢查事件 '{e.name}': isAble={IsEventAble(e)}, canStoryMode={e.canStoryMode}, canEventMode={e.canEventMode}");
                return IsEventAble(e) && e.canStoryMode && !e.canEventMode;
            });
            result.AddRange(storyOnly);

            // 如果不夠 6 個，再從兩邊都有的事件補充
            if (result.Count < 6)
            {
                List<EventData> shared = events.FindAll(e => IsEventAble(e) && e.canStoryMode && e.canEventMode);
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
            return events.FindAll(e => IsEventAble(e) && e.canEventMode);
        }
    }

    /// <summary>
    /// 檢查事件是否啟用 (結合 EventData.isAble 和 Runtime 狀態)
    /// </summary>
    private bool IsEventAble(EventData eventData)
    {
        // 再檢查 EventAbleManager 的 Runtime 狀態
        if (EventAbleManager.Instance != null)
        {
            return EventAbleManager.Instance.GetAble(eventData.conversationTitle, eventData.isAble);
        }

        return eventData.isAble;
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
