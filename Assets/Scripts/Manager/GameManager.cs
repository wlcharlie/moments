using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton 模式
    public static GameManager Instance { get; private set; }

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

        GameObject uiTitleSceneButtons = GameObject.Find("UITitleSceneButtons");
        if (uiTitleSceneButtons != null)
        {
            Transform menuTransform = uiTitleSceneButtons.transform.Find("Menu");
            if (menuTransform != null)
            {
                menuTransform.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("在 UITitleSceneButtons 下找不到 'Menu' 子物件");
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
        // TODO: 後續補上邏輯
    }

    public void OnDebugModeButtonClicked()
    {
        Debug.Log("進入偵錯模式");
        TransitionManager.Instance.LoadSceneWithTransition("DialogueDebugScene", TransitionType.LoadingScreen);
    }

    public void OnFollowUsButtonClicked()
    {
        Debug.Log("關注我們");
        // TODO: 後續補上邏輯
    }

    // ===== 流程 管理 =====

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

    // 角色狀態
    private int statusHeart = 50;
    private int statusMoney = 50;
    private int statusEnergy = 50;

    public int StatusHeart { get => statusHeart; }
    public int StatusMoney { get => statusMoney; }
    public int StatusEnergy { get => statusEnergy; }

    // 角色狀態監聽
    public delegate void StatusChangedHandler(int newValue);
    public event StatusChangedHandler OnStatusHeartChanged;
    public event StatusChangedHandler OnStatusMoneyChanged;
    public event StatusChangedHandler OnStatusEnergyChanged;

    public void UpdateStatusHeart(int amount)
    {
        statusHeart = Mathf.Clamp(statusHeart + amount, 0, 100);
        OnStatusHeartChanged?.Invoke(statusHeart);
        Debug.Log($"心情值: {statusHeart}");
    }

    public void UpdateStatusMoney(int amount)
    {
        statusMoney = Mathf.Clamp(statusMoney + amount, 0, 100);
        OnStatusMoneyChanged?.Invoke(statusMoney);
        Debug.Log($"金錢值: {statusMoney}");
    }

    public void UpdateStatusEnergy(int amount)
    {
        statusEnergy = Mathf.Clamp(statusEnergy + amount, 0, 100);
        OnStatusEnergyChanged?.Invoke(statusEnergy);
        Debug.Log($"體力值: {statusEnergy}");
    }
}