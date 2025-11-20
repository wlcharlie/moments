using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueDebugManager : MonoBehaviour
{
    [System.Serializable]
    public class ChapterEntry
    {
        [Tooltip("按鈕上顯示的文字")]
        public string buttonName;
        [Tooltip("要切換的場景名稱（留空代表留在當前場景）")]
        public string sceneToLoad = "MainStoryScene";
        [Tooltip("Dialogue System 的對話 ID")]
        public string conversationStart = "CH01_SC02_SE01";
    }

    [SerializeField] private ChapterEntry[] chapters;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Transform buttonParent;

    private static string pendingConversation;
    private static string pendingScene;

    private void Awake()
    {
        if (buttonPrefab == null)
        {
            Debug.LogError("DialogueDebugManager: buttonPrefab 未指定。");
            enabled = false;
        }
    }

    private void Start()
    {
        if (buttonPrefab == null)
        {
            Debug.LogError("DialogueDebugManager: buttonPrefab 未指定。");
            return;
        }

        if (chapters == null || chapters.Length == 0) return;

        foreach (var entry in chapters)
        {
            if (entry == null) continue;

            Button buttonInstance = Instantiate(buttonPrefab, buttonParent);
            TMP_Text label = buttonInstance.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = entry.buttonName;
            }

            var capturedEntry = entry;
            buttonInstance.onClick.AddListener(() => StartChapter(capturedEntry));
        }
    }

    private void StartChapter(ChapterEntry entry)
    {
        if (entry == null)
        {
            Debug.LogWarning("DialogueDebugManager: ChapterEntry 為空。");
            return;
        }

        if (string.IsNullOrEmpty(entry.conversationStart))
        {
            Debug.LogWarning($"DialogueDebugManager: '{entry.buttonName}' 沒有指定 conversationStart。");
            return;
        }

        pendingConversation = entry.conversationStart;
        pendingScene = entry.sceneToLoad;

        if (string.IsNullOrEmpty(entry.sceneToLoad) || entry.sceneToLoad == SceneManager.GetActiveScene().name)
        {
            StartPendingConversation();
            return;
        }

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.LoadSceneWithTransition(
                entry.sceneToLoad,
                onLoadDone: () =>
                {
                    StartPendingConversation();
                });
        }
        else
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
            SceneManager.LoadScene(entry.sceneToLoad);
        }
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != pendingScene) return;

        SceneManager.sceneLoaded -= HandleSceneLoaded;
        StartPendingConversation();
    }

    private static void StartPendingConversation()
    {
        if (string.IsNullOrEmpty(pendingConversation))
        {
            pendingScene = null;
            return;
        }

        if (DialogueManager.instance == null)
        {
            Debug.LogWarning("DialogueDebugManager: DialogueManager instance 不存在,無法啟動對話。");
            return;
        }

        ResetDialogueState();
        DialogueManager.StartConversation(pendingConversation);

        pendingConversation = null;
        pendingScene = null;
    }

    private static void ResetDialogueState()
    {
        if (!Application.isPlaying || DialogueManager.instance == null)
        {
            return;
        }

        DialogueManager.StopAllConversations();
    }
}

