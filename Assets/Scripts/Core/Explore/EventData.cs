using UnityEngine;

/// <summary>
/// 事件資料 - 定義地圖上可觸發的事件
/// </summary>
[System.Serializable]
public class EventData
{
    [Delayed] public string name;
    public Sprite thumbnail;
    [Delayed] public string conversationTitle;

    [Header("啟用設定")]
    [Tooltip("是否啟用此事件")]
    public bool isAble = true;

    [Header("遊戲模式")]
    [Tooltip("在故事模式中可使用")]
    public bool canStoryMode = true;
    [Tooltip("在事件模式中可使用")]
    public bool canEventMode = true;
}
