using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mini Map 單個格子
/// </summary>
public class MiniMapTile : MonoBehaviour
{
    [Header("組件引用")]
    [SerializeField] private Image dotImage; // 底部圓點
    [SerializeField] private Image eventIconImage; // 上方物件圖示

    [Header("視覺設定")]
    [SerializeField] private Color normalDotColor = new Color(1f, 1f, 1f, 0.5f); // 普通狀態（半透明白色）
    [SerializeField] private Color passedDotColor = new Color(1f, 1f, 1f, 1f); // 已經過的格子（不透明白色）
    [SerializeField] private Color currentDotColor = new Color(0.2f, 0.8f, 1f, 1f); // 當前位置（藍色）

    [Header("格子資訊")]
    [SerializeField] private int tileIndex = 0; // 格子索引（0 = 起點）
    [SerializeField] private Sprite eventIcon; // 物件圖示

    private bool isPassed = false; // 是否已經過
    private bool isCurrent = false; // 是否當前位置

    private void Start()
    {
        UpdateVisual();
    }

    /// <summary>
    /// 設定格子索引
    /// </summary>
    public void SetTileIndex(int index)
    {
        tileIndex = index;
    }

    /// <summary>
    /// 獲取格子索引
    /// </summary>
    public int GetTileIndex()
    {
        return tileIndex;
    }

    /// <summary>
    /// 設定物件圖示
    /// </summary>
    public void SetEventIcon(Sprite icon)
    {
        eventIcon = icon;
        if (eventIconImage != null)
        {
            eventIconImage.sprite = icon;
            eventIconImage.enabled = (icon != null);
        }
    }

    /// <summary>
    /// 設定為當前位置
    /// </summary>
    public void SetCurrent(bool current)
    {
        isCurrent = current;
        if (current)
        {
            isPassed = true; // 當前位置自動標記為已經過
        }
        UpdateVisual();
    }

    /// <summary>
    /// 設定為已經過
    /// </summary>
    public void SetPassed(bool passed)
    {
        isPassed = passed;
        UpdateVisual();
    }

    /// <summary>
    /// 更新視覺效果
    /// </summary>
    private void UpdateVisual()
    {
        if (dotImage == null) return;

        if (isCurrent)
        {
            dotImage.color = currentDotColor;
        }
        else if (isPassed)
        {
            dotImage.color = passedDotColor;
        }
        else
        {
            dotImage.color = normalDotColor;
        }
    }

    /// <summary>
    /// 獲取格子的世界位置（用於玩家移動）
    /// </summary>
    public Vector3 GetWorldPosition()
    {
        return transform.position;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateVisual();
    }
#endif
}
