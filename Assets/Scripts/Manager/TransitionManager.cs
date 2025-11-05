using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    [SerializeField] private RectTransform background;
    [SerializeField] private float transitionDuration = 0.5f;

    private bool isTransitioning = false;

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
        if (background != null)
        {
            // 初始位置：右側外面 (left: 786, right: 0)
            background.offsetMin = new Vector2(786, background.offsetMin.y);
            background.offsetMax = new Vector2(0, background.offsetMax.y);
        }
    }

    /// <summary>
    /// 載入場景並執行過渡動畫
    /// </summary>
    /// <param name="sceneName">要載入的場景名稱</param>
    public void LoadSceneWithTransition(string sceneName)
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionCoroutine(sceneName));
        }
    }

    /// <summary>
    /// 執行過渡動畫的協程
    /// </summary>
    private IEnumerator TransitionCoroutine(string sceneName)
    {
        isTransitioning = true;

        // 階段 1: 從右邊滑入至中間 (left: 786 -> 0, right: 0 -> 0)
        yield return StartCoroutine(SlideIn());

        // 非同步載入場景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // 等待場景載入完成
        while (!asyncLoad.isDone)
        {
            // 可選：在此處更新載入進度 UI
            // float progress = asyncLoad.progress;
            yield return null;
        }

        // 階段 2: 從中間滑出至左邊 (left: 0 -> 0, right: 0 -> 786)
        yield return StartCoroutine(SlideOut());

        isTransitioning = false;
    }

    /// <summary>
    /// 從右邊滑入 (left 從 786 遞減至 0)
    /// </summary>
    private IEnumerator SlideIn()
    {
        float elapsed = 0f;
        Vector2 startMin = new Vector2(786, background.offsetMin.y);
        Vector2 startMax = new Vector2(0, background.offsetMax.y);
        Vector2 endMin = new Vector2(0, background.offsetMin.y);
        Vector2 endMax = new Vector2(0, background.offsetMax.y);

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            background.offsetMin = Vector2.Lerp(startMin, endMin, t);
            background.offsetMax = Vector2.Lerp(startMax, endMax, t);

            yield return null;
        }

        background.offsetMin = endMin;
        background.offsetMax = endMax;
    }

    /// <summary>
    /// 從中間滑出至左邊 (right 從 0 遞增至 786)
    /// </summary>
    private IEnumerator SlideOut()
    {
        float elapsed = 0f;
        Vector2 startMin = new Vector2(0, background.offsetMin.y);
        Vector2 startMax = new Vector2(0, background.offsetMax.y);
        Vector2 endMin = new Vector2(0, background.offsetMin.y);
        Vector2 endMax = new Vector2(-786, background.offsetMax.y);

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            background.offsetMin = Vector2.Lerp(startMin, endMin, t);
            background.offsetMax = Vector2.Lerp(startMax, endMax, t);

            yield return null;
        }

        background.offsetMin = endMin;
        background.offsetMax = endMax;

        // 重置回初始位置 (left: 786, right: 0)，準備下次使用
        background.offsetMin = new Vector2(786, background.offsetMin.y);
        background.offsetMax = new Vector2(0, background.offsetMax.y);
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
