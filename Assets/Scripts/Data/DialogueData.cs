using System;
using System.Collections.Generic;

/// <summary>
/// 對話資料結構
/// Unity 的 JsonUtility 不直接支援 Dictionary，所以使用 List 來儲存
/// 用於儲存對話系統中的 key-value pairs (例如：遊戲進度、選擇記錄等)
/// </summary>
[Serializable]
public class DialogueData
{
    public List<DialogueEntry> entries = new List<DialogueEntry>();

    /// <summary>
    /// 取得指定 key 的 value
    /// </summary>
    public string GetValue(string key)
    {
        var entry = entries.Find(e => e.key == key);
        return entry != null ? entry.value : null;
    }

    /// <summary>
    /// 設定或新增 key-value pair
    /// </summary>
    public void SetValue(string key, string value)
    {
        var entry = entries.Find(e => e.key == key);
        if (entry != null)
        {
            entry.value = value;
        }
        else
        {
            entries.Add(new DialogueEntry { key = key, value = value });
        }
    }

    /// <summary>
    /// 檢查是否包含指定的 key
    /// </summary>
    public bool ContainsKey(string key)
    {
        return entries.Exists(e => e.key == key);
    }
}

/// <summary>
/// 對話資料中的單一條目 (key-value pair)
/// </summary>
[Serializable]
public class DialogueEntry
{
    public string key;
    public string value;
}
