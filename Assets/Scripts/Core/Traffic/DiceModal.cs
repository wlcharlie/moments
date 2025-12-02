using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using PixelCrushers.DialogueSystem;

/// <summary>
/// 控制骰子 Modal 的顯示、隱藏和結果文字
/// </summary>
public class DiceModal : MonoBehaviour
{
    [Header("組件引用")]
    [SerializeField] private CanvasGroup canvasGroup; // 使用 CanvasGroup 控制顯示/隱藏
    [SerializeField] private DiceRoller diceRoller; // 骰子滾動控制器
    [SerializeField] private TextMeshProUGUI resultText; // 顯示「移動 X 格！」的文字

    [Header("動畫設定")]
    [SerializeField] private float resultTextDisplayDuration = 1.5f; // 結果文字顯示時長

    private bool isInitialized = false;
    private bool shouldStartHidden = true; // 記錄是否應該初始隱藏

    private void Awake()
    {
        // 使用 Awake 確保即使 GameObject disabled 也能初始化引用
        Initialize();
    }

    private void OnEnable()
    {
        // 當 GameObject 被啟用時，如果還沒執行過 Start，不要隱藏
        // 這是為了處理第一次從 disabled 啟用的情況
    }

    private void Start()
    {
        // 確保初始化
        if (!isInitialized)
        {
            Initialize();
        }

        // 只有在應該初始隱藏時才隱藏（避免干擾 Show() 的調用）
        if (shouldStartHidden)
        {
            Hide();
            shouldStartHidden = false;
        }
    }

    private void Initialize()
    {
        if (isInitialized) return;

        // 訂閱骰子完成事件
        if (diceRoller != null)
        {
            diceRoller.OnDiceRollComplete += OnDiceRollComplete;
        }

        // 初始隱藏結果文字
        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }

        isInitialized = true;
    }

    private void OnDestroy()
    {
        // 取消訂閱
        if (diceRoller != null)
        {
            diceRoller.OnDiceRollComplete -= OnDiceRollComplete;
        }
    }

    /// <summary>
    /// 顯示 Modal 並開始擲骰子
    /// </summary>
    public void ShowAndRoll()
    {
        // 確保初始化完成
        if (!isInitialized)
        {
            Initialize();
        }

        if (diceRoller != null && diceRoller.IsRolling())
        {
            Debug.LogWarning("骰子正在轉動中，無法再次擲骰");
            return;
        }

        Show();

        // 隱藏結果文字
        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }

        // 開始擲骰子
        if (diceRoller != null)
        {
            diceRoller.RollDice();
        }
    }

    /// <summary>
    /// 顯示 Modal
    /// </summary>
    public void Show()
    {
        // 標記不要在 Start 中隱藏
        shouldStartHidden = false;

        // 先確保 GameObject 本身是啟用的（從外部呼叫時可能是 disabled）
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    /// <summary>
    /// 隱藏 Modal
    /// </summary>
    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// 骰子轉動完成時的回調
    /// </summary>
    private void OnDiceRollComplete(int result)
    {
        Debug.Log($"骰子結果: {result}，準備顯示結果文字");
        StartCoroutine(ShowResultAndHide(result));
    }

    /// <summary>
    /// 顯示結果文字，然後隱藏 Modal
    /// </summary>
    private IEnumerator ShowResultAndHide(int result)
    {
        // 顯示結果文字
        if (resultText != null)
        {
            resultText.text = $"移動 {result} 格！";
            resultText.gameObject.SetActive(true);
        }

        // 等待一段時間
        yield return new WaitForSeconds(resultTextDisplayDuration);

        // 隱藏 Modal
        Hide();

        // 通知遊戲管理器移動玩家
        TrafficGameManager manager = FindFirstObjectByType<TrafficGameManager>();
        if (manager != null)
        {
            manager.MovePlayer(result);
        }

        // 切回 MainStoryScene 並從存檔繼續對話
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeConversation();
        }
    }



    /// <summary>
    /// 手動觸發擲骰子（供外部按鈕調用）
    /// </summary>
    public void OnRollButtonClicked()
    {
        ShowAndRoll();
    }
}
