using UnityEngine;
using UnityEngine.EventSystems;
using PixelCrushers.DialogueSystem;

public class TapToContinue : MonoBehaviour, IPointerClickHandler
{
    [Header("Settings")]
    [Tooltip("對話面板的 GameObject（如果為空，會自動查找）")]
    public GameObject dialoguePanel;
    
    [Tooltip("當對話面板被隱藏時，是否自動顯示此元件")]
    public bool autoShowWhenPanelHidden = true;

    private bool wasPanelActive = true;

    void Update()
    {
        // 只在對話進行時才檢查
        if (!DialogueManager.isConversationActive)
        {
            wasPanelActive = true;
            return;
        }

        // 獲取對話面板的狀態
        bool isPanelActive = GetDialoguePanelActiveState();
        
        // 當面板狀態改變時，根據設置自動顯示
        if (isPanelActive != wasPanelActive)
        {
            if (autoShowWhenPanelHidden && !isPanelActive)
            {
                gameObject.SetActive(true);
            }
            wasPanelActive = isPanelActive;
        }
    }

    /// <summary>
    /// 獲取對話面板的活動狀態
    /// </summary>
    private bool GetDialoguePanelActiveState()
    {
        if (dialoguePanel != null)
        {
            return dialoguePanel.activeSelf;
        }

        // 嘗試從 DialogueManager 獲取對話面板
        var dialogueUI = DialogueManager.instance?.dialogueUI as AbstractDialogueUI;
        if (dialogueUI != null)
        {
            var unityUIDialogueControls = dialogueUI.dialogueControls as UnityUIDialogueControls;
            if (unityUIDialogueControls != null && unityUIDialogueControls.panel != null)
            {
                dialoguePanel = unityUIDialogueControls.panel.gameObject;
                return dialoguePanel.activeSelf;
            }
        }

        // 嘗試用名稱查找
        if (dialoguePanel == null)
        {
            GameObject found = GameObject.Find("Dialogue Panel");
            if (found != null)
            {
                dialoguePanel = found;
                return dialoguePanel.activeSelf;
            }
        }

        return true;
    }

    /// <summary>
    /// 實現 IPointerClickHandler 接口，直接處理點擊事件
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        ContinueConversation();
    }

    /// <summary>
    /// 公開方法，可以從 Button 的 onClick 事件中調用
    /// </summary>
    public void OnTap()
    {
        ContinueConversation();
    }

    /// <summary>
    /// 繼續對話的核心邏輯
    /// </summary>
    private void ContinueConversation()
    {
        if (!DialogueManager.isConversationActive)
        {
            return;
        }

        // 優先使用 ConversationView 的方法
        var conversationView = DialogueManager.instance.conversationView;
        if (conversationView != null)
        {
            conversationView.HandleContinueButtonClick();
            return;
        }

        // 備用方案：使用 BroadcastMessage
        DialogueManager.instance.BroadcastMessage(
            DialogueSystemMessages.OnConversationContinueAll,
            SendMessageOptions.DontRequireReceiver
        );
    }
}
