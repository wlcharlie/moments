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

    // StoryMode 特定流程表
    // Key: 存檔中記錄的對話, Value: 小遊戲結束後要進入的對話
    private static readonly System.Collections.Generic.Dictionary<string, string> flowConversationMap = new()
    {
        { "CH01_SC04_SE02", "CH01_SC04_SE03" },
    };

    // StoryMode 強制移動目標節點
    private MapNode forcedTargetNode;

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

        // StoryMode: 檢查是否有強制流程
        if (mode == GameMode.Story)
        {
            CheckStoryModeFlow();
        }
    }

    /// <summary>
    /// 檢查 StoryMode 流程，標記強制移動目標節點
    /// </summary>
    private void CheckStoryModeFlow()
    {
        Debug.Log("[ExploreGameManager] 檢查 StoryMode 強制流程");
        if (PersistentDataManager.Instance == null)
        {
            Debug.LogWarning("[ExploreGameManager] PersistentDataManager 未初始化");
            return;
        }

        // 讀取 dialogue.json
        DialogueData dialogueData = PersistentDataManager.Instance.LoadData<DialogueData>("dialogue");
        string mainStory = dialogueData?.GetValue("mainStory");

        if (string.IsNullOrEmpty(mainStory))
        {
            Debug.Log("[ExploreGameManager] 沒有 mainStory 記錄，使用一般流程");
            return;
        }

        Debug.Log($"[ExploreGameManager] 讀取到 mainStory: {mainStory}");

        // 檢查是否在流程表中
        if (flowConversationMap.TryGetValue(mainStory, out string targetConversation))
        {
            // 標記目標節點
            forcedTargetNode = mapController.MarkNodeByConversationTitle(targetConversation);

            if (forcedTargetNode != null)
            {
                int steps = mapPlayer.CalculateStepsToNode(forcedTargetNode);
                Debug.Log($"[ExploreGameManager] StoryMode 流程啟動: {mainStory} → {targetConversation}，距離 {steps} 步");
            }
        }
        else
        {
            Debug.Log($"[ExploreGameManager] mainStory '{mainStory}' 不在流程表中，使用一般流程");
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

    /// <summary>
    /// 觸發骰子（供外部 UI 呼叫）
    /// </summary>
    public void RollDice()
    {
        if (diceController == null) return;

        // 如果有強制目標節點，計算步數並擲出對應結果
        if (forcedTargetNode != null)
        {
            int steps = mapPlayer.CalculateStepsToNode(forcedTargetNode);
            if (steps > 0 && steps <= 6)
            {
                Debug.Log($"[ExploreGameManager] 強制擲骰: {steps} 步到達 {forcedTargetNode.NodeName}");
                diceController.RollWithResult(steps);
                return;
            }
            else
            {
                Debug.LogWarning($"[ExploreGameManager] 強制目標距離 {steps} 步，超出骰子範圍，使用一般擲骰");
            }
        }

        // 一般擲骰
        diceController.Roll();
    }

    private void HandleRollComplete(int result)
    {
        Debug.Log($"[ExploreGameManager] 骰子結果: {result}");

        if (mapPlayer != null)
        {
            mapPlayer.MoveSteps(result);
        }

        // 如果到達強制目標節點，清除標記
        if (forcedTargetNode != null && mapPlayer.CurrentNode == forcedTargetNode)
        {
            forcedTargetNode.SetMarked(false);
            forcedTargetNode = null;
            Debug.Log("[ExploreGameManager] 已到達強制目標節點，清除標記");
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

        TransitionManager.Instance.LoadSceneWithTransition("MainStoryScene", TransitionType.Cover, onLoadDone: () =>
        {
            DialogueManager.StopConversation();
            DialogueManager.StartConversation(conversationTitle);
        });
    }
}
