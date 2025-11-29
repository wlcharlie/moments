using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// Sequencer command to unload an additive scene.
    /// Usage: UnloadScene(sceneName[, useFadeOut])
    /// - sceneName: Name of the scene to unload (required)
    /// - useFadeOut: "true" or "false" (optional, defaults to "true")
    ///
    /// Examples:
    /// - UnloadScene(OverlayScene)
    /// - UnloadScene(OverlayScene, true)
    /// - UnloadScene(OverlayScene, false)
    /// </summary>
    public class SequencerCommandUnloadScene : SequencerCommand
    {
        private bool isWaitingForUnload = false;

        public void Awake()
        {
            // 獲取場景名稱參數
            string sceneName = GetParameter(0);

            // 獲取是否使用淡出效果參數 (可選，預設為 true)
            string useFadeOutString = GetParameter(1, "true");
            bool useFadeOut = !useFadeOutString.Equals("false", System.StringComparison.OrdinalIgnoreCase);

            // 驗證場景名稱
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("UnloadScene: Scene name is required.");
                Stop();
                return;
            }

            // 檢查場景是否已載入
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogWarning($"UnloadScene: Scene '{sceneName}' is not loaded.");
                Stop();
                return;
            }

            // 卸載場景
            if (TransitionManager.Instance != null)
            {
                isWaitingForUnload = true;
                TransitionManager.Instance.UnloadScene(sceneName, useFadeOut, OnUnloadComplete);
                Debug.Log($"UnloadScene: Unloading scene '{sceneName}' with fade out: {useFadeOut}");
            }
            else
            {
                // TransitionManager 不存在，直接卸載
                SceneManager.UnloadSceneAsync(sceneName);
                Debug.Log($"UnloadScene: Unloading scene '{sceneName}' without TransitionManager.");
                Stop();
            }
        }

        private void OnUnloadComplete()
        {
            isWaitingForUnload = false;
            Sequencer.Message("SceneUnloaded");
            Stop();
        }

        public void OnDestroy()
        {
            // 清理代碼（如果需要）
        }
    }
}
