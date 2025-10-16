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
        Debug.Log("開始遊戲");
        SceneManager.LoadScene("MainStoryScene"); // 載入遊戲場景
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
    private int statusHeart = 100;
    private int statusMoney = 50;
    private int statusEnergy = 75;

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