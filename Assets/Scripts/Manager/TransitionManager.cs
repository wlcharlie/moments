using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum TransitionType
{
    Cover,         // 滑動遮罩效果
    Splash,        // 白色閃光效果
    LoadingScreen, // 僅顯示載入畫面
    FadeIn         // 淡入效果（僅限 Additive 模式）
}

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    [SerializeField] private RectTransform coverBackground;
    [SerializeField] private RectTransform splashBackground;
    [SerializeField] private LoadingScreen loadingScreen;
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private float splashDuration = 0.3f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

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
    /// <param name="onLoadDone">場景載入完成後的回調函數（可選）</param>
    public void LoadSceneWithTransition(string sceneName, TransitionType transitionType = TransitionType.Cover, Action onLoadDone = null)
    {
        LoadSceneWithTransition(sceneName, transitionType, LoadSceneMode.Single, onLoadDone);
    }

    /// <summary>
    /// 載入場景並執行過渡動畫（支援 Additive 模式）
    /// </summary>
    /// <param name="sceneName">要載入的場景名稱</param>
    /// <param name="transitionType">轉場類型</param>
    /// <param name="loadSceneMode">場景載入模式</param>
    /// <param name="onLoadDone">場景載入完成後的回調函數（可選）</param>
    public void LoadSceneWithTransition(string sceneName, TransitionType transitionType, LoadSceneMode loadSceneMode, Action onLoadDone = null)
    {
        // FadeIn 只能在 Additive 模式下使用
        if (transitionType == TransitionType.FadeIn && loadSceneMode != LoadSceneMode.Additive)
        {
            Debug.LogWarning("TransitionManager: FadeIn 轉場類型只能在 Additive 模式下使用，已自動切換為 Additive 模式");
            loadSceneMode = LoadSceneMode.Additive;
        }

        if (!isTransitioning)
        {
            StartCoroutine(TransitionCoroutine(sceneName, transitionType, loadSceneMode, onLoadDone));
        }
    }

    /// <summary>
    /// 執行過渡動畫的協程
    /// </summary>
    private IEnumerator TransitionCoroutine(string sceneName, TransitionType transitionType, LoadSceneMode loadSceneMode, Action onLoadDone)
    {
        isTransitioning = true;

        switch (transitionType)
        {
            case TransitionType.Cover:
                yield return StartCoroutine(CoverTransition(sceneName, loadSceneMode, onLoadDone));
                break;

            case TransitionType.Splash:
                yield return StartCoroutine(SplashTransition(sceneName, loadSceneMode, onLoadDone));
                break;

            case TransitionType.LoadingScreen:
                yield return StartCoroutine(LoadingScreenTransition(sceneName, loadSceneMode, onLoadDone));
                break;

            case TransitionType.FadeIn:
                yield return StartCoroutine(FadeInTransition(sceneName, onLoadDone));
                break;
        }

        isTransitioning = false;
    }

    /// <summary>
    /// Cover 轉場：滑動遮罩效果
    /// </summary>
    private IEnumerator CoverTransition(string sceneName, LoadSceneMode loadSceneMode, Action onLoadDone)
    {
        // 階段 1: 從右邊滑入至中間
        yield return StartCoroutine(SlideIn());

        // 非同步載入場景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);

        // 等待場景載入完成
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 場景載入完成，觸發回調
        onLoadDone?.Invoke();

        // 階段 2: 從中間滑出至左邊
        yield return StartCoroutine(SlideOut());
    }

    /// <summary>
    /// Splash 轉場：白色閃光效果
    /// </summary>
    private IEnumerator SplashTransition(string sceneName, LoadSceneMode loadSceneMode, Action onLoadDone)
    {
        // 非同步載入場景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);

        // 等待場景載入完成
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 場景載入完成，觸發回調
        onLoadDone?.Invoke();

        // 白色閃光效果
        yield return StartCoroutine(SplashEffect());
    }

    /// <summary>
    /// LoadingScreen 轉場：僅顯示載入畫面和進度條
    /// </summary>
    private IEnumerator LoadingScreenTransition(string sceneName, LoadSceneMode loadSceneMode, Action onLoadDone)
    {
        // 顯示載入畫面
        ShowLoadingScreen();

        yield return new WaitForSeconds(0.4f);

        // 非同步載入場景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);

        // 等待場景載入完成，同時更新進度條
        while (!asyncLoad.isDone)
        {
            UpdateLoadingProgress(asyncLoad.progress);
            yield return null;
        }

        // 確保進度達到 100%
        UpdateLoadingProgress(1f);

        // 場景載入完成，觸發回調
        onLoadDone?.Invoke();

        // 短暫延遲讓使用者看到 100%
        yield return new WaitForSeconds(1f);

        // 隱藏載入畫面
        HideLoadingScreen();
    }

    /// <summary>
    /// FadeIn 轉場：淡入效果（僅限 Additive 模式）
    /// </summary>
    private IEnumerator FadeInTransition(string sceneName, Action onLoadDone)
    {
        // 非同步載入場景（Additive 模式）
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // 等待場景載入完成
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 取得新載入的場景
        Scene loadedScene = SceneManager.GetSceneByName(sceneName);

        // 對場景中所有根物件的 CanvasGroup 執行淡入
        yield return StartCoroutine(FadeInSceneCanvasGroups(loadedScene));

        // 場景載入完成，觸發回調
        onLoadDone?.Invoke();
    }

    /// <summary>
    /// 對場景中所有 CanvasGroup 執行淡入效果
    /// </summary>
    private IEnumerator FadeInSceneCanvasGroups(Scene scene)
    {
        if (!scene.IsValid())
        {
            yield break;
        }

        // 取得場景中所有根物件的 CanvasGroup
        GameObject[] rootObjects = scene.GetRootGameObjects();
        System.Collections.Generic.List<CanvasGroup> canvasGroups = new System.Collections.Generic.List<CanvasGroup>();

        foreach (GameObject root in rootObjects)
        {
            CanvasGroup cg = root.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                canvasGroups.Add(cg);
            }
        }

        // 如果沒有 CanvasGroup，直接結束
        if (canvasGroups.Count == 0)
        {
            yield break;
        }

        // 執行淡入動畫
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            float alpha = Mathf.Lerp(0f, 1f, t);

            foreach (CanvasGroup cg in canvasGroups)
            {
                if (cg != null)
                {
                    cg.alpha = alpha;
                }
            }

            yield return null;
        }

        // 確保完全不透明
        foreach (CanvasGroup cg in canvasGroups)
        {
            if (cg != null)
            {
                cg.alpha = 1f;
            }
        }
    }

    /// <summary>
    /// 卸載 Additive 場景（帶淡出效果）
    /// </summary>
    /// <param name="sceneName">要卸載的場景名稱</param>
    /// <param name="useFadeOut">是否使用淡出效果（預設為 true）</param>
    /// <param name="onUnloadDone">卸載完成後的回調函數（可選）</param>
    public void UnloadScene(string sceneName, bool useFadeOut = true, Action onUnloadDone = null)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogWarning($"TransitionManager: 場景 '{sceneName}' 不存在或未載入");
            onUnloadDone?.Invoke();
            return;
        }

        StartCoroutine(UnloadSceneCoroutine(scene, useFadeOut, onUnloadDone));
    }

    /// <summary>
    /// 卸載場景的協程
    /// </summary>
    private IEnumerator UnloadSceneCoroutine(Scene scene, bool useFadeOut, Action onUnloadDone)
    {
        if (useFadeOut)
        {
            // 執行淡出效果
            yield return StartCoroutine(FadeOutSceneCanvasGroups(scene));
        }

        // 卸載場景
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(scene);

        // 等待卸載完成
        while (!asyncUnload.isDone)
        {
            yield return null;
        }

        // 卸載完成，觸發回調
        onUnloadDone?.Invoke();
    }

    /// <summary>
    /// 對場景中所有 CanvasGroup 執行淡出效果
    /// </summary>
    private IEnumerator FadeOutSceneCanvasGroups(Scene scene)
    {
        if (!scene.IsValid())
        {
            yield break;
        }

        // 取得場景中所有根物件的 CanvasGroup
        GameObject[] rootObjects = scene.GetRootGameObjects();
        System.Collections.Generic.List<CanvasGroup> canvasGroups = new System.Collections.Generic.List<CanvasGroup>();

        foreach (GameObject root in rootObjects)
        {
            CanvasGroup cg = root.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                canvasGroups.Add(cg);
            }
        }

        // 如果沒有 CanvasGroup，直接結束
        if (canvasGroups.Count == 0)
        {
            yield break;
        }

        // 執行淡出動畫
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            float alpha = Mathf.Lerp(1f, 0f, t);

            foreach (CanvasGroup cg in canvasGroups)
            {
                if (cg != null)
                {
                    cg.alpha = alpha;
                }
            }

            yield return null;
        }

        // 確保完全透明
        foreach (CanvasGroup cg in canvasGroups)
        {
            if (cg != null)
            {
                cg.alpha = 0f;
            }
        }
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

    /// <summary>
    /// 用於在對話之間切換時提供視覺轉場
    /// </summary>
    /// <param name="transitionType">轉場類型</param>
    /// <param name="onTransitionDone">轉場完成後的回調函數</param>
    public void DoConversationTransition(TransitionType transitionType, Action onTransitionDone = null)
    {
        if (!isTransitioning)
        {
            StartCoroutine(ConversationTransitionCoroutine(transitionType, onTransitionDone));
        }
        else
        {
            // 如果正在轉場，直接執行回調
            onTransitionDone?.Invoke();
        }
    }

    /// <summary>
    /// 對話轉場協程
    /// </summary>
    private IEnumerator ConversationTransitionCoroutine(TransitionType transitionType, Action onTransitionDone)
    {
        isTransitioning = true;

        switch (transitionType)
        {
            case TransitionType.Cover:
                // Cover 轉場：滑入 -> 滑出
                yield return StartCoroutine(SlideIn());
                onTransitionDone?.Invoke(); // Callback after slide in
                yield return StartCoroutine(SlideOut());
                break;

            case TransitionType.Splash:
                // Splash 轉場：白色閃光
                onTransitionDone?.Invoke(); // Callback before splash effect
                yield return StartCoroutine(SplashEffect());
                break;

            case TransitionType.FadeIn:
                // FadeIn 轉場：淡出 -> 淡入（需要找到對話 UI 的 CanvasGroup）
                yield return StartCoroutine(FadeOutDialogueUI());
                onTransitionDone?.Invoke(); // Callback after fade out
                yield return StartCoroutine(FadeInDialogueUI());
                break;

            default:
                // 預設：直接執行回調，無轉場效果
                onTransitionDone?.Invoke();
                break;
        }

        isTransitioning = false;
    }

    /// <summary>
    /// 淡出對話 UI
    /// </summary>
    private IEnumerator FadeOutDialogueUI()
    {
        // 嘗試找到 Dialogue UI 的 CanvasGroup
        CanvasGroup dialogueCanvasGroup = FindDialogueUICanvasGroup();
        if (dialogueCanvasGroup == null)
        {
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            dialogueCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        dialogueCanvasGroup.alpha = 0f;
    }

    /// <summary>
    /// 淡入對話 UI
    /// </summary>
    private IEnumerator FadeInDialogueUI()
    {
        // 嘗試找到 Dialogue UI 的 CanvasGroup
        CanvasGroup dialogueCanvasGroup = FindDialogueUICanvasGroup();
        if (dialogueCanvasGroup == null)
        {
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            dialogueCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        dialogueCanvasGroup.alpha = 1f;
    }

    /// <summary>
    /// 尋找對話 UI 的 CanvasGroup
    /// </summary>
    private CanvasGroup FindDialogueUICanvasGroup()
    {
        // 嘗試從 DialogueManager 獲取 UI
        if (PixelCrushers.DialogueSystem.DialogueManager.instance != null &&
            PixelCrushers.DialogueSystem.DialogueManager.instance.dialogueUI != null)
        {
            GameObject uiGameObject = (PixelCrushers.DialogueSystem.DialogueManager.instance.dialogueUI as MonoBehaviour)?.gameObject;
            if (uiGameObject != null)
            {
                CanvasGroup cg = uiGameObject.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    return cg;
                }

                // 如果 UI GameObject 本身沒有 CanvasGroup，嘗試在子物件中尋找
                cg = uiGameObject.GetComponentInChildren<CanvasGroup>();
                if (cg != null)
                {
                    return cg;
                }
            }
        }

        return null;
    }
}
