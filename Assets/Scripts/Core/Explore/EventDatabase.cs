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
    /// </summary>
    public List<EventData> GetEventsForMode(GameMode mode)
    {
        return mode == GameMode.Story
            ? events.FindAll(e => e.canStoryMode)
            : events.FindAll(e => e.canEventMode);
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
}
