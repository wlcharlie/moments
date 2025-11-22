using UnityEngine;
using UnityEngine.Playables;      // PlayableDirector
using UnityEngine.SceneManagement; // 換場景用
using UnityEngine.UI;              // Image, Canvas
using TMPro;                       // TextMeshProUGUI
using System.Collections;

public class WorkRoutineController : MonoBehaviour
{
    [Header("設定")]
    public PlayableDirector director;   // 把 TL_WorkRoutine 那個 PlayableDirector 拉進來
    public int loopCount = 1;            // 要播幾遍
    public string nextSceneName;        // 下一個場景名稱（在 Build Settings 裡要有）

    [Header("轉場設定")]
    [Tooltip("Cover 可能會有藍色閃現，建議使用 LoadingScreen 或 Splash")]
    public TransitionType transitionType = TransitionType.LoadingScreen;  // 場景切換時的轉場類型

    [Header("淡入效果（Timeline 播放前）")]
    [Tooltip("用於淡入的 CanvasGroup（可選）。如果為空，會嘗試使用 TransitionManager 的 CoverBackground，或自動創建一個。")]
    public CanvasGroup fade;            // 用於淡入的 CanvasGroup（可選，Timeline 播放前的效果）
    public float fadeTime = 0.3f;       // 淡入時間

    [Header("跳過功能")]
    public KeyCode skipKey = KeyCode.Space;  // 跳過按鍵

    int currentCount = 0;
    bool isFadingOut = false;
    bool isInitialized = false;  // 標記是否已初始化，避免初始化時的 Stop() 觸發事件

    void Start()
    {
        // 只在運行時執行（編輯器預覽時不執行）
        if (!Application.isPlaying)
            return;

        // 確保有指定
        if (director == null)
        {
            director = FindFirstObjectByType<PlayableDirector>();
        }

        // 如果還是找不到，報錯
        if (director == null)
        {
            Debug.LogError("WorkRoutineController: 找不到 PlayableDirector！請在 Inspector 中指定或確保場景中有 PlayableDirector。");
            return;
        }

        // 如果 Play On Awake 開啟了，先停止 Timeline（在訂閱事件之前，避免觸發事件）
        if (director.state == PlayState.Playing)
        {
            director.Stop();
            director.time = 0;
        }

        // 設定 Wrap Mode 為 None，確保播放完後會停止並觸發 stopped 事件
        // 如果設為 Loop，Timeline 會一直循環而不會觸發 stopped 事件
        director.extrapolationMode = DirectorWrapMode.None;

        // 監聽 Timeline 播放結束事件（在停止之後訂閱，避免觸發）
        director.stopped += OnTimelineStopped;

        // 如果沒有指定 Fade，嘗試使用 TransitionManager 的背景物件
        if (fade == null && TransitionManager.Instance != null)
        {
            // 嘗試從 TransitionManager 獲取背景物件
            fade = GetFadeCanvasGroupFromTransitionManager();
        }

        // 標記已初始化
        isInitialized = true;

        // 開始淡入並播放
        StartCoroutine(StartSequence());
    }

    IEnumerator StartSequence()
    {
        // 淡入（從黑屏到正常）
        yield return Fade(1, 0);

        // 如果沒勾 Play On Awake，在這裡手動開播
        if (director.state != PlayState.Playing)
        {
            director.time = 0;
            director.Play();
        }
    }

    void Update()
    {
        // 只在運行時執行
        if (!Application.isPlaying)
            return;

        // 處理跳過功能
        if (director != null && director.state == PlayState.Playing && !isFadingOut)
        {
            if (Input.GetKeyDown(skipKey))
            {
                // 跳到最後 0.05 秒，讓 Timeline 自然結束
                director.time = director.duration - 0.05f;
            }
        }
    }

    void OnTimelineStopped(PlayableDirector pd)
    {
        // 如果還沒初始化完成，忽略這個事件（可能是初始化時的 Stop() 觸發的）
        if (!isInitialized)
            return;

        // 每播完一次 +1
        currentCount++;

        if (currentCount < loopCount)
        {
            // 還沒播夠次數 → 重頭再播一次（不需要淡出）
            director.time = 0;
            director.Play();
        }
        else
        {
            // 播完指定次數 → 淡出並換場景
            StartCoroutine(EndSequence());
        }
    }

