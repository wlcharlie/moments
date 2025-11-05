using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum TransitionType
{
    Cover,         // 滑動遮罩效果
    Splash,        // 白色閃光效果
    LoadingScreen  // 僅顯示載入畫面
}

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    [SerializeField] private RectTransform coverBackground;
    [SerializeField] private RectTransform splashBackground;
    [SerializeField] private LoadingScreen loadingScreen;
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private float splashDuration = 0.3f;

    private bool isTransitioning = false;
    private Image splashImage;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (coverBackground != null)
        {
            // 初始位置：右側外面 (left: 786, right: 0)
            coverBackground.offsetMin = new Vector2(786, coverBackground.offsetMin.y);
            coverBackground.offsetMax = new Vector2(0, coverBackground.offsetMax.y);
            // 預設停用
            coverBackground.gameObject.SetActive(false);
        }

        if (splashBackground != null)
        {
            splashImage = splashBackground.GetComponent<Image>();
            if (splashImage != null)
            {
                // 初始設定：完全透明
                Color color = splashImage.color;
                color.a = 0f;
                splashImage.color = color;
            }
            // 預設停用
            splashBackground.gameObject.SetActive(false);
        }

        if (loadingScreen != null)
        {
            // 預設停用
            loadingScreen.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 載入場景並執行過渡動畫
    /// </summary>
    /// <param name="sceneName">要載入的場景名稱</param>
    /// <param name="transitionType">轉場類型（預設為 Cover）</param>
    public void LoadSceneWithTransition(string sceneName, TransitionType transitionType = TransitionType.Cover)
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionCoroutine(sceneName, transitionType));
        }
    }

    /// <summary>
    /// 執行過渡動畫的協程
    /// </summary>
    private IEnumerator TransitionCoroutine(string sceneName, TransitionType transitionType)
    {
        isTransitioning = true;


        switch (transitionType)
        {
            case TransitionType.Cover:
                yield return StartCoroutine(CoverTransition(sceneName));
                break;

            case TransitionType.Splash:
                yield return StartCoroutine(SplashTransition(sceneName));
                break;

            case TransitionType.LoadingScreen:
                yield return StartCoroutine(LoadingScreenTransition(sceneName));
                break;
        }

        isTransitioning = false;
    }

    /// <summary>
    /// Cover 轉場：滑動遮罩效果
    /// </summary>
    private IEnumerator CoverTransition(string sceneName)
    {
        // 階段 1: 從右邊滑入至中間
        yield return StartCoroutine(SlideIn());

        // 非同步載入場景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // 等待場景載入完成
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 階段 2: 從中間滑出至左邊
        yield return StartCoroutine(SlideOut());
    }

    /// <summary>
    /// Splash 轉場：白色閃光效果
    /// </summary>
    private IEnumerator SplashTransition(string sceneName)
    {
        // 非同步載入場景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // 等待場景載入完成
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 白色閃光效果
        yield return StartCoroutine(SplashEffect());
    }

    /// <summary>
    /// LoadingScreen 轉場：僅顯示載入畫面和進度條
    /// </summary>
    private IEnumerator LoadingScreenTransition(string sceneName)
    {
        // 顯示載入畫面
        ShowLoadingScreen();

        yield return new WaitForSeconds(0.4f);

        // 非同步載入場景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // 等待場景載入完成，同時更新進度條
        while (!asyncLoad.isDone)
        {
            UpdateLoadingProgress(asyncLoad.progress);
            yield return null;
        }

        // 確保進度達到 100%
        UpdateLoadingProgress(1f);

        // 短暫延遲讓使用者看到 100%
        yield return new WaitForSeconds(1f);

        // 隱藏載入畫面
        HideLoadingScreen();
    }

    /// <summary>
    /// 從右邊滑入 (left 從 786 遞減至 0)
    /// </summary>
    private IEnumerator SlideIn()
    {
        // 啟用 coverBackground
        if (coverBackground != null)
        {
            coverBackground.gameObject.SetActive(true);
        }

        float elapsed = 0f;
        Vector2 startMin = new Vector2(786, coverBackground.offsetMin.y);
        Vector2 startMax = new Vector2(0, coverBackground.offsetMax.y);
        Vector2 endMin = new Vector2(0, coverBackground.offsetMin.y);
        Vector2 endMax = new Vector2(0, coverBackground.offsetMax.y);

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            coverBackground.offsetMin = Vector2.Lerp(startMin, endMin, t);
            coverBackground.offsetMax = Vector2.Lerp(startMax, endMax, t);

            yield return null;
        }

        coverBackground.offsetMin = endMin;
        coverBackground.offsetMax = endMax;
    }

    /// <summary>
    /// 從中間滑出至左邊 (right 從 0 遞增至 786)
    /// </summary>
    private IEnumerator SlideOut()
    {
        float elapsed = 0f;
        Vector2 startMin = new Vector2(0, coverBackground.offsetMin.y);
        Vector2 startMax = new Vector2(0, coverBackground.offsetMax.y);
        Vector2 endMin = new Vector2(0, coverBackground.offsetMin.y);
        Vector2 endMax = new Vector2(-786, coverBackground.offsetMax.y);

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            coverBackground.offsetMin = Vector2.Lerp(startMin, endMin, t);
            coverBackground.offsetMax = Vector2.Lerp(startMax, endMax, t);

            yield return null;
        }

        coverBackground.offsetMin = endMin;
        coverBackground.offsetMax = endMax;

        // 重置回初始位置 (left: 786, right: 0)，準備下次使用
        coverBackground.offsetMin = new Vector2(786, coverBackground.offsetMin.y);
        coverBackground.offsetMax = new Vector2(0, coverBackground.offsetMax.y);

        // 停用 coverBackground
        if (coverBackground != null)
        {
            coverBackground.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 白色閃光效果 (opacity 0 -> 1 -> 0)
    /// </summary>
    private IEnumerator SplashEffect()
    {
        if (splashImage == null)
        {
            yield break;
        }

        // 啟用 splashBackground
        if (splashBackground != null)
        {
            splashBackground.gameObject.SetActive(true);
        }

        float halfDuration = splashDuration / 2f;
        float elapsed = 0f;

        // 淡入 (0 -> 1)
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            Color color = splashImage.color;
            color.a = Mathf.Lerp(0f, 1f, t);
            splashImage.color = color;
            yield return null;
        }

        // 確保達到完全不透明
        Color maxColor = splashImage.color;
        maxColor.a = 1f;
        splashImage.color = maxColor;

        elapsed = 0f;

        // 淡出 (1 -> 0)
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            Color color = splashImage.color;
            color.a = Mathf.Lerp(1f, 0f, t);
            splashImage.color = color;
            yield return null;
        }

        // 確保完全透明
        Color endColor = splashImage.color;
        endColor.a = 0f;
        splashImage.color = endColor;

        // 停用 splashBackground
        if (splashBackground != null)
        {
            splashBackground.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 顯示載入畫面
    /// </summary>
    private void ShowLoadingScreen()
    {
        if (loadingScreen != null)
        {
            loadingScreen.gameObject.SetActive(true);
            loadingScreen.ResetProgress();
        }
    }

    /// <summary>
    /// 隱藏載入畫面
    /// </summary>
    private void HideLoadingScreen()
    {
        if (loadingScreen != null)
        {
            loadingScreen.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 更新載入進度
    /// </summary>
    /// <param name="progress">進度值 (0-1)</param>
    private void UpdateLoadingProgress(float progress)
    {
        if (loadingScreen != null)
        {
            loadingScreen.UpdateProgress(progress);
        }
    }

    /// <summary>
    /// 啟動過渡效果（僅滑入動畫）
    /// </summary>
    public void StartTransition()
    {
        if (!isTransitioning)
        {
            StartCoroutine(SlideIn());
        }
    }
}
