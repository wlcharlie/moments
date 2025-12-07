using UnityEngine;
using PixelCrushers.DialogueSystem;
using System.Reflection;

/// <summary>
/// 用於在對話中切換到另一個對話的輔助類
/// 可以在對話節點的 Script 欄位中使用 Lua 調用
/// 
/// 用法（在對話節點的 Script 欄位）:
/// SwitchConversation("CH01_SC04_SE02")
/// SwitchConversation("CH01_SC04_SE02", "None")
/// SwitchConversation("CH01_SC04_SE02", "Cover")
/// </summary>
public class ConversationSwitcher : MonoBehaviour
{
    [Tooltip("Typically leave unticked so temporary Dialogue Managers don't unregister your functions.")]
    public bool unregisterOnDisable = false;

    void OnEnable()
    {
        // 註冊 Lua 函數
        Lua.RegisterFunction("SwitchConversation", this, 
            SymbolExtensions.GetMethodInfo(() => SwitchConversation(string.Empty, string.Empty)));
        Debug.Log("ConversationSwitcher: 已註冊 Lua 函數 SwitchConversation");
    }

    void OnDisable()
    {
        if (unregisterOnDisable)
        {
            Lua.UnregisterFunction("SwitchConversation");
        }
    }

    /// <summary>
    /// 切換對話（Lua 可調用）
    /// </summary>
    /// <param name="conversationId">對話 ID</param>
    /// <param name="transitionType">轉場類型（可選，預設為 "None"）</param>
    public void SwitchConversation(string conversationId, string transitionType = "None")
    {
        if (string.IsNullOrEmpty(conversationId))
        {
            Debug.LogError("ConversationSwitcher: Conversation ID is required.");
            return;
        }

        Debug.Log($"ConversationSwitcher: 準備切換到對話 '{conversationId}' (轉場類型: {transitionType})");

        // 解析轉場類型
        bool useTransition = false;
        TransitionType transitionTypeEnum = TransitionType.Cover;

        if (!transitionType.Equals("None", System.StringComparison.OrdinalIgnoreCase))
        {
            if (System.Enum.TryParse<TransitionType>(transitionType, true, out transitionTypeEnum))
            {
                useTransition = true;
            }
        }

        // 執行切換
        if (useTransition && TransitionManager.Instance != null)
        {
            Debug.Log($"ConversationSwitcher: 使用轉場效果 '{transitionTypeEnum}'");
            TransitionManager.Instance.DoConversationTransition(transitionTypeEnum, () =>
            {
                DialogueManager.StopAllConversations();
                DialogueManager.StartConversation(conversationId);
                Debug.Log($"ConversationSwitcher: 已切換到對話 '{conversationId}'");
            });
        }
        else
        {
            Debug.Log($"ConversationSwitcher: 直接切換對話（無轉場）");
            DialogueManager.StopAllConversations();
            DialogueManager.StartConversation(conversationId);
            Debug.Log($"ConversationSwitcher: 已切換到對話 '{conversationId}'");
        }
    }
}

