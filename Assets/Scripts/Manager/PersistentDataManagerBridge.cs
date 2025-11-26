using UnityEngine;
using PixelCrushers.DialogueSystem;

/// <summary>
/// 轉發呼叫給 PersistentDataManager Singleton
/// 用於 UI 事件綁定，避免直接引用 Singleton
/// 使用方式：在 Unity Editor 建立 ScriptableObject asset，然後在 UI Button 的 onClick 事件中引用靜態方法
/// 注意：帶參數的方法（如 DeleteSaveData）需要在 UI 中手動輸入參數字串
/// </summary>
[CreateAssetMenu(fileName = "PersistentDataManagerBridge", menuName = "Bridges/Persistent Data Manager Bridge")]
public class PersistentDataManagerBridge : ScriptableObject
{
    // 靜態方法可以直接被 Unity UI 事件系統引用

    public static void DeleteSaveData(string fileName)
    {
        if (PersistentDataManager.Instance != null)
            PersistentDataManager.Instance.DeleteSaveData(fileName);
    }

    public static void DeleteAllSaveData()
    {
        if (PersistentDataManager.Instance != null)
            PersistentDataManager.Instance.DeleteAllSaveData();
    }

    /// <summary>
    /// 當對話行顯示時觸發
    /// 用於 Dialogue System 的 OnConversationLine 事件
    /// 會自動將 conversation title 儲存到 dialogue.json
    /// </summary>
    public static void OnConversationLine(Subtitle subtitle)
    {
        Debug.Log("PersistentDataManagerBridge: OnConversationLine 被呼叫");

        // 取得 conversation title
        int conversationID = subtitle.dialogueEntry.conversationID;
        string conversationTitle = DialogueManager.GetConversationTitle(conversationID);

        Debug.Log($"Conversation ID: {conversationID}");
        Debug.Log($"Conversation Title: {conversationTitle}");

        // 儲存 conversation title 到 dialogue.json
        if (PersistentDataManager.Instance != null)
        {
            // 載入現有資料
            DialogueData data = PersistentDataManager.Instance.LoadData<DialogueData>("dialogue");

            // 如果資料結構是 null，初始化它
            if (data == null)
            {
                data = new DialogueData();
            }

            // 儲存 conversation title（使用 key: "mainStory"）
            data.SetValue("mainStory", conversationTitle);

            // 儲存回檔案
            PersistentDataManager.Instance.SaveData(data, "dialogue");

            Debug.Log($"已儲存 mainStory = {conversationTitle} 到 dialogue.json");
        }
        else
        {
            Debug.LogError("PersistentDataManagerBridge: PersistentDataManager.Instance is null.");
        }
    }
}
