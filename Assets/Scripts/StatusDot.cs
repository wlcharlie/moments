using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 圓形狀態指示器，用於 Traffic Scene 的 Status Bar
/// </summary>
public class StatusDot : MonoBehaviour
{
    [Header("組件引用")]
    [SerializeField] private Image backgroundCircle; // 背景圓形（顏色）
    [SerializeField] private Image fillImage; // 填充進度條（與背景同色，用 fillAmount 控制）
    [SerializeField] private Image borderImage; // 外框圖片 (Dots_Border)
    [SerializeField] private Image iconImage; // 狀態圖示 (Heart/Money/Energy)

    [Header("狀態設定")]
    [SerializeField] private StatusType statusType = StatusType.Heart;

    // 狀態顏色常數
    private readonly Color heartColor = new Color(1f, 0.675f, 0.612f); // #FFAC9C
    private readonly Color moneyColor = new Color(0.576f, 0.851f, 0.749f); // #93D9BF
    private readonly Color energyColor = new Color(1f, 0.906f, 0.478f); // #FFE77A

    private void Start()
    {
        UpdateVisual();
        SubscribeToPlayerStats();
        UpdateFillAmount(); // 初始化填充進度
    }

    private void OnDestroy()
    {
        UnsubscribeFromPlayerStats();
    }

    /// <summary>
    /// 訂閱 PlayerStats 事件
    /// </summary>
    private void SubscribeToPlayerStats()
    {
        if (PlayerStats.Instance == null) return;

        switch (statusType)
        {
            case StatusType.Heart:
                PlayerStats.Instance.OnHeartChanged += OnValueChanged;
                break;
            case StatusType.Money:
                PlayerStats.Instance.OnMoneyChanged += OnValueChanged;
                break;
            case StatusType.Energy:
                PlayerStats.Instance.OnEnergyChanged += OnValueChanged;
                break;
        }
    }

    /// <summary>
    /// 取消訂閱 PlayerStats 事件
    /// </summary>
    private void UnsubscribeFromPlayerStats()
    {
        if (PlayerStats.Instance == null) return;

        switch (statusType)
        {
            case StatusType.Heart:
                PlayerStats.Instance.OnHeartChanged -= OnValueChanged;
                break;
            case StatusType.Money:
                PlayerStats.Instance.OnMoneyChanged -= OnValueChanged;
                break;
            case StatusType.Energy:
                PlayerStats.Instance.OnEnergyChanged -= OnValueChanged;
                break;
        }
    }

    /// <summary>
    /// 當數值變化時的回調
    /// </summary>
    private void OnValueChanged(int currentValue, int maxValue)
    {
        UpdateFillAmount();
    }

    /// <summary>
    /// 設定狀態類型
    /// </summary>
    public void SetStatusType(StatusType type)
    {
        statusType = type;
        UpdateVisual();
    }

    /// <summary>
    /// 更新視覺效果（顏色）
    /// </summary>
    private void UpdateVisual()
    {
        Color statusColor = GetStatusColor();

        if (backgroundCircle != null)
        {
            backgroundCircle.color = statusColor;
        }

        if (fillImage != null)
        {
            fillImage.color = statusColor;
        }
    }

    /// <summary>
    /// 更新填充進度條
    /// </summary>
    private void UpdateFillAmount()
    {
        if (fillImage == null) return;
        if (PlayerStats.Instance == null) return;

        int currentValue = 0;
        int maxValue = 0;

        switch (statusType)
        {
            case StatusType.Heart:
                currentValue = PlayerStats.Instance.GetHeart();
                maxValue = PlayerStats.Instance.GetMaxHeart();
                break;
            case StatusType.Money:
                currentValue = PlayerStats.Instance.GetMoney();
                maxValue = PlayerStats.Instance.GetMaxMoney();
                break;
            case StatusType.Energy:
                currentValue = PlayerStats.Instance.GetEnergy();
                maxValue = PlayerStats.Instance.GetMaxEnergy();
                break;
        }

        // 計算填充百分比
        float fillAmount = maxValue > 0 ? (float)currentValue / maxValue : 0f;
        fillImage.fillAmount = Mathf.Clamp01(fillAmount);
    }

    /// <summary>
    /// 獲取狀態對應的顏色
    /// </summary>
    private Color GetStatusColor()
    {
        return statusType switch
        {
            StatusType.Heart => heartColor,
            StatusType.Money => moneyColor,
            StatusType.Energy => energyColor,
            _ => Color.white
        };
    }

    /// <summary>
    /// 設定圖示 Sprite
    /// </summary>
    public void SetIcon(Sprite iconSprite)
    {
        if (iconImage != null)
        {
            iconImage.sprite = iconSprite;
        }
    }

    /// <summary>
    /// 設定外框 Sprite
    /// </summary>
    public void SetBorder(Sprite borderSprite)
    {
        if (borderImage != null)
        {
            borderImage.sprite = borderSprite;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 在編輯器中即時預覽
        UpdateVisual();
    }
#endif
}
