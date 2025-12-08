using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;


public class ExploreGameManager : MonoBehaviour
{
    [Header("骰子")]
    [SerializeField] private DiceController diceController;

    [Header("地圖移動")]
    [SerializeField] private MapPlayer mapPlayer;

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

        if (mapPlayer != null)
        {
            mapPlayer.OnMoveComplete += HandleMoveComplete;
        }
    }

    void OnDestroy()
    {
        if (diceController != null)
        {
            diceController.OnRollComplete -= HandleRollComplete;
        }

        if (mapPlayer != null)
        {
            mapPlayer.OnMoveComplete -= HandleMoveComplete;
        }
    }

    private void HandleRollComplete(int result)
    {
        Debug.Log($"骰子結果: {result}");

        // 如果有 MapPlayer，先移動玩家
        if (mapPlayer != null)
        {
            mapPlayer.MoveSteps(result);
            return; // 等待移動完成後再處理對話
        }

        // 沒有 MapPlayer 時，直接使用原本邏輯
        StartConversationFromResult(result);
    }

    private void HandleMoveComplete(MapNode arrivedNode)
    {
        Debug.Log($"玩家到達節點: {arrivedNode.NodeName}");

        // 優先使用節點上設定的對話
        if (!string.IsNullOrEmpty(arrivedNode.ConversationId))
        {
            StartConversation(arrivedNode.ConversationId);
            return;
        }

        // 沒有設定對話時，使用原本的邏輯
        StartConversationFromResult(1);
    }

    private void StartConversationFromResult(int result)
    {

        return; // Temporary disable conversation starting
        string conversationId;

        // 檢查 flowConversationMap 是否有對應的對話
        DialogueData dialogueData = PersistentDataManager.Instance?.LoadData<DialogueData>("dialogue");
        string mainStory = dialogueData?.GetValue("mainStory");

        if (!string.IsNullOrEmpty(mainStory) && flowConversationMap.TryGetValue(mainStory, out string mappedConversation))
        {
            // 使用 flowConversationMap 中對應的對話
            conversationId = mappedConversation;
            Debug.Log($"使用 flowConversationMap: {mainStory} -> {conversationId}");
        }
        else
        {
            // 使用原本的隨機對話邏輯
            List<string> conversationIds = GameManager.Instance.CurrentMode == GameMode.Story
                ? storyModeConversationIds
                : eventModeConversationIds;

            int index = (result - 1) % conversationIds.Count;
            conversationId = conversationIds[index];
            Debug.Log($"模式: {GameManager.Instance.CurrentMode}, 對話: {conversationId}");
        }

        StartConversation(conversationId);
    }

    private void StartConversation(string conversationId)
    {
        TransitionManager.Instance.LoadSceneWithTransition("MainStoryScene", TransitionType.Cover, onLoadDone: () =>
        {
            DialogueManager.StopConversation();
            DialogueManager.StartConversation(conversationId);
        });
    }
}
