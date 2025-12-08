using PixelCrushers.DialogueSystem;
using UnityEngine;

public class ExploreGameManager : MonoBehaviour
{
    [Header("骰子")]
    [SerializeField] private DiceController diceController;

    [Header("地圖")]
    [SerializeField] private ExploreMapController mapController;
    [SerializeField] private MapPlayer mapPlayer;

    [Header("模式設定")]
    [Tooltip("勾選後使用下方指定的模式，否則使用 GameManager 的模式")]
    [SerializeField] private bool useOverrideMode;
    [SerializeField] private GameMode overrideMode = GameMode.Story;

    /// <summary>
    /// 取得當前使用的遊戲模式（優先使用 GameManager，若設定 override 則使用指定模式）
    /// </summary>
    private GameMode CurrentGameMode =>
        useOverrideMode ? overrideMode : GameManager.Instance?.CurrentMode ?? overrideMode;

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

        // 根據當前模式生成地圖
        if (mapController == null)
        {
            Debug.LogError("[ExploreGameManager] mapController 未設定");
            return;
        }

        GameMode mode = CurrentGameMode;
        Debug.Log($"[ExploreGameManager] 開始生成地圖，模式: {mode} (useOverride: {useOverrideMode})");

        MapNode startNode = mapController.GenerateMap(mode);

        if (startNode == null)
        {
            Debug.LogError("[ExploreGameManager] GenerateMap 返回 null，地圖生成失敗");
            return;
        }

        if (mapPlayer == null)
        {
            Debug.LogError("[ExploreGameManager] mapPlayer 未設定");
            return;
        }

        mapPlayer.TeleportToNode(startNode);
        Debug.Log($"[ExploreGameManager] 玩家已傳送到起點: {startNode.NodeName}, 位置: {startNode.transform.position}");
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

        if (mapPlayer != null)
        {
            mapPlayer.MoveSteps(result);
        }
    }

    private void HandleMoveComplete(MapNode arrivedNode)
    {
        Debug.Log($"玩家到達節點: {arrivedNode.NodeName}");

        if (!string.IsNullOrEmpty(arrivedNode.ConversationTitle))
        {
            StartConversation(arrivedNode.ConversationTitle);
        }
    }

    private void StartConversation(string conversationTitle)
    {
        Debug.Log($"開始對話: {conversationTitle}");
        return;
        TransitionManager.Instance.LoadSceneWithTransition("MainStoryScene", TransitionType.Cover, onLoadDone: () =>
        {
            DialogueManager.StopConversation();
            DialogueManager.StartConversation(conversationTitle);
        });
    }
}
