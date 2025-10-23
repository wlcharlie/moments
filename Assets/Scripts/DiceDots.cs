using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 顯示骰子點點的視覺效果 (1-6點)
/// </summary>
public class DiceDots : MonoBehaviour
{
    [Header("點點設定")]
    [SerializeField] private GameObject[] dots; // 9個點點位置（最多需要6個，但用9宮格佈局方便）
    [SerializeField] private Color dotColor = Color.black; // 點點顏色
    [SerializeField] private float dotSize = 15f; // 點點大小
    [SerializeField] private float dotSpacing = 24f; // 點點之間的間距

    private void Awake()
    {
        // 在 Awake 中創建點點，確保即使 GameObject disabled 也能初始化
        EnsureDotsCreated();
    }

    private void Start()
    {
        // 確保點點已創建
        EnsureDotsCreated();
    }

    /// <summary>
    /// 確保點點已經創建
    /// </summary>
    private void EnsureDotsCreated()
    {
        // 如果沒有手動設置點點，自動創建
        if (dots == null || dots.Length == 0)
        {
            CreateDots();
        }
    }

    /// <summary>
    /// 設置顯示的點數 (1-6)
    /// </summary>
    public void SetDotNumber(int number)
    {
        // 確保點點已創建（處理第一次調用時可能還沒初始化的情況）
        EnsureDotsCreated();

        if (number < 1 || number > 6)
        {
            Debug.LogWarning($"骰子點數必須在 1-6 之間，收到: {number}");
            return;
        }

        // 先隱藏所有點點
        HideAllDots();

        // 根據點數顯示對應的點點
        // 使用 9 宮格佈局：
        // 0 1 2
        // 3 4 5
        // 6 7 8
        switch (number)
        {
            case 1:
                ShowDot(4); // 中心
                break;
            case 2:
                ShowDot(0); // 左上
                ShowDot(8); // 右下
                break;
            case 3:
                ShowDot(0); // 左上
                ShowDot(4); // 中心
                ShowDot(8); // 右下
                break;
            case 4:
                ShowDot(0); // 左上
                ShowDot(2); // 右上
                ShowDot(6); // 左下
                ShowDot(8); // 右下
                break;
            case 5:
                ShowDot(0); // 左上
                ShowDot(2); // 右上
                ShowDot(4); // 中心
                ShowDot(6); // 左下
                ShowDot(8); // 右下
                break;
            case 6:
                ShowDot(0); // 左上
                ShowDot(2); // 右上
                ShowDot(3); // 左中
                ShowDot(5); // 右中
                ShowDot(6); // 左下
                ShowDot(8); // 右下
                break;
        }
    }

    /// <summary>
    /// 隱藏所有點點
    /// </summary>
    private void HideAllDots()
    {
        if (dots == null) return;

        foreach (GameObject dot in dots)
        {
            if (dot != null)
            {
                dot.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 顯示指定位置的點點
    /// </summary>
    private void ShowDot(int index)
    {
        if (dots != null && index >= 0 && index < dots.Length && dots[index] != null)
        {
            dots[index].SetActive(true);
        }
    }

    /// <summary>
    /// 自動創建 9 個點點位置（3x3 網格）
    /// </summary>
    private void CreateDots()
    {
        dots = new GameObject[9];
        RectTransform parentRect = GetComponent<RectTransform>();

        if (parentRect == null)
        {
            Debug.LogError("DiceDots 需要在有 RectTransform 的 GameObject 上");
            return;
        }

        Vector2[] positions = new Vector2[9]
        {
            new Vector2(-dotSpacing, dotSpacing),   // 0: 左上
            new Vector2(0, dotSpacing),           // 1: 中上
            new Vector2(dotSpacing, dotSpacing),     // 2: 右上
            new Vector2(-dotSpacing, 0),          // 3: 左中
            new Vector2(0, 0),                 // 4: 中心
            new Vector2(dotSpacing, 0),           // 5: 右中
            new Vector2(-dotSpacing, -dotSpacing),   // 6: 左下
            new Vector2(0, -dotSpacing),          // 7: 中下
            new Vector2(dotSpacing, -dotSpacing)     // 8: 右下
        };

        for (int i = 0; i < 9; i++)
        {
            // 創建點點 GameObject
            GameObject dot = new GameObject($"Dot_{i}");
            dot.transform.SetParent(transform);

            // 添加 RectTransform
            RectTransform dotRect = dot.AddComponent<RectTransform>();
            dotRect.anchorMin = new Vector2(0.5f, 0.5f);
            dotRect.anchorMax = new Vector2(0.5f, 0.5f);
            dotRect.pivot = new Vector2(0.5f, 0.5f);
            dotRect.anchoredPosition = positions[i];
            dotRect.sizeDelta = new Vector2(dotSize, dotSize);
            dotRect.localScale = Vector3.one;

            // 添加 Image 組件顯示圓點
            Image dotImage = dot.AddComponent<Image>();
            dotImage.color = dotColor;

            // 使用圓形 sprite（如果有的話）或者純色
            // 你可以在 Unity 中手動指定圓形的 sprite

            // 使用 UIRadialGradient 也可以做出圓形效果
            // 或者簡單用正方形暫時代替

            dots[i] = dot;
            dot.SetActive(false); // 初始隱藏
        }
    }

    /// <summary>
    /// 在編輯器中預覽點數
    /// </summary>
    [ContextMenu("預覽 1 點")]
    private void Preview1() => SetDotNumber(1);

    [ContextMenu("預覽 2 點")]
    private void Preview2() => SetDotNumber(2);

    [ContextMenu("預覽 3 點")]
    private void Preview3() => SetDotNumber(3);

    [ContextMenu("預覽 4 點")]
    private void Preview4() => SetDotNumber(4);

    [ContextMenu("預覽 5 點")]
    private void Preview5() => SetDotNumber(5);

    [ContextMenu("預覽 6 點")]
    private void Preview6() => SetDotNumber(6);
}
