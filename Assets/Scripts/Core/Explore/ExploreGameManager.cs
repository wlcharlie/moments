using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;


public class ExploreGameManager : MonoBehaviour
{
    [SerializeField] private DiceController diceController;
    private readonly List<string> storyModeConversationIds = new() { "COMM_MRT_02", "COMM_MART_02" };
    private readonly List<string> eventModeConversationIds = new() { "COMM_MRT_02", "COMM_MART_02" };

    // 特定流程表
    // Key: 存檔中記錄的對話, Value: 小遊戲結束後要進入的對話
    private static readonly System.Collections.Generic.Dictionary<string, string> flowConversationMap = new()
    {
        { "CH01_SC04_SE02", "CH01_SC04_SE03" },
    };

    void Start()
    {
        if (diceController != null)
        {
            diceController.OnRollComplete += HandleRollComplete;
        }
    }

    void OnDestroy()
    {
        if (diceController != null)
        {
            diceController.OnRollComplete -= HandleRollComplete;
        }
    }

    private void HandleRollComplete(int result)
    {
        string conversationId;

        // 檢查 flowConversationMap 是否有對應的對話
        DialogueData dialogueData = PersistentDataManager.Instance?.LoadData<DialogueData>("dialogue");
        string mainStory = dialogueData?.GetValue("mainStory");

        if (!string.IsNullOrEmpty(mainStory) && flowConversationMap.TryGetValue(mainStory, out string mappedConversation))
        {
            // 使用 flowConversationMap 中對應的對話
            conversationId = mappedConversation;
            Debug.Log($"骰子結果: {result}, 使用 flowConversationMap: {mainStory} -> {conversationId}");
        }
        else
        {
            // 使用原本的隨機對話邏輯
            List<string> conversationIds = GameManager.Instance.CurrentMode == GameMode.Story
                ? storyModeConversationIds
                : eventModeConversationIds;

            int index = (result - 1) % conversationIds.Count;
            conversationId = conversationIds[index];
            Debug.Log($"骰子結果: {result}, 模式: {GameManager.Instance.CurrentMode}, 對話: {conversationId}");
        }

        TransitionManager.Instance.LoadSceneWithTransition("MainStoryScene", TransitionType.Cover, onLoadDone: () =>
        {
            // 場景載入完成後啟動對話
            DialogueManager.StopConversation(); // 停止當前對話
            DialogueManager.StartConversation(conversationId);
        });
    }
}
