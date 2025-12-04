using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UISafeAreaHandler : MonoBehaviour
{
    [SerializeField] private RectTransform safeAreaRect;

#if UNITY_EDITOR
    [Header("Editor Debug")]
    [SerializeField] private bool showDebugBorder = true;
    [SerializeField] private Color debugBorderColor = Color.cyan;
    [SerializeField] private float borderWidth = 4f;

    private RectTransform debugBorderContainer;
    private Image[] borderImages;
#endif

    public static Vector2 SafeAreaMin { get; private set; }
    public static Vector2 SafeAreaMax { get; private set; }

    private Rect lastSafeArea;
    private Vector2Int lastScreenSize;

    void OnEnable()
    {
        ApplySafeArea();
    }

#if UNITY_EDITOR
    void OnDisable()
    {
        DestroyDebugBorder();
    }
#endif

    void Update()
    {
        if (Screen.width != lastScreenSize.x ||
            Screen.height != lastScreenSize.y ||
            Screen.safeArea != lastSafeArea)
        {
            ApplySafeArea();
        }

#if UNITY_EDITOR
        UpdateDebugBorder();
#endif
    }

    void ApplySafeArea()
    {
        if (safeAreaRect == null) return;
        if (Screen.width <= 0 || Screen.height <= 0) return;

        Rect safeArea = Screen.safeArea;
        lastSafeArea = safeArea;
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);

        Vector2 anchorMin = new(
            safeArea.x / Screen.width,
            safeArea.y / Screen.height
        );
        Vector2 anchorMax = new(
            (safeArea.x + safeArea.width) / Screen.width,
            (safeArea.y + safeArea.height) / Screen.height
        );

        safeAreaRect.anchorMin = anchorMin;
        safeAreaRect.anchorMax = anchorMax;
        safeAreaRect.offsetMin = Vector2.zero;
        safeAreaRect.offsetMax = Vector2.zero;

        SafeAreaMin = anchorMin;
        SafeAreaMax = anchorMax;
    }

#if UNITY_EDITOR
    void CreateDebugBorder()
    {
        if (safeAreaRect == null || !showDebugBorder) return;

        DestroyDebugBorder();

        // Create container
        var containerGO = new GameObject("SafeAreaDebugBorder");
        containerGO.hideFlags = HideFlags.DontSave;
        debugBorderContainer = containerGO.AddComponent<RectTransform>();
        debugBorderContainer.SetParent(safeAreaRect, false);
        debugBorderContainer.anchorMin = Vector2.zero;
        debugBorderContainer.anchorMax = Vector2.one;
        debugBorderContainer.offsetMin = Vector2.zero;
        debugBorderContainer.offsetMax = Vector2.zero;

        // Create 4 border images (top, bottom, left, right)
        borderImages = new Image[4];
        string[] names = { "Top", "Bottom", "Left", "Right" };

        for (int i = 0; i < 4; i++)
        {
            var borderGO = new GameObject($"Border{names[i]}");
            borderGO.hideFlags = HideFlags.DontSave;
            var rect = borderGO.AddComponent<RectTransform>();
            rect.SetParent(debugBorderContainer, false);

            var img = borderGO.AddComponent<Image>();
            img.color = debugBorderColor;
            img.raycastTarget = false;
            borderImages[i] = img;
        }

        SetupBorderAnchors();
    }

    void SetupBorderAnchors()
    {
        if (borderImages == null || borderImages.Length < 4) return;
        if (float.IsNaN(borderWidth) || borderWidth <= 0) borderWidth = 4f;

        // Top border
        var top = borderImages[0].rectTransform;
        top.anchorMin = new Vector2(0, 1);
        top.anchorMax = new Vector2(1, 1);
        top.pivot = new Vector2(0.5f, 1);
        top.sizeDelta = new Vector2(0, borderWidth);
        top.anchoredPosition = Vector2.zero;

        // Bottom border
        var bottom = borderImages[1].rectTransform;
        bottom.anchorMin = new Vector2(0, 0);
        bottom.anchorMax = new Vector2(1, 0);
        bottom.pivot = new Vector2(0.5f, 0);
        bottom.sizeDelta = new Vector2(0, borderWidth);
        bottom.anchoredPosition = Vector2.zero;

        // Left border
        var left = borderImages[2].rectTransform;
        left.anchorMin = new Vector2(0, 0);
        left.anchorMax = new Vector2(0, 1);
        left.pivot = new Vector2(0, 0.5f);
        left.sizeDelta = new Vector2(borderWidth, 0);
        left.anchoredPosition = Vector2.zero;

        // Right border
        var right = borderImages[3].rectTransform;
        right.anchorMin = new Vector2(1, 0);
        right.anchorMax = new Vector2(1, 1);
        right.pivot = new Vector2(1, 0.5f);
        right.sizeDelta = new Vector2(borderWidth, 0);
        right.anchoredPosition = Vector2.zero;
    }

    void UpdateDebugBorder()
    {
        if (!showDebugBorder)
        {
            DestroyDebugBorder();
            return;
        }

        if (debugBorderContainer == null && showDebugBorder)
        {
            CreateDebugBorder();
            return;
        }

        if (borderImages != null)
        {
            foreach (var img in borderImages)
            {
                if (img != null)
                    img.color = debugBorderColor;
            }
        }
    }

    void DestroyDebugBorder()
    {
        if (debugBorderContainer != null)
        {
            DestroyImmediate(debugBorderContainer.gameObject);
            debugBorderContainer = null;
            borderImages = null;
        }
    }

    void OnValidate()
    {
        if (debugBorderContainer != null)
        {
            SetupBorderAnchors();
            UpdateDebugBorder();
        }
    }
#endif
}
