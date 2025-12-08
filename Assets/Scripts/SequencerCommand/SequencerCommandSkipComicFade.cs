using UnityEngine;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// Sequencer command to skip fade effect for Comic Image.
    /// Usage: SkipComicFade(position[, fadeType])
    /// - position: "left", "right", "center", or GameObject name (required)
    /// - fadeType: "in" (skip fade in), "out" (skip fade out), or "both" (skip both, default)
    ///
    /// Examples:
    /// - SkipComicFade(left)  // 跳過下一次淡入和淡出
    /// - SkipComicFade(left, in)  // 只跳過淡入
    /// - SkipComicFade(left, out)  // 只跳過淡出
    /// - SkipComicFade(right, both)  // 跳過淡入和淡出
    /// </summary>
    public class SequencerCommandSkipComicFade : SequencerCommand
    {
        public void Awake()
        {
            // 獲取位置參數
            string position = GetParameter(0);
            
            // 獲取淡入淡出類型參數（可選，預設為 "both"）
            string fadeType = GetParameter(1, "both").ToLower();

            // 驗證位置參數
            if (string.IsNullOrEmpty(position))
            {
                Debug.LogError("SkipComicFade: Position parameter is required.");
                Stop();
                return;
            }

            // 將位置轉換為 GameObject 名稱
            string gameObjectName = GetGameObjectName(position);

            // 查找 GameObject
            GameObject comicObject = GameObject.Find(gameObjectName);
            if (comicObject == null)
            {
                Debug.LogWarning($"SkipComicFade: 找不到 {gameObjectName} 物件");
                Stop();
                return;
            }

            // 獲取 ComicImageFader 組件
            ComicImageFader fader = comicObject.GetComponent<ComicImageFader>();
            if (fader == null)
            {
                Debug.LogWarning($"SkipComicFade: {gameObjectName} 物件沒有 ComicImageFader 組件");
                Stop();
                return;
            }

            // 根據類型設置跳過淡入淡出
            switch (fadeType)
            {
                case "in":
                    // 只跳過淡入：設置 skipNextFade 標記
                    fader.SkipNextFade();
                    Debug.Log($"SkipComicFade: {gameObjectName} 將跳過下一次淡入效果");
                    break;
                case "out":
                    // 只跳過淡出：設置 skipNextFade 標記（淡出也會被跳過）
                    fader.SkipNextFade();
                    Debug.Log($"SkipComicFade: {gameObjectName} 將跳過下一次淡出效果");
                    break;
                case "both":
                default:
                    // 跳過淡入和淡出：設置 skipNextFade 標記
                    fader.SkipNextFade();
                    Debug.Log($"SkipComicFade: {gameObjectName} 將跳過下一次淡入和淡出效果");
                    break;
            }

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

