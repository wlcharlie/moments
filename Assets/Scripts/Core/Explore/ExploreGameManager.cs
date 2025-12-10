using PixelCrushers.DialogueSystem;
using UnityEngine;

public class ExploreGameManager : MonoBehaviour
{
    private const string SAVE_KEY = "exploreMap";

    [Header("骰子")]
    [SerializeField] private DiceController diceController;

    [Header("地圖")]
    [SerializeField] private ExploreMapController mapController;
    [SerializeField] private MapPlayer mapPlayer;
    [SerializeField] private MapCameraFollow mapCameraFollow;

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
            mapPlayer.OnLoopBack += HandleLoopBack;
        }

        // 根據當前模式生成地圖
        if (mapController == null)
        {
            Debug.LogError("[ExploreGameManager] mapController 未設定");
            return;
        }

        if (mapPlayer == null)
        {
            Debug.LogError("[ExploreGameManager] mapPlayer 未設定");
            return;
        }

        GameMode mode = CurrentGameMode;
        MapNode playerStartNode;

        // 嘗試載入存檔
        ExploreMapSaveData saveData = LoadMapState();
        if (saveData != null && saveData.isValid && saveData.mode == mode)
        {
            // 使用存檔的 seed 生成相同的地圖
            Debug.Log($"[ExploreGameManager] 載入存檔，seed: {saveData.seed}, 玩家位置: {saveData.playerNodeIndex}");
            mapController.GenerateMap(mode, saveData.seed);
            playerStartNode = mapController.GetNodeByIndex(saveData.playerNodeIndex);

            if (playerStartNode == null)
            {
                Debug.LogWarning("[ExploreGameManager] 存檔的節點索引無效，重置到起點");
                playerStartNode = mapController.StartNode;
            }
        }
        else
        {
            // 新遊戲
            Debug.Log($"[ExploreGameManager] 開始新地圖，模式: {mode} (useOverride: {useOverrideMode})");
            playerStartNode = mapController.GenerateMap(mode);
        }

        if (playerStartNode == null)
        {
            Debug.LogError("[ExploreGameManager] GenerateMap 返回 null，地圖生成失敗");
            return;
        }

        mapPlayer.TeleportToNode(playerStartNode);
        Debug.Log($"[ExploreGameManager] 玩家已傳送到: {playerStartNode.NodeName}, 位置: {playerStartNode.transform.position}");

        // 設定終點循環回的節點 (第一個事件節點)
        MapNode loopBackNode = mapController.FirstEventNode;
        if (loopBackNode != null)
        {
            mapPlayer.SetLoopBackNode(loopBackNode);
            Debug.Log($"[ExploreGameManager] 設定循環節點: {loopBackNode.NodeName}");
        }

        // 立即保存當前狀態
        SaveMapState();

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
            mapPlayer.OnLoopBack -= HandleLoopBack;
        }
    }

    /// <summary>
    /// 保存地圖狀態
    /// </summary>
    private void SaveMapState()
    {
        if (PersistentDataManager.Instance == null || mapController == null || mapPlayer == null)
        {
            return;
        }

        ExploreMapSaveData saveData = new()
        {
            mode = CurrentGameMode,
            seed = mapController.CurrentSeed,
            playerNodeIndex = mapController.GetNodeIndex(mapPlayer.CurrentNode),
            isValid = true
        };

        PersistentDataManager.Instance.SaveData(saveData, SAVE_KEY);
        Debug.Log($"[ExploreGameManager] 已保存地圖狀態: seed={saveData.seed}, playerNodeIndex={saveData.playerNodeIndex}");
    }

    /// <summary>
    /// 載入地圖狀態
    /// </summary>
    private ExploreMapSaveData LoadMapState()
    {
        if (PersistentDataManager.Instance == null)
        {
            return null;
        }

        if (!PersistentDataManager.Instance.HasSaveData(SAVE_KEY))
        {
            return null;
        }

        return PersistentDataManager.Instance.LoadData<ExploreMapSaveData>(SAVE_KEY);
    }

    /// <summary>
    /// 清除地圖存檔 (開始新遊戲時呼叫)
    /// </summary>
    public void ClearMapSave()
    {
        if (PersistentDataManager.Instance != null)
        {
            PersistentDataManager.Instance.DeleteSaveData(SAVE_KEY);
            Debug.Log("[ExploreGameManager] 已清除地圖存檔");
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

        // 玩家開始移動時，重置拖曳狀態讓自動跟隨生效
        if (mapCameraFollow != null)
        {
            mapCameraFollow.ResetDragState();
        }

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

        // 保存當前狀態
        SaveMapState();

        if (!string.IsNullOrEmpty(arrivedNode.ConversationTitle))
        {
            StartConversation(arrivedNode.ConversationTitle);
        }
    }

    private void HandleLoopBack()
    {
        Debug.Log("[ExploreGameManager] 玩家循環回起點，重置地圖視角");

        if (mapCameraFollow != null)
        {
            mapCameraFollow.SnapToCenter();
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
