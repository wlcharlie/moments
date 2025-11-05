using UnityEngine;
using TMPro;

public class LoadingDots : MonoBehaviour
{
    [Header("Target")]
    public TextMeshProUGUI loadingText;
    [Header("Content")]
    public string baseText = "讀取中";
    public int maxDots = 3;
    [Header("Speed")]
    public float interval = 0.3f;   // 每幀點點間隔秒數
    public bool useUnscaledTime = true; // Loading 期間常用
    [Header("Effects")]
    public bool breathingAlpha = true; // 字體「呼吸」效果
    public float breathingMinAlpha = 0.6f;
    public float breathingMaxAlpha = 1f;

    float timer;
    int dots;

    void Reset()
    {
        loadingText = GetComponent<TextMeshProUGUI>();
    }

    void Awake()
    {
        // Auto-assign when not wired in Inspector.
        if (loadingText == null)
        {
            loadingText = GetComponent<TextMeshProUGUI>();
            if (loadingText == null)
            {
                loadingText = GetComponentInChildren<TextMeshProUGUI>(true);
            }
            if (loadingText == null)
            {
                Debug.LogError("[LoadingDots] Missing TextMeshProUGUI reference.", this);
            }
        }
    }

    void Update()
    {
        if (loadingText == null) return;

        // 平滑節奏：依時間直接計算目前應顯示的點數，避免硬跳的累加與重置
        float t = useUnscaledTime ? Time.unscaledTime : Time.time;
        int newDots = Mathf.FloorToInt((t / interval) % (maxDots + 1));
        if (newDots != dots)
        {
            dots = newDots;
            loadingText.text = baseText + new string('.', dots);
        }

        // 可選：字體顏色微閃（呼吸感）
        if (breathingAlpha)
        {
            float phase = Mathf.PingPong(t, 1f);
            loadingText.alpha = Mathf.Lerp(breathingMinAlpha, breathingMaxAlpha, phase);
        }
    }
}
