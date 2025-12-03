using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueDebugManager : MonoBehaviour
{
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Transform buttonParent;
    [SerializeField] private string defaultSceneToLoad = "MainStoryScene";
    [Tooltip("篩選對話標題的前綴（留空則顯示所有對話）")]
    [SerializeField] private string conversationFilter = "";

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

        if (DialogueManager.masterDatabase == null)
        {
            Debug.LogError("DialogueDebugManager: DialogueManager.masterDatabase 不存在。");
            return;
        }

        var conversations = DialogueManager.masterDatabase.conversations;
        if (conversations == null || conversations.Count == 0)
        {
            Debug.LogWarning("DialogueDebugManager: 沒有找到任何對話。");
            return;
        }

        foreach (var conversation in conversations)
        {
            if (conversation == null) continue;

            string title = conversation.Title;
            if (string.IsNullOrEmpty(title)) continue;

            // 過濾對話（如果有設定篩選條件）
            if (!string.IsNullOrEmpty(conversationFilter) && !title.StartsWith(conversationFilter))
                continue;

            Button buttonInstance = Instantiate(buttonPrefab, buttonParent);
            TMP_Text label = buttonInstance.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = title;
            }

            var capturedTitle = title;
            buttonInstance.onClick.AddListener(() => StartConversation(capturedTitle));
        }
    }

    private void StartConversation(string conversationTitle)
    {
        if (string.IsNullOrEmpty(conversationTitle))
        {
            Debug.LogWarning("DialogueDebugManager: conversationTitle 為空。");
            return;
        }

        pendingConversation = conversationTitle;
        pendingScene = defaultSceneToLoad;

        if (string.IsNullOrEmpty(defaultSceneToLoad) || defaultSceneToLoad == SceneManager.GetActiveScene().name)
        {
            StartPendingConversation();
            return;
        }

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.LoadSceneWithTransition(
                defaultSceneToLoad,
                onLoadDone: () =>
                {
                    StartPendingConversation();
                });
        }
        else
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
            SceneManager.LoadScene(defaultSceneToLoad);
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

