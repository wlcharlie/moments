using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameMode
{
    Story,
    Event
}

public class GameManager : MonoBehaviour
{
    // Singleton 模式
    public static GameManager Instance { get; private set; }

    // 遊戲模式
    public GameMode CurrentMode { get; private set; } = GameMode.Story;

    void Awake()
    {
        // 確保只有一個 GameManager 存在
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 切換場景時不會被銷毀
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ===== UI Panel 事件處理 =====

    public void OnMenuPanelClicked()
    {
        Debug.Log("選單 Panel 被點擊");
        // 你的邏輯
    }

    public void OnSettingsPanelClicked()
    {
        Debug.Log("設定 Panel 被點擊");
        // 開啟設定畫面
    }

    public void OnInventoryPanelClicked()
    {
        Debug.Log("背包 Panel 被點擊");
        // 開啟背包系統
    }

    // ===== 按鈕事件處理 =====

    // 首頁 開始遊戲按鈕
    public void OnStartButtonClicked()
    {
        Debug.Log("顯示選單");

        // 播放FMOD ui_start_game
        FMODUnity.RuntimeManager.PlayOneShot("event:/ui/ui_start_game");

        GameObject uiTitleSceneButtons = GameObject.Find("UITitleSceneButtons");
        if (uiTitleSceneButtons != null)
        {
            Transform menuTransform = uiTitleSceneButtons.transform.Find("SafeArea/Menu");
            if (menuTransform != null)
            {
                menuTransform.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("在 UITitleSceneButtons 下找不到 'SafeArea/Menu' 子物件");
            }
        }
        else
        {
            Debug.LogWarning("找不到名為 'UITitleSceneButtons' 的 GameObject");
        }

        GameObject startGameButton = GameObject.Find("StartGameButton");
        if (startGameButton != null)
        {
            startGameButton.SetActive(false);
        }
        else
        {
            Debug.LogWarning("找不到名為 'StartGameButton' 的 GameObject");
        }
    }

    /// <summary>
    /// 從存檔中讀取對話進度
    /// </summary>
    private string GetSavedConversation()
    {
        // 預設對話
        string defaultConversation = "CH01_SC01_SE01";

        // 檢查 PersistentDataManager 是否存在
        if (global::PersistentDataManager.Instance == null)
        {
            Debug.LogWarning("PersistentDataManager 不存在，使用預設對話");
            return defaultConversation;
        }

        // 檢查是否有存檔
        if (!global::PersistentDataManager.Instance.HasSaveData("dialogue"))
        {
            Debug.Log("沒有存檔，從頭開始遊戲");
            return defaultConversation;
        }

        // 載入存檔資料
        var saveData = global::PersistentDataManager.Instance.LoadData<DialogueData>("dialogue");

        if (saveData != null && saveData.entries != null)
        {
            // 取得 mainStory 的值
            string savedConversation = saveData.GetValue("mainStory");

            if (!string.IsNullOrEmpty(savedConversation))
            {
                Debug.Log($"載入存檔進度: {savedConversation}");
                return savedConversation;
            }
        }

        Debug.Log("存檔中沒有 mainStory，使用預設對話");
        return defaultConversation;
    }

    public void OnQuitButtonClicked()
    {
        Debug.Log("退出遊戲");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnPauseButtonClicked()
    {
        Debug.Log("暫停遊戲");
        Time.timeScale = 0f; // 暫停遊戲時間
    }

    public void OnResumeButtonClicked()
    {
        Debug.Log("繼續遊戲");
        Time.timeScale = 1f; // 恢復遊戲時間
    }

    // ===== 模式選擇按鈕 =====

    public void OnStoryModeButtonClicked()
    {
        Debug.Log("開始遊戲");
        CurrentMode = GameMode.Story;

        // 讀取存檔中的對話進度
        string conversationToResume = GetSavedConversation();

        Debug.Log($"準備從對話 {conversationToResume} 繼續遊戲");

        if (TransitionManager.Instance != null)
        {
            Debug.Log("開始遊戲過場");
            TransitionManager.Instance.LoadSceneWithTransition("MainStoryScene", TransitionType.LoadingScreen, onLoadDone: () =>
            {
                // 場景載入完成後啟動對話
                DialogueManager.StartConversation(conversationToResume);
            });
        }
        else
        {
            SceneManager.LoadScene("MainStoryScene"); // 後備方案
            DialogueManager.StartConversation(conversationToResume);
        }
    }

    public void OnEventModeButtonClicked()
    {
        Debug.Log("進入事件模式");
        CurrentMode = GameMode.Event;
        // 清除 exploreMap.json
        PersistentDataManager.Instance.DeleteSaveData("exploreMap");
        if (TransitionManager.Instance != null)
        {
            Debug.Log("開始遊戲過場");
            TransitionManager.Instance.LoadSceneWithTransition("ExploreScene", TransitionType.LoadingScreen);
        }
        else
        {
            SceneManager.LoadScene("ExploreScene"); // 後備方案
        }
    }

    public void OnDebugModeButtonClicked()
    {
        Debug.Log("進入偵錯模式");
        CurrentMode = GameMode.Story;
        TransitionManager.Instance.LoadSceneWithTransition("DialogueDebugScene", TransitionType.LoadingScreen);
    }

    public void OnFollowUsButtonClicked()
    {
        Debug.Log("關注我們");
        // TODO: 後續補上邏輯
    }

    // ===== 流程 管理 =====

    /// <summary>
    /// 取得小遊戲結束後應該進入的對話
    /// 根據存檔中記錄的對話查找映射表
    /// </summary>
    // private string GetMinigameNextConversation()
    // {
    //     string savedConversation = GetSavedConversation();

    //     if (minigameNextConversationMap.TryGetValue(savedConversation, out string nextConversation))
    //     {
    //         Debug.Log($"找到映射: {savedConversation} -> {nextConversation}");
    //         return nextConversation;
    //     }

    //     // 如果沒有映射，直接返回存檔中的對話
    //     Debug.Log($"沒有找到 {savedConversation} 的映射，使用存檔對話");
    //     return savedConversation;
    // }

    /// <summary>
    /// 切換到 MainStoryScene 並從存檔繼續對話
    /// 從 dialogue.json 讀取 mainStory 值作為對話起點
    /// </summary>
    public void ResumeConversation()
    {
        if (CurrentMode == GameMode.Story)
        {
            string conversationToResume = GetSavedConversation();
            Debug.Log($"ResumeConversation: 準備從對話 {conversationToResume} 繼續");

            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.LoadSceneWithTransition("MainStoryScene", TransitionType.Cover, onLoadDone: () =>
                {
                    DialogueManager.StopAllConversations();
                    DialogueManager.StartConversation(conversationToResume);
                });
            }
            else
            {
                SceneManager.LoadScene("MainStoryScene");
                DialogueManager.StopAllConversations();
                DialogueManager.StartConversation(conversationToResume);
            }
        }
        else
        {
            Debug.Log("ResumeConversation: 回到 ExploreScene");

            if (TransitionManager.Instance != null)
            {
                DialogueManager.StopAllConversations();
                TransitionManager.Instance.LoadSceneWithTransition("ExploreScene", TransitionType.Cover);
            }
            else
            {
                DialogueManager.StopAllConversations();
                SceneManager.LoadScene("ExploreScene");
            }
        }
    }

    public void SwitchScene(string sceneName)
    {
        Debug.Log($"切換場景到 {sceneName}");

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.LoadSceneWithTransition(sceneName, TransitionType.Cover);
        }
        else
        {
            SceneManager.LoadScene(sceneName); // 後備方案
        }
    }

