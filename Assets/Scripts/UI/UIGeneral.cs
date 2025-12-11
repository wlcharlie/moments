using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class UIGeneral : MonoBehaviour
{
    public static UIGeneral Instance { get; private set; }

    [Tooltip("在這些場景中會隱藏此 UI")]
    [SerializeField] private string[] hiddenScenes;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            canvasGroup = GetComponent<CanvasGroup>();
            CheckAndHide();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckAndHide();
    }

    private void CheckAndHide()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        foreach (string sceneName in hiddenScenes)
        {
            if (currentScene == sceneName)
            {
                SetVisible(false);
                return;
            }
        }

        SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    /// <summary>
    /// 停止對話並返回標題畫面
    /// </summary>
    public void OnHomeButtonClicked()
    {
        DialogueManager.StopAllConversations();

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.LoadSceneWithTransition("TitleScene", TransitionType.Cover);
        }
        else
        {
            SceneManager.LoadScene("TitleScene");
        }
    }
}
