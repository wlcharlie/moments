using UnityEngine;
using UnityEngine.SceneManagement;
using PixelCrushers.DialogueSystem;

/// <summary>
/// 用於 Timeline 場景，可以透過 Signal 觸發回到對話場景
/// 在 Timeline 中放置 Signal Emitter，綁定 Sig_ReturnToScene，然後在 SignalReceiver 中綁定此腳本的 ReturnToScene 方法
/// </summary>
public class DirectorAutoReturnToScene : MonoBehaviour
{
    // 靜態變數：保存要啟動的對話信息（用於場景切換後啟動對話）
    private static string pendingConversationId = null;
    private static string pendingSceneName = null;

    [Header("目標場景設定")]
    [Tooltip("要回到的對話場景名稱")]
    [SerializeField] private string targetSceneName = "MainStoryScene";
    
    [Tooltip("要啟動的對話 ID（可選，如果為空則只載入場景）")]
    [SerializeField] private string conversationId = "";
    
    [Header("轉場設定")]
    [Tooltip("轉場類型（選擇 None 則不使用轉場效果）")]
    [SerializeField] private TransitionType transitionType = TransitionType.LoadingScreen;
    
    [Tooltip("是否使用轉場效果（取消勾選則直接載入場景，不使用轉場）")]
    [SerializeField] private bool useTransition = true;


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

        // 如果使用轉場且 TransitionManager 存在，使用轉場效果
        if (useTransition && TransitionManager.Instance != null)
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
            // 不使用轉場或 TransitionManager 不存在，直接載入場景
            if (!useTransition)
            {
                Debug.Log("DirectorAutoReturnToScene: 不使用轉場效果，直接載入場景");
            }
            else
            {
                Debug.LogWarning("DirectorAutoReturnToScene: TransitionManager.Instance 為 null，使用直接載入場景");
            }
            
            // 如果有對話 ID，保存到靜態變數
            if (!string.IsNullOrEmpty(conversationId))
            {
                pendingConversationId = conversationId;
                pendingSceneName = targetSceneName;
            }
            
            // 訂閱場景載入完成事件
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(targetSceneName);
        }
    }

    /// <summary>
    /// 場景載入完成後的回調（靜態方法，用於場景切換）
    /// </summary>
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 取消訂閱，避免重複觸發
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // 確保載入的是目標場景
        if (scene.name != pendingSceneName)
        {
            return;
        }
        
        // 立即啟動對話（使用協程）
        // 在目標場景中查找任何 GameObject 來執行協程
        GameObject tempObj = new GameObject("TempConversationStarter");
        DirectorAutoReturnToScene tempScript = tempObj.AddComponent<DirectorAutoReturnToScene>();
        tempScript.StartCoroutine(tempScript.StartPendingConversation());
    }

    /// <summary>
    /// 啟動待處理的對話（靜態方法調用）
    /// </summary>
    private System.Collections.IEnumerator StartPendingConversation()
    {
        // 等待幾幀，確保場景和對話系統完全載入
        yield return null;
        yield return null;
        
        // 確保 DialogueManager 存在
        int retryCount = 0;
        while (DialogueManager.instance == null && retryCount < 10)
        {
            Debug.LogWarning($"DirectorAutoReturnToScene: DialogueManager.instance 為 null，等待載入... (重試 {retryCount + 1}/10)");
            yield return new WaitForSeconds(0.1f);
            retryCount++;
        }
        
        if (DialogueManager.instance == null)
        {
            Debug.LogError("DirectorAutoReturnToScene: DialogueManager.instance 仍然為 null，無法啟動對話");
            pendingConversationId = null;
            pendingSceneName = null;
            
            // 銷毀臨時物件
            if (gameObject.name == "TempConversationStarter")
            {
                Destroy(gameObject);
            }
            yield break;
        }
        
        string conversationToStart = pendingConversationId;
        pendingConversationId = null;
        pendingSceneName = null;
        
        Debug.Log($"DirectorAutoReturnToScene: 啟動對話 {conversationToStart}");
        DialogueManager.StopAllConversations();
        DialogueManager.StartConversation(conversationToStart);
        
        // 銷毀臨時物件
        if (gameObject.name == "TempConversationStarter")
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 清理：取消訂閱事件
    /// </summary>
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
