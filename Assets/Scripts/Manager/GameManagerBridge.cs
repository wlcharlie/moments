using UnityEngine;

/// <summary>
/// 轉發呼叫給 GameManager Singleton
/// 用於 UI 事件綁定，避免直接引用 Singleton
/// 使用方式：在 Unity Editor 建立 ScriptableObject asset，然後在 UI Button 的 onClick 事件中引用靜態方法
/// </summary>
[CreateAssetMenu(fileName = "GameManagerBridge", menuName = "Bridges/Game Manager Bridge")]
public class GameManagerBridge : ScriptableObject
{
    // 靜態方法可以直接被 Unity UI 事件系統引用

    public static void OnStartButtonClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStartButtonClicked();
    }

    public static void OnStoryModeButtonClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStoryModeButtonClicked();
    }

    public static void OnEventModeButtonClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnEventModeButtonClicked();
    }

    public static void OnDebugModeButtonClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnDebugModeButtonClicked();
    }

    public static void OnFollowUsButtonClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnFollowUsButtonClicked();
    }

    public static void OpenURL(string url)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OpenURL(url);
    }
}
