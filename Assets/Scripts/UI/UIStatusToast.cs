using UnityEngine;
using System.Collections;
using TMPro;

public class UIStatusToast : MonoBehaviour
{
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private TextMeshProUGUI statusName;
    [SerializeField] private UIStatus statusIcon;
    [SerializeField] private UIArrow statusArrow;


    private RectTransform rectTransform;

    private Coroutine hideCoroutine;

    // 追蹤前一次的值以判斷增減
    private int previousHeartValue;
    private int previousMoneyValue;
    private int previousEnergyValue;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(-150, -154); // 初始隱藏位置

        // 訂閱狀態變化事件
        if (PlayerStatusManager.Instance != null)
        {
            // 初始化前一次的值
            previousHeartValue = PlayerStatusManager.Instance.StatusHeart;
            previousMoneyValue = PlayerStatusManager.Instance.StatusMoney;
            previousEnergyValue = PlayerStatusManager.Instance.StatusEnergy;

            PlayerStatusManager.Instance.OnStatusHeartChanged += OnStatusHeartChanged;
            PlayerStatusManager.Instance.OnStatusMoneyChanged += OnStatusMoneyChanged;
            PlayerStatusManager.Instance.OnStatusEnergyChanged += OnStatusEnergyChanged;
        }
    }

    void OnDestroy()
    {
        if (PlayerStatusManager.Instance != null)
        {
            PlayerStatusManager.Instance.OnStatusHeartChanged -= OnStatusHeartChanged;
            PlayerStatusManager.Instance.OnStatusMoneyChanged -= OnStatusMoneyChanged;
            PlayerStatusManager.Instance.OnStatusEnergyChanged -= OnStatusEnergyChanged;
        }
    }

    private void OnStatusHeartChanged(int newValue)
    {
        Debug.Log($"心情值改變為 {newValue}，顯示 Toast");
        bool isIncreasing = newValue > previousHeartValue;
        ShowToast(StatusType.Heart, "心情", newValue, isIncreasing);
        previousHeartValue = newValue;
    }

    private void OnStatusMoneyChanged(int newValue)
    {
        Debug.Log($"金錢改變為 {newValue}，顯示 Toast");
        bool isIncreasing = newValue > previousMoneyValue;
        ShowToast(StatusType.Money, "金錢", newValue, isIncreasing);
        previousMoneyValue = newValue;
    }

    private void OnStatusEnergyChanged(int newValue)
    {
        Debug.Log($"活力改變為 {newValue}，顯示 Toast");
        bool isIncreasing = newValue > previousEnergyValue;
        ShowToast(StatusType.Energy, "活力", newValue, isIncreasing);
        previousEnergyValue = newValue;
    }

    private void ShowToast(StatusType type, string name, int value, bool isIncreasing)
    {
        // 更新狀態圖示
        if (statusIcon != null)
        {
            statusIcon.SetStatusType(type);
            // 將值標準化為 0-1 範圍（狀態值範圍是 0-100）
            statusIcon.SetValue(value / 100f);
        }


        // 更新狀態名稱
        if (statusName != null)
        {
            statusName.text = name;
        }

        // 更新箭頭方向（會自動調整位置）
        if (statusArrow != null)
        {
            // true = 向上箭頭（增加），false = 向下箭頭（減少）
            statusArrow.SetDirection(isIncreasing);
        }

        // 停止之前的隱藏協程
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        // 顯示 Toast
        rectTransform.anchoredPosition = new Vector2(99, -154); // 移動到顯示位置

        // 開始新的隱藏協程
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        rectTransform.anchoredPosition = new Vector2(-150, -154); // 移動到隱藏位置
    }
}
