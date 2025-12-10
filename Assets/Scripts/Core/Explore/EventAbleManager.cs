using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 事件啟用狀態管理器 - 在 Runtime 追蹤事件的 isAble 狀態
/// 支援存檔/讀檔，不會修改原始 ScriptableObject
/// </summary>
public class EventAbleManager : MonoBehaviour
{
    public static EventAbleManager Instance { get; private set; }

    private const string SAVE_KEY = "eventAble";

    // Key: conversationTitle, Value: isAble
    private Dictionary<string, bool> ableStates = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadStates();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 設定事件的啟用狀態 (使用 conversationTitle)
    /// </summary>
    public void SetAble(string conversationTitle, bool isAble)
    {
        if (string.IsNullOrEmpty(conversationTitle)) return;

        ableStates[conversationTitle] = isAble;
        SaveStates();
        Debug.Log($"[EventAbleManager] 設定 '{conversationTitle}' isAble = {isAble}");
    }

    /// <summary>
    /// 取得事件的啟用狀態
    /// 如果沒有記錄，回傳 defaultValue
    /// </summary>
    public bool GetAble(string conversationTitle, bool defaultValue = true)
    {
        if (string.IsNullOrEmpty(conversationTitle)) return defaultValue;

        if (ableStates.TryGetValue(conversationTitle, out bool isAble))
        {
            return isAble;
        }
        return defaultValue;
    }

    /// <summary>
    /// 檢查事件是否啟用 (結合 EventData.isAble 和 Runtime 狀態)
    /// </summary>
    public bool IsEventAble(EventData eventData)
    {
        if (eventData == null) return false;

        // 先檢查 EventData 原始設定
        if (!eventData.isAble) return false;

        // 再檢查 Runtime 狀態 (如果有設定的話)
        return GetAble(eventData.conversationTitle, true);
    }

    /// <summary>
    /// 清除所有 Runtime 狀態
    /// </summary>
    public void ClearAllStates()
    {
        ableStates.Clear();
        SaveStates();
        Debug.Log("[EventAbleManager] 已清除所有狀態");
    }

    private void SaveStates()
    {
        if (PersistentDataManager.Instance == null) return;

        EventAbleSaveData saveData = new()
        {
            states = new List<EventAbleState>()
        };

        foreach (var kvp in ableStates)
        {
            saveData.states.Add(new EventAbleState
            {
                conversationTitle = kvp.Key,
                isAble = kvp.Value
            });
        }

        PersistentDataManager.Instance.SaveData(saveData, SAVE_KEY);
    }

    private void LoadStates()
    {
        if (PersistentDataManager.Instance == null) return;

        if (!PersistentDataManager.Instance.HasSaveData(SAVE_KEY)) return;

        EventAbleSaveData saveData = PersistentDataManager.Instance.LoadData<EventAbleSaveData>(SAVE_KEY);
        if (saveData?.states == null) return;

        ableStates.Clear();
        foreach (var state in saveData.states)
        {
            ableStates[state.conversationTitle] = state.isAble;
        }

        Debug.Log($"[EventAbleManager] 已載入 {ableStates.Count} 個狀態");
    }
}

[System.Serializable]
public class EventAbleSaveData
{
    public List<EventAbleState> states;
}

[System.Serializable]
public class EventAbleState
{
    public string conversationTitle;
    public bool isAble;
}