    public void SwitchSceneAndConversation(string sceneName, string conversationName)
    {
        Debug.Log($"切換場景到 {sceneName} 並啟動對話 {conversationName}");

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.LoadSceneWithTransition(sceneName, TransitionType.LoadingScreen, onLoadDone: () =>
            {
                // 場景載入完成後啟動對話
                DialogueManager.StartConversation(conversationName);
            });
        }
        else
        {
            SceneManager.LoadScene(sceneName); // 後備方案
            DialogueManager.StartConversation(conversationName);
        }
    }

    // ===== 遊戲狀態管理 =====

    private int score = 0;

    public int AddScore(int amount)
    {
        score += amount;
        Debug.Log($"得分: {score}");
        // 更新 UI 顯示分數

        return score;
    }

    public int GetScore()
    {
        return score;
    }

    // ===== 角色狀態（委派給 PlayerStatusManager）=====
    // 保留向後相容性，實際邏輯已移至 PlayerStatusManager

    public int StatusHeart => PlayerStatusManager.Instance?.StatusHeart ?? 50;
    public int StatusMoney => PlayerStatusManager.Instance?.StatusMoney ?? 50;
    public int StatusEnergy => PlayerStatusManager.Instance?.StatusEnergy ?? 50;

    public event PlayerStatusManager.StatusChangedHandler OnStatusHeartChanged
    {
        add => PlayerStatusManager.Instance.OnStatusHeartChanged += value;
        remove => PlayerStatusManager.Instance.OnStatusHeartChanged -= value;
    }

    public event PlayerStatusManager.StatusChangedHandler OnStatusMoneyChanged
    {
        add => PlayerStatusManager.Instance.OnStatusMoneyChanged += value;
        remove => PlayerStatusManager.Instance.OnStatusMoneyChanged -= value;
    }

    public event PlayerStatusManager.StatusChangedHandler OnStatusEnergyChanged
    {
        add => PlayerStatusManager.Instance.OnStatusEnergyChanged += value;
        remove => PlayerStatusManager.Instance.OnStatusEnergyChanged -= value;
    }

    public void UpdateStatusHeart(int amount)
    {
        PlayerStatusManager.Instance?.UpdateStatusHeart(amount);
    }

    public void UpdateStatusMoney(int amount)
    {
        PlayerStatusManager.Instance?.UpdateStatusMoney(amount);
    }

    public void UpdateStatusEnergy(int amount)
    {
        PlayerStatusManager.Instance?.UpdateStatusEnergy(amount);
    }

    // ===== 遊戲結束 =====

    /// <summary>
    /// 開啟指定網址
    /// </summary>
    /// <param name="url">網址</param>
    public void OpenURL(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("網址為空");
            return;
        }

        Debug.Log($"開啟網址: {url}");
        Application.OpenURL(url);
    }

}