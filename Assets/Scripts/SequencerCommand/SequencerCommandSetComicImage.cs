using UnityEngine;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// Sequencer command to set Comic Image without fade-in effect.
    /// Usage: SetComicImage(position, imagePath[, skipFade])
    /// - position: "left", "right", "center", or GameObject name (required)
    /// - imagePath: Addressables path to the image (required, empty string to clear)
    /// - skipFade: "true" or "false" (optional, defaults to "true" for this command)
    ///
    /// Examples:
    /// - SetComicImage(left, Assets/Arts/Comics/panel_left.png)
    /// - SetComicImage(right, Assets/Arts/Comics/panel_right.png)
    /// - SetComicImage(center, Assets/Arts/Comics/panel_center.png)
    /// - SetComicImage(ComicImageLeft, Assets/Arts/Comics/panel_left.png, true)
    /// - SetComicImage(left, , false)  // Clear with fade out
    /// </summary>
    public class SequencerCommandSetComicImage : SequencerCommand
    {
        public void Awake()
        {
            // 獲取位置參數
            string position = GetParameter(0);
            
            // 獲取圖片路徑參數
            string imagePath = GetParameter(1);
            
            // 獲取是否跳過淡入參數（可選，預設為 true，因為這個命令的主要用途就是跳過淡入）
            string skipFadeString = GetParameter(2, "true");
            bool skipFade = !skipFadeString.Equals("false", System.StringComparison.OrdinalIgnoreCase);

            // 驗證位置參數
            if (string.IsNullOrEmpty(position))
            {
                Debug.LogError("SetComicImage: Position parameter is required.");
                Stop();
                return;
            }

            // 將位置轉換為 GameObject 名稱
            string gameObjectName = GetGameObjectName(position);

            // 使用 DialogueEventManager 設置 Comic Image
            DialogueEventManager.SetComicImage(gameObjectName, imagePath, skipFade);

            Debug.Log($"SetComicImage: 設置 {gameObjectName}，路徑={imagePath}，跳過淡入={skipFade}");

            Stop();
        }

        /// <summary>
        /// 將位置參數轉換為 GameObject 名稱
        /// </summary>
        private string GetGameObjectName(string position)
        {
            string lowerPosition = position.ToLower();
            
            switch (lowerPosition)
            {
                case "left":
                    return "ComicImageLeft";
                case "right":
                    return "ComicImageRight";
                case "center":
                case "centre":
                    return "ComicImage";
                default:
                    // 如果不是預定義的位置，假設是直接提供的 GameObject 名稱
                    return position;
            }
        }

        public void Update()
        {
            // 此命令在 Awake() 中立即完成，不需要 Update()
        }

        public void OnDestroy()
        {
            // 不需要清理
        }
    }
}

