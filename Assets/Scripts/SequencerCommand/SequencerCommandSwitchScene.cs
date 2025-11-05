using UnityEngine;
using UnityEngine.SceneManagement;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// Sequencer command to switch scenes.
    /// Usage: SwitchScene(sceneName[, loadSceneMode])
    /// - sceneName: Name of the scene to load (required)
    /// - loadSceneMode: "Single" or "Additive" (optional, defaults to "Single")
    ///
    /// Examples:
    /// - SwitchScene(MainStoryScene)
    /// - SwitchScene(TitleScene, Single)
    /// - SwitchScene(Playground, Additive)
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
                // 優先使用 TransitionManager 進行場景切換
                if (TransitionManager.Instance != null && loadMode == LoadSceneMode.Single)
                {
                    TransitionManager.Instance.LoadSceneWithTransition(sceneName);
                    Debug.Log($"SwitchScene: Loading scene '{sceneName}' with TransitionManager.");
                }
                else
                {
                    // 如果是 Additive 模式或 TransitionManager 不存在，使用原本的方式
                    SceneManager.LoadScene(sceneName, loadMode);
                    Debug.Log($"SwitchScene: Loading scene '{sceneName}' with mode '{loadMode}'.");
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
