using UnityEngine;
using System.Collections;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// 還原 Portrait 到原始定位位置
    /// 用法: PortraitRestore([duration])
    /// - duration: 還原動畫持續時間（秒），預設為 0.5。如果設為 0 則立即還原
    /// 
    /// 範例:
    /// - PortraitRestore() - 使用預設 0.5 秒還原到原始位置
    /// - PortraitRestore(0.8) - 0.8 秒還原動畫
    /// - PortraitRestore(0) - 立即還原到原始位置（無動畫）
    /// </summary>
    public class SequencerCommandPortraitRestore : SequencerCommand
    {
        private Coroutine restoreCoroutine;
        private RectTransform portraitRectTransform;

        // 靜態變數用於保存原始位置（由其他 portrait 命令設置）
        private static Vector2? savedOriginalPosition = null;
        private static bool hasSavedPosition = false;

        public void Start()
        {
            // 獲取參數
            float duration = GetParameterAsFloat(0, 0.5f); // 預設 0.5 秒

            // 找到 Portrait Image
            portraitRectTransform = FindPortraitImage();
            if (portraitRectTransform == null)
            {
                Debug.LogWarning("PortraitRestore: 找不到 Portrait Image");
                Stop();
                return;
            }

            // 檢查是否有保存的原始位置
            if (!hasSavedPosition || !savedOriginalPosition.HasValue)
            {
                Debug.LogWarning("PortraitRestore: 沒有保存的原始位置，無法還原");
                Stop();
                return;
            }

            Vector2 targetPosition = savedOriginalPosition.Value;

            // 如果 duration 為 0，立即還原
            if (duration <= 0f)
            {
                portraitRectTransform.anchoredPosition = targetPosition;
                Stop();
                return;
            }

            // 開始還原動畫
            restoreCoroutine = StartCoroutine(RestoreCoroutine(targetPosition, duration));
        }

        private RectTransform FindPortraitImage()
        {
            // 方法 1: 使用 SequencerTools 的方法（最可靠）
            Transform portraitTransform = SequencerTools.GetPortraitImage(speaker);
            if (portraitTransform != null)
            {
                RectTransform rectTransform = portraitTransform.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    return rectTransform;
                }
            }

            // 方法 2: 直接搜尋 GameObject
            GameObject portraitObj = GameObject.Find("Portrait Image");
            if (portraitObj != null)
            {
                RectTransform rectTransform = portraitObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    return rectTransform;
                }
            }

            // 方法 3: 搜尋路徑（使用多種可能的路徑）
            GameObject dialogueManager = GameObject.Find("Dialogue Manager");
            if (dialogueManager != null)
            {
                // 嘗試常見的路徑
                string[] possiblePaths = new string[]
                {
                    "Canvas/Basic Standard Dialogue UI/Dialogue Panel/PC Subtitle Panel/Portrait Image",
                    "Canvas/Standard Dialogue UI/Dialogue Panel/PC Subtitle Panel/Portrait Image",
                    "Canvas/Dialogue Panel/PC Subtitle Panel/Portrait Image"
                };

                foreach (string path in possiblePaths)
                {
                    Transform portraitTransform2 = dialogueManager.transform.Find(path);
                    if (portraitTransform2 != null)
                    {
                        RectTransform rectTransform = portraitTransform2.GetComponent<RectTransform>();
                        if (rectTransform != null)
                        {
                            return rectTransform;
                        }
                    }
                }
            }

            // 方法 4: 在整個場景中搜尋所有包含 "Portrait" 和 "Image" 的物件
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Portrait") && obj.name.Contains("Image"))
                {
                    RectTransform rectTransform = obj.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        return rectTransform;
                    }
                }
            }

            return null;
        }

        private IEnumerator RestoreCoroutine(Vector2 targetPosition, float duration)
        {
            if (portraitRectTransform == null)
            {
                Stop();
                yield break;
            }

            // 記錄當前位置
            Vector2 startPosition = portraitRectTransform.anchoredPosition;

            // 執行動畫
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                
                // 使用平滑曲線讓動畫更自然
                t = Mathf.SmoothStep(0f, 1f, t);

                // 插值移動
                portraitRectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);

                yield return null;
            }

            // 確保到達目標位置
            portraitRectTransform.anchoredPosition = targetPosition;

            Stop();
        }

        public void OnDestroy()
        {
            // 如果命令被中斷，停止協程
            if (portraitRectTransform != null && restoreCoroutine != null)
            {
                StopCoroutine(restoreCoroutine);
            }
        }

        /// <summary>
        /// 靜態方法：保存原始位置（供其他 portrait 命令使用）
        /// </summary>
        public static void SaveOriginalPosition(Vector2 position)
        {
            savedOriginalPosition = position;
            hasSavedPosition = true;
        }

        /// <summary>
        /// 靜態方法：清除保存的位置
        /// </summary>
        public static void ClearSavedPosition()
        {
            savedOriginalPosition = null;
            hasSavedPosition = false;
        }
    }
}

