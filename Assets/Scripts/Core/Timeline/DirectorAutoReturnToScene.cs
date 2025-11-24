using UnityEngine;
using UnityEngine.SceneManagement;
using PixelCrushers.DialogueSystem;

/// <summary>
/// 用於 Timeline 場景，可以透過 Signal 觸發回到對話場景
/// 在 Timeline 中放置 Signal Emitter，綁定 Sig_ReturnToScene，然後在 SignalReceiver 中綁定此腳本的 ReturnToScene 方法
/// </summary>
public class DirectorAutoReturnToScene : MonoBehaviour
{
    [Header("目標場景設定")]
    [Tooltip("要回到的對話場景名稱")]
    [SerializeField] private string targetSceneName = "MainStoryScene";
    
    [Tooltip("要啟動的對話 ID（可選，如果為空則只載入場景）")]
    [SerializeField] private string conversationId = "";
    
    [Header("轉場設定")]
    [Tooltip("轉場類型")]
    [SerializeField] private TransitionType transitionType = TransitionType.LoadingScreen;

    /// <summary>
    /// 回到指定的對話場景
    /// 此方法可以透過 Timeline Signal 觸發
    /// </summary>
    public void ReturnToScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("DirectorAutoReturnToScene: 未指定目標場景名稱！");
            return;
        }

        Debug.Log($"DirectorAutoReturnToScene: 準備回到場景 {targetSceneName}");

        // 使用 TransitionManager 載入場景
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.LoadSceneWithTransition(
                targetSceneName,
                transitionType,
                onLoadDone: () =>
                {
                    // 場景載入完成後，如果有指定對話 ID，則啟動對話
                    if (!string.IsNullOrEmpty(conversationId))
                    {
                        Debug.Log($"DirectorAutoReturnToScene: 啟動對話 {conversationId}");
                        DialogueManager.StopAllConversations();
                        DialogueManager.StartConversation(conversationId);
                    }
                });
        }
        else
        {
            // 後備方案：直接載入場景
            Debug.LogWarning("DirectorAutoReturnToScene: TransitionManager.Instance 為 null，使用直接載入場景");
            SceneManager.LoadScene(targetSceneName);
            
            // 場景載入後啟動對話（需要等待一幀）
            if (!string.IsNullOrEmpty(conversationId))
            {
                StartCoroutine(StartConversationAfterLoad());
            }
        }
    }

    /// <summary>
    /// 場景載入後啟動對話（用於後備方案）
    /// </summary>
    private System.Collections.IEnumerator StartConversationAfterLoad()
    {
        // 等待一幀，確保場景完全載入
        yield return null;
        
        Debug.Log($"DirectorAutoReturnToScene: 啟動對話 {conversationId}");
        DialogueManager.StopAllConversations();
        DialogueManager.StartConversation(conversationId);
    }
}