    IEnumerator EndSequence()
    {
        isFadingOut = true;
        
        if (string.IsNullOrEmpty(nextSceneName))
            yield break;
        
        // 檢查 TransitionManager 是否存在
        if (TransitionManager.Instance == null)
        {
            Debug.LogWarning("WorkRoutineController: TransitionManager.Instance 為 null，使用簡單黑色遮罩後切換場景");
            // 如果沒有 TransitionManager，創建一個簡單的黑色遮罩來避免藍色閃現
            yield return StartCoroutine(SimpleFadeOutAndLoadScene(nextSceneName));
            yield break;
        }
        
        Debug.Log($"WorkRoutineController: 開始轉場到 {nextSceneName}，轉場類型: {transitionType}");
        
        // 使用 TransitionManager 的轉場效果
        // LoadSceneWithTransition 內部會處理所有轉場邏輯，包括場景載入
        TransitionManager.Instance.LoadSceneWithTransition(nextSceneName, transitionType);
        
        // 注意：LoadSceneWithTransition 是異步的，場景切換後這個 GameObject 會被銷毀
        // 所以這裡不需要等待轉場完成，TransitionManager 會處理一切
        yield break;
    }

    IEnumerator Fade(float from, float to)
    {
        if (fade == null) yield break;

        float t = 0;
        fade.alpha = from;
        
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            fade.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        
        fade.alpha = to;
    }

    /// <summary>
    /// 簡單的淡出並載入場景（當 TransitionManager 不存在時使用）
    /// </summary>
    private IEnumerator SimpleFadeOutAndLoadScene(string sceneName)
    {
        // 創建一個簡單的黑色遮罩
        GameObject fadeCanvasObj = new GameObject("TempFadeCanvas");
        Canvas fadeCanvas = fadeCanvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999; // 確保在最上層
        
        CanvasScaler scaler = fadeCanvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        fadeCanvasObj.AddComponent<GraphicRaycaster>();
        
        // 創建黑色遮罩 Image
        GameObject fadeImageObj = new GameObject("FadeImage");
        fadeImageObj.transform.SetParent(fadeCanvasObj.transform, false);
        Image fadeImage = fadeImageObj.AddComponent<Image>();
        fadeImage.color = Color.black;
        
        RectTransform rectTransform = fadeImageObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        
        // 創建 Loading 文字（置中顯示）
        GameObject loadingTextObj = new GameObject("LoadingText");
        loadingTextObj.transform.SetParent(fadeCanvasObj.transform, false);
        TextMeshProUGUI loadingText = loadingTextObj.AddComponent<TextMeshProUGUI>();
        loadingText.text = "Loading...";
        loadingText.fontSize = 60;
        loadingText.color = Color.white;
        loadingText.alignment = TextAlignmentOptions.Center;
        loadingText.horizontalAlignment = HorizontalAlignmentOptions.Center;
        loadingText.verticalAlignment = VerticalAlignmentOptions.Middle;
        
        // 設定文字位置（螢幕中央）
        RectTransform textRect = loadingTextObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(400, 100);
        textRect.anchoredPosition = Vector2.zero; // 置中
        
        // 確保文字在黑色遮罩上方（透過設定 Canvas 的 sortingOrder 已確保）
        
        // 確保 Loading 文字一開始就顯示（alpha = 1）
        Color textColor = loadingText.color;
        textColor.a = 1f;
        loadingText.color = textColor;
        
        // 快速淡出（0.1 秒）
        float fadeTime = 0.1f;
        float elapsed = 0f;
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeTime);
            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;
            yield return null;
        }
        
        // 確保完全不透明
        Color finalColor = fadeImage.color;
        finalColor.a = 1f;
        fadeImage.color = finalColor;
        
        // 載入場景
        SceneManager.LoadScene(sceneName);
        
        // 場景載入後，遮罩會被自動銷毀（因為是新場景）
    }

    /// <summary>
    /// 嘗試從 TransitionManager 獲取或創建 CanvasGroup
    /// </summary>
    private CanvasGroup GetFadeCanvasGroupFromTransitionManager()
    {
        if (TransitionManager.Instance == null)
            return null;

        // 使用反射或直接訪問來獲取 coverBackground
        // 由於 coverBackground 是 private，我們需要通過其他方式
        // 最簡單的方式是在 TransitionManager 中添加一個公開方法
        // 或者我們可以直接在場景中找到 TransitionManager 的子物件
        
        GameObject transitionManagerObj = TransitionManager.Instance.gameObject;
        if (transitionManagerObj != null)
        {
            // 查找 CoverBackground 或 SplashBackground
            Transform coverBg = transitionManagerObj.transform.Find("CoverBackground");
            if (coverBg != null)
            {
                CanvasGroup cg = coverBg.GetComponent<CanvasGroup>();
                if (cg == null)
                {
                    // 如果沒有 CanvasGroup，添加一個
                    cg = coverBg.gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = 1f; // 初始為不透明（黑屏）
                    cg.interactable = false;
                    cg.blocksRaycasts = false;
                }
                return cg;
            }
        }
        
        return null;
    }

    private void OnDestroy()
    {
        if (director != null)
        {
            director.stopped -= OnTimelineStopped;
        }
    }
}
