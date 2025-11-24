using UnityEngine;
using UnityEngine.SceneManagement;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// Sequencer command to switch scenes.
    /// Usage: SwitchScene(sceneName[, loadSceneMode][, transitionType])
    /// - sceneName: Name of the scene to load (required)
    /// - loadSceneMode: "Single" or "Additive" (optional, defaults to "Single")
    /// - transitionType: "Cover", "Splash", "LoadingScreen", or "None" (optional, defaults to "Cover")
    ///   - "None": 不使用轉場效果，直接載入場景
    ///
    /// Examples:
    /// - SwitchScene(MainStoryScene)
    /// - SwitchScene(TitleScene, Single)
    /// - SwitchScene(Playground, Additive)
    /// - SwitchScene(CH01_SC03_BeigoSneakDoor, Single, LoadingScreen)
    /// - SwitchScene(CH01_SC03_BeigoSneakDoor, Single, None)
    /// - SwitchScene(CH01_SC03_BeigoSneakDoor, Single, Cover)
    /// </summary>
    public class SequencerCommandSwitchScene : SequencerCommand
    {
        public void Awake()
        {
            // 獲取場景名稱參數
            string sceneName = GetParameter(0);

            // 獲取載入模式參數 (可選，預設為 Single)
            string loadModeString = GetParameter(1, "Single");
            LoadSceneMode loadMode = LoadSceneMode.Single;

            // 解析載入模式
            if (loadModeString.Equals("Additive", System.StringComparison.OrdinalIgnoreCase))
            {
                loadMode = LoadSceneMode.Additive;
            }

            // 獲取轉場類型參數 (可選，預設為 Cover)
            string transitionTypeString = GetParameter(2, "Cover");
            TransitionType? transitionType = null;
            bool useTransition = true;

            // 解析轉場類型
            if (transitionTypeString.Equals("None", System.StringComparison.OrdinalIgnoreCase))
            {
                useTransition = false;
            }
            else
            {
                // 嘗試解析轉場類型
                if (System.Enum.TryParse<TransitionType>(transitionTypeString, true, out TransitionType parsedType))
                {
                    transitionType = parsedType;
                }
                else
                {
                    // 如果無法解析，使用預設值 Cover
                    Debug.LogWarning($"SwitchScene: 無法識別轉場類型 '{transitionTypeString}'，使用預設值 'Cover'");
                    transitionType = TransitionType.Cover;
                }
            }

            // 驗證場景名稱
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("SwitchScene: Scene name is required.");
                Stop();
                return;
            }

            // 載入場景
            try
            {
                // 如果使用轉場且是 Single 模式，使用 TransitionManager
                if (useTransition && TransitionManager.Instance != null && loadMode == LoadSceneMode.Single && transitionType.HasValue)
                {
                    TransitionManager.Instance.LoadSceneWithTransition(sceneName, transitionType.Value);
                    Debug.Log($"SwitchScene: Loading scene '{sceneName}' with TransitionManager, transition type: {transitionType.Value}");
                }
                else
                {
                    // 不使用轉場、Additive 模式或 TransitionManager 不存在，直接載入場景
                    SceneManager.LoadScene(sceneName, loadMode);
                    Debug.Log($"SwitchScene: Loading scene '{sceneName}' with mode '{loadMode}' (no transition).");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SwitchScene: Failed to load scene '{sceneName}'. Error: {e.Message}");
            }

            // 立即完成命令
            Stop();
        }

        public void OnDestroy()
        {
            // 清理代碼（如果需要）
            // 場景切換是一次性操作，通常不需要特別清理
        }
    }
}
