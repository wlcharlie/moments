using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Traffic 場景的遊戲管理器
/// 管理擲骰次數、玩家位置等遊戲狀態
/// </summary>
public class TrafficGameManager : MonoBehaviour
{
    [Header("遊戲設定")]
    [SerializeField] private int maxRollCount = 3; // 最大擲骰次數
    [SerializeField] private int totalMapPositions = 7; // 地圖總格數（根據設計圖調整）

    [Header("UI 引用")]
    [SerializeField] private Button goButton; // Go 按鈕
    [SerializeField] private DiceModal diceModal; // 骰子 Modal
    [SerializeField] private TrafficStatusBar statusBar; // 上方狀態列
    [SerializeField] private MiniMap miniMap; // 下方小地圖

    [Header("遊戲狀態")]
    private int currentRollCount = 0; // 當前已使用的擲骰次數
    private int playerPosition = 0; // 玩家當前位置（0 = 起點）

    // 事件：玩家移動完成
    public System.Action<int> OnPlayerMoved;

    private void Start()
    {
        // 初始化
        currentRollCount = 0;
        playerPosition = 0;

        // 綁定 Go 按鈕事件
        if (goButton != null)
        {
            goButton.onClick.AddListener(OnGoButtonClicked);
        }

        // 訂閱 Mini Map 移動完成事件
        if (miniMap != null)
        {
            miniMap.OnPlayerMoveComplete += OnMiniMapMoveComplete;

            // 同步格子數量到地圖
            miniMap.SetTileCount(totalMapPositions);
        }

        UpdateRollCountDisplay();
    }

    private void OnDestroy()
    {
        // 取消訂閱
        if (miniMap != null)
        {
            miniMap.OnPlayerMoveComplete -= OnMiniMapMoveComplete;
        }
    }

    /// <summary>
    /// Mini Map 移動完成回調
    /// </summary>
    private void OnMiniMapMoveComplete(int tileIndex)
    {
        Debug.Log($"玩家到達格子 {tileIndex}");
        // TODO: 這裡可以觸發對應格子的事件
    }

    /// <summary>
    /// Go 按鈕點擊事件
    /// </summary>
    private void OnGoButtonClicked()
    {
        // 檢查是否還有擲骰次數
        if (currentRollCount >= maxRollCount)
        {
            Debug.Log("已經用完所有擲骰次數");
            // TODO: 顯示提示或進入下一階段
            return;
        }

        // 檢查是否已經到達終點
        if (playerPosition >= totalMapPositions)
        {
            Debug.Log("已經到達終點");
            return;
        }

        // 增加使用次數
        currentRollCount++;
        UpdateRollCountDisplay();

        // 顯示骰子 Modal 並開始擲骰
        if (diceModal != null)
        {
            diceModal.ShowAndRoll();
        }

        // 如果用完次數，禁用按鈕
        if (currentRollCount >= maxRollCount && goButton != null)
        {
            goButton.interactable = false;
        }
    }

    /// <summary>
    /// 移動玩家
    /// 由 DiceModal 在骰子完成後調用
    /// </summary>
    public void MovePlayer(int steps)
    {
        int previousPosition = playerPosition;
        playerPosition += steps;

        // 限制在地圖範圍內
        playerPosition = Mathf.Clamp(playerPosition, 0, totalMapPositions);

        Debug.Log($"玩家從位置 {previousPosition} 移動到 {playerPosition}");

        // 更新 Mini Map 上的玩家位置
        if (miniMap != null)
        {
            miniMap.MovePlayer(steps);
        }

        // 觸發事件
        OnPlayerMoved?.Invoke(playerPosition);

        // 檢查是否到達終點
        if (playerPosition >= totalMapPositions)
        {
            Debug.Log("到達終點！");
            OnReachEnd();
        }
    }

    /// <summary>
    /// 更新剩餘次數顯示
    /// </summary>
    private void UpdateRollCountDisplay()
    {
        // 更新 Status Bar 的前進次數
        if (statusBar != null)
        {
            int remaining = maxRollCount - currentRollCount;
            statusBar.UpdateRollCount(remaining, maxRollCount);
        }

        Debug.Log($"擲骰次數: {currentRollCount}/{maxRollCount}");
    }

    /// <summary>
    /// 到達終點時的處理
    /// </summary>
    private void OnReachEnd()
    {
        Debug.Log("遊戲結束！");
        // TODO: 顯示結束畫面或進入下一場景
    }

    /// <summary>
    /// 重置遊戲（用於測試）
    /// </summary>
    [ContextMenu("重置遊戲")]
    public void ResetGame()
    {
        currentRollCount = 0;
        playerPosition = 0;

        if (goButton != null)
        {
            goButton.interactable = true;
        }

        UpdateRollCountDisplay();
        Debug.Log("遊戲已重置");
    }

    /// <summary>
    /// 獲取當前玩家位置
    /// </summary>
    public int GetPlayerPosition()
    {
        return playerPosition;
    }

    /// <summary>
    /// 獲取剩餘擲骰次數
    /// </summary>
    public int GetRemainingRollCount()
    {
        return maxRollCount - currentRollCount;
    }
}
