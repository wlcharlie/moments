using UnityEngine;

/// <summary>
/// 管理遊戲的目標幀率
/// 在遊戲啟動時設定為 60 FPS
/// </summary>
public class FrameRateManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("目標幀率，-1 表示不限制")]
    private int targetFrameRate = 60;

    void Awake()
    {
        Application.targetFrameRate = targetFrameRate;
        Debug.Log($"目標幀率已設定為: {targetFrameRate} FPS");
    }
}
