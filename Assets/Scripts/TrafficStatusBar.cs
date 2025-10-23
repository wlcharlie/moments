using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Traffic Scene 上方狀態列管理器
/// </summary>
public class TrafficStatusBar : MonoBehaviour
{
    [Header("UI 組件")]
    [SerializeField] private Image characterImage; // 人物圖片
    [SerializeField] private TextMeshProUGUI rollCountText; // 前進次數文字
    [SerializeField] private StatusDot heartDot; // 心情狀態點
    [SerializeField] private StatusDot moneyDot; // 金錢狀態點
    [SerializeField] private StatusDot energyDot; // 活力狀態點

    [Header("圖片資源")]
    [SerializeField] private Sprite characterSprite; // walk.png
    [SerializeField] private Sprite heartIcon; // Heart.png
    [SerializeField] private Sprite moneyIcon; // Money.png
    [SerializeField] private Sprite energyIcon; // Energy.png
    [SerializeField] private Sprite dotsBorder; // Dots_Border.png

    private void Start()
    {
        InitializeUI();
    }

    /// <summary>
    /// 初始化 UI
    /// </summary>
    private void InitializeUI()
    {
        // 設定人物圖片
        if (characterImage != null && characterSprite != null)
        {
            characterImage.sprite = characterSprite;
        }

        // 設定狀態點圖示和外框
        if (heartDot != null)
        {
            heartDot.SetStatusType(StatusType.Heart);
            if (heartIcon != null) heartDot.SetIcon(heartIcon);
            if (dotsBorder != null) heartDot.SetBorder(dotsBorder);
        }

        if (moneyDot != null)
        {
            moneyDot.SetStatusType(StatusType.Money);
            if (moneyIcon != null) moneyDot.SetIcon(moneyIcon);
            if (dotsBorder != null) moneyDot.SetBorder(dotsBorder);
        }

        if (energyDot != null)
        {
            energyDot.SetStatusType(StatusType.Energy);
            if (energyIcon != null) energyDot.SetIcon(energyIcon);
            if (dotsBorder != null) energyDot.SetBorder(dotsBorder);
        }
    }

    /// <summary>
    /// 更新前進次數顯示
    /// </summary>
    public void UpdateRollCount(int current, int max)
    {
        if (rollCountText != null)
        {
            rollCountText.text = $"前進次數 {current}/{max}";
        }
    }

    /// <summary>
    /// 設定人物圖片
    /// </summary>
    public void SetCharacterSprite(Sprite sprite)
    {
        characterSprite = sprite;
        if (characterImage != null)
        {
            characterImage.sprite = sprite;
        }
    }
}
