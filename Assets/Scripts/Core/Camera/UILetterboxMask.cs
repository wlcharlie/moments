using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UILetterboxMask : MonoBehaviour
{
    public static UILetterboxMask Instance { get; private set; }

    [SerializeField] private RectTransform topBar;
    [SerializeField] private RectTransform bottomBar;
    [SerializeField] private RectTransform leftBar;
    [SerializeField] private RectTransform rightBar;
    [SerializeField] private GameObject persistRoot;

    private CanvasScaler canvasScaler;
    private float referenceWidth => canvasScaler != null ? canvasScaler.referenceResolution.x : 786f;

    void Start()
    {
        if (Application.isPlaying)
        {
            if (Instance == null)
            {
                Instance = this;
                if (persistRoot != null)
                    DontDestroyOnLoad(persistRoot);
            }
            else
            {
                Destroy(persistRoot != null ? persistRoot : gameObject);
            }
        }
    }

    void OnEnable()
    {
        canvasScaler = GetComponentInParent<CanvasScaler>();
        CameraFitInside.OnCameraAdjusted += UpdateBars;
        UpdateBars(CameraFitInside.LastLetterboxWidth, CameraFitInside.LastLetterboxHeight);
    }

    void OnDisable()
    {
        CameraFitInside.OnCameraAdjusted -= UpdateBars;
    }

    void UpdateBars(float letterboxWidth = 0f, float letterboxHeight = 0f)
    {
        if (Camera.main == null) return;

        // Canvas Scaler Match=0 時，縮放比例 = referenceWidth / Screen.width
        float scaleFactor = referenceWidth / Screen.width;
        float canvasHeight = Screen.height * scaleFactor;

        // 世界單位轉 Canvas 單位
        float unitToCanvas = canvasHeight / (Camera.main.orthographicSize * 2f);

        float barWidth = letterboxWidth * unitToCanvas;
        float barHeight = letterboxHeight * unitToCanvas;

        if (leftBar != null && rightBar != null)
        {
            leftBar.gameObject.SetActive(barWidth > 0);
            rightBar.gameObject.SetActive(barWidth > 0);
            leftBar.sizeDelta = new Vector2(barWidth, canvasHeight);
            rightBar.sizeDelta = new Vector2(barWidth, canvasHeight);
        }

        if (topBar != null && bottomBar != null)
        {
            topBar.gameObject.SetActive(barHeight > 0);
            bottomBar.gameObject.SetActive(barHeight > 0);
            topBar.sizeDelta = new Vector2(referenceWidth, barHeight);
            bottomBar.sizeDelta = new Vector2(referenceWidth, barHeight);
        }
    }
}