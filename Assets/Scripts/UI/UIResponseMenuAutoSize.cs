using UnityEngine;
using UnityEngine.UI;
using PixelCrushers.DialogueSystem;

[ExecuteAlways]   // 在編輯模式也會更新，方便你在 Scene 裡看效果
public class ResponseMenuAutoSize : MonoBehaviour
{
    [Header("References")]
    [Tooltip("要調整高度的外框，一般就是 Container 自己")]
    public RectTransform container;

    [Tooltip("內層的 Scroll Content（放 Response Button 的那個）")]
    public RectTransform content;

    [Header("Height Settings")]
    [Tooltip("最小高度")]
    public float minHeight = 440f;

    [Tooltip("最大高度（超過就出捲動）")]
    public float maxHeight = 680f;

    [Tooltip("額外加在內容高度上的上下留白")]
    public float extraPadding = 40f;

    void Reset()
    {
        // 自動抓參考，方便你拖 Script 進去時不用自己設
        container = GetComponent<RectTransform>();

        // 嘗試在孩子裡找一個叫 Scroll Content 的物件
        if (content == null)
        {
            var t = transform.Find("Panel/Scroll Rect/Scroll Content");
            if (t != null)
            {
                content = t.GetComponent<RectTransform>();
            }
            else
            {
                Debug.LogWarning($"[ResponseMenuAutoSize] 找不到 Scroll Content (路徑: Panel/Scroll Rect/Scroll Content)。請檢查物件階層結構。");
            }
        }
    }
    

    void Awake()
    {
        if (container == null)
            container = GetComponent<RectTransform>();
        
        // 訂閱 Response Menu 顯示事件
        SubscribeToResponseMenuEvent();
    }
    
    void OnEnable()
    {
        // 確保在啟用時也訂閱事件
        SubscribeToResponseMenuEvent();
    }
    
    void OnDisable()
    {
        // 取消訂閱事件
        UnsubscribeFromResponseMenuEvent();
    }
    
    /// <summary>
    /// 訂閱 Response Menu 顯示事件
    /// </summary>
    private void SubscribeToResponseMenuEvent()
    {
        if (!Application.isPlaying) return;
        
        var dialogueSystemEvents = DialogueManager.instance?.GetComponent<DialogueSystemEvents>();
        if (dialogueSystemEvents != null)
        {
            dialogueSystemEvents.conversationEvents.onConversationResponseMenu.AddListener(OnResponseMenuShown);
        }
    }
    
    /// <summary>
    /// 取消訂閱 Response Menu 顯示事件
    /// </summary>
    private void UnsubscribeFromResponseMenuEvent()
    {
        if (!Application.isPlaying) return;
        
        var dialogueSystemEvents = DialogueManager.instance?.GetComponent<DialogueSystemEvents>();
        if (dialogueSystemEvents != null)
        {
            dialogueSystemEvents.conversationEvents.onConversationResponseMenu.RemoveListener(OnResponseMenuShown);
        }
    }
    
    /// <summary>
    /// Response Menu 顯示時的回調
    /// </summary>
    private void OnResponseMenuShown(Response[] responses)
    {
        // 檢查 content 是否已設定
        if (content == null)
        {
            Debug.LogWarning("[ResponseMenuAutoSize] Content 尚未設定！請在 Inspector 中指定 Content。");
        }
        
        // 檢查 container 是否已設定
        if (container == null)
        {
            Debug.LogWarning("[ResponseMenuAutoSize] Container 尚未設定！");
        }
    }

    void LateUpdate()
    {
        if (container == null || content == null) return;

        UpdateHeight();
    }

    void UpdateHeight()
    {
        // 取得 Content 的「偏好高度」
        float preferredHeight = LayoutUtility.GetPreferredHeight(content);

        // 加上額外 padding
        float targetHeight = preferredHeight + extraPadding;

        // 限制在 min / max 之間
        targetHeight = Mathf.Clamp(targetHeight, minHeight, maxHeight);

        // 套用到 Container 的 sizeDelta.y
        var size = container.sizeDelta;
        size.y = targetHeight;
        container.sizeDelta = size;
    }
}
