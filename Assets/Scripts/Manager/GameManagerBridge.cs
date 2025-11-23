using UnityEngine;

/// <summary>
/// 轉發呼叫給 GameManager Singleton
/// 用於 UI 事件綁定，避免直接引用 Singleton
/// </summary>
public class GameManagerBridge : MonoBehaviour
{
    public void OnStartButtonClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStartButtonClicked();
    }

    public void OnStoryModeButtonClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStoryModeButtonClicked();
    }

    public void OnEventModeButtonClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnEventModeButtonClicked();
    }

    public void OnDebugModeButtonClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnDebugModeButtonClicked();
    }

    public void OnFollowUsButtonClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnFollowUsButtonClicked();
    }
}
