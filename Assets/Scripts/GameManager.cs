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

    // ===== 通用方法（可傳參數）=====

    public void ShowPanel(GameObject panel)
    {
        panel.SetActive(true);
        Debug.Log($"顯示 Panel: {panel.name}");
    }

    public void HidePanel(GameObject panel)
    {
        panel.SetActive(false);
        Debug.Log($"隱藏 Panel: {panel.name}");
    }

    public void TogglePanel(GameObject panel)
    {
        panel.SetActive(!panel.activeSelf);
        Debug.Log($"切換 Panel: {panel.name} - 現在是 {panel.activeSelf}");
    }

    // ===== 遊戲狀態管理 =====

    private int score = 0;

    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log($"得分: {score}");
        // 更新 UI 顯示分數
    }

    public int GetScore()
    {
        return score;
    }
}