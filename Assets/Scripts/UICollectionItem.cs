using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Collection Item Prefab 的控制腳本
/// 管理單一收集項目的顯示
/// </summary>
public class UICollectionItem : MonoBehaviour
{
    [Header("UI References - 請在 Inspector 設定這些 child objects")]
    [SerializeField] private TextMeshProUGUI noText;
    [SerializeField] private Image animalImage;
    [SerializeField] private TextMeshProUGUI captureCountText;

    [Header("Display Colors")]
    [SerializeField] private Color collectedColor = new Color(1f, 1f, 1f, 1f); // #FFFFFF
    [SerializeField] private Color seenColor = new Color(1f, 1f, 1f, 0.102f); // #FFFFFF1A (透明度約10%)
    [SerializeField] private Color lockedColor = Color.white; // #FFFFFF (備用顏色)

    [Header("Silhouette Settings")]
    [Tooltip("剪影用的 Material（用於 not seen 狀態）")]
    [SerializeField] private Material silhouetteMaterial;

    private AnimalData animalData;

    /// <summary>
    /// 設定動物資料並更新顯示
    /// </summary>
    public void Setup(AnimalData data)
    {
        animalData = data;
        UpdateDisplay();
    }

    /// <summary>
    /// 根據 AnimalData 更新完整顯示
    /// </summary>
    private void UpdateDisplay()
    {
        if (animalData == null) return;

        // 設定序號
        if (noText != null)
        {
            UpdateNo(animalData.no);
        }

        // 設定圖片
        if (animalImage != null && animalData.mainSprite != null)
        {
            UpdateImage(animalData.mainSprite);
        }

        // 設定捕獲次數（假設使用 captureSprites 的長度作為捕獲次數）
        if (captureCountText != null)
        {
            int captureCount = animalData.captureSprites != null ? animalData.captureSprites.Length : 0;
            UpdateCaptureCount(captureCount);
        }

        // 根據收集狀態設定圖片顏色和材質
        if (animalImage != null)
        {
            if (animalData.collected)
            {
                // 已收集：#FFFFFF (完全不透明的白色) + 移除 Material
                animalImage.material = null;
                animalImage.color = collectedColor;
            }
            else if (animalData.seen)
            {
                // 已遇見但未收集：#FFFFFF1A (稍微透明的白色) + 移除 Material
                animalImage.material = null;
                animalImage.color = seenColor;
            }
            else
            {
                animalImage.color = lockedColor;
                // 未遇見也未收集：使用剪影效果
                if (silhouetteMaterial != null)
                {
                    // 使用剪影 Material
                    animalImage.material = silhouetteMaterial;
                }
            }
        }
    }

    /// <summary>
    /// 手動更新序號文字
    /// </summary>
    public void UpdateNo(string no)
    {
        if (noText != null)
        {
            noText.text = $"no. <size=36>{no}</size>";
        }
    }

    /// <summary>
    /// 手動更新圖片
    /// </summary>
    public void UpdateImage(Sprite sprite)
    {
        if (animalImage != null)
        {
            animalImage.sprite = sprite;
        }
    }

    /// <summary>
    /// 手動更新捕獲次數
    /// </summary>
    public void UpdateCaptureCount(int count)
    {
        if (captureCountText != null)
        {
            captureCountText.text = $"{count}/2";
        }
    }
}
