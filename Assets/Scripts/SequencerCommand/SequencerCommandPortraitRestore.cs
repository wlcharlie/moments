using UnityEngine;
using UnityEngine.UI;
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
        private CanvasGroup portraitCanvasGroup;
        private Image portraitImage;
        private Vector2 targetAnchoredPosition;
        private float targetAlpha;
        private bool hasTargets;

        // 靜態變數用於保存原始位置和透明度（由其他 portrait 命令設置）
        private static Vector2? savedOriginalPosition = null;
        private static float? savedOriginalAlpha = null;
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

            // 獲取 CanvasGroup 或 Image 組件來控制透明度
            portraitCanvasGroup = portraitRectTransform.GetComponent<CanvasGroup>();
            if (portraitCanvasGroup == null)
            {
                portraitImage = portraitRectTransform.GetComponent<Image>();
            }

            // 計算目標位置：優先使用保存的原始位置（避免 0 變成「當前位置」）
            Vector2 targetPosition;
            if (hasSavedPosition && savedOriginalPosition.HasValue)
            {
                // 直接還原到保存的原始位置（含 X/Y）
                targetPosition = savedOriginalPosition.Value;
            }
            else
            {
                // 如果沒有保存的位置，維持舊行為：X=200，Y 使用當前值
                targetPosition = new Vector2(200f, portraitRectTransform.anchoredPosition.y);
            }

            // 獲取目標透明度（如果有保存的原始透明度，否則設為 1）
            targetAlpha = savedOriginalAlpha.HasValue ? savedOriginalAlpha.Value : 1f;

            // 保存目標（若被 Continue 中斷，OnDestroy 會直接 snap 回去避免停在半路）
            targetAnchoredPosition = targetPosition;
            hasTargets = true;

            // 如果 duration 為 0，立即還原
            if (duration <= 0f)
            {
                portraitRectTransform.anchoredPosition = targetPosition;
                if (portraitCanvasGroup != null)
                {
                    portraitCanvasGroup.alpha = targetAlpha;
                }
                else if (portraitImage != null)
                {
                    Color color = portraitImage.color;
                    color.a = targetAlpha;
                    portraitImage.color = color;
                }
                Stop();
                return;
            }

            // 開始還原動畫
            restoreCoroutine = StartCoroutine(RestoreCoroutine(targetPosition, targetAlpha, duration));
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

        private IEnumerator RestoreCoroutine(Vector2 targetPosition, float targetAlpha, float duration)
        {
            if (portraitRectTransform == null)
            {
                Stop();
                yield break;
            }

            // 記錄當前位置和透明度
            Vector2 startPosition = portraitRectTransform.anchoredPosition;
            float startAlpha = 1f;
            if (portraitCanvasGroup != null)
            {
                startAlpha = portraitCanvasGroup.alpha;
            }
            else if (portraitImage != null)
            {
                startAlpha = portraitImage.color.a;
            }

            // 執行動畫（同時移動和恢復透明度）
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                
                // 使用平滑曲線讓動畫更自然
                t = Mathf.SmoothStep(0f, 1f, t);

                // 插值移動
                portraitRectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);

                // 插值透明度
                float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                if (portraitCanvasGroup != null)
                {
                    portraitCanvasGroup.alpha = alpha;
                }
                else if (portraitImage != null)
                {
                    Color color = portraitImage.color;
                    color.a = alpha;
                    portraitImage.color = color;
                }

                yield return null;
            }

            // 確保到達目標位置和透明度
            portraitRectTransform.anchoredPosition = targetPosition;
            if (portraitCanvasGroup != null)
            {
                portraitCanvasGroup.alpha = targetAlpha;
            }
            else if (portraitImage != null)
            {
                Color color = portraitImage.color;
                color.a = targetAlpha;
                portraitImage.color = color;
            }

            Stop();
        }

        public void OnDestroy()
        {
            // 如果命令被中斷，停止協程並落到終態，避免停在半路（位置/透明度）
            if (portraitRectTransform == null) return;

            if (restoreCoroutine != null)
            {
                StopCoroutine(restoreCoroutine);
            }

            if (hasTargets)
            {
                portraitRectTransform.anchoredPosition = targetAnchoredPosition;
                if (portraitCanvasGroup != null)
                {
                    portraitCanvasGroup.alpha = targetAlpha;
                }
                else if (portraitImage != null)
                {
                    Color color = portraitImage.color;
                    color.a = targetAlpha;
                    portraitImage.color = color;
                }
            }
        }

        /// <summary>
        /// 靜態方法：保存原始位置和透明度（供其他 portrait 命令使用）
        /// </summary>
        public static void SaveOriginalPosition(Vector2 position)
        {
            // 只在第一次保存，避免後續命令把「當前位置」覆蓋掉原始定位
            if (!hasSavedPosition)
            {
                savedOriginalPosition = position;
                hasSavedPosition = true;
            }
        }

        /// <summary>
        /// 靜態方法：保存原始透明度
        /// </summary>
        public static void SaveOriginalAlpha(float alpha)
        {
            // 只在第一次保存，避免後續命令覆蓋掉原始透明度
            if (!savedOriginalAlpha.HasValue)
            {
                savedOriginalAlpha = alpha;
            }
        }

        /// <summary>
        /// 靜態方法：嘗試取得保存的原始位置（如果存在）
        /// </summary>
        public static bool TryGetSavedOriginalPosition(out Vector2 position)
        {
            if (hasSavedPosition && savedOriginalPosition.HasValue)
            {
                position = savedOriginalPosition.Value;
                return true;
            }

            position = default;
            return false;
        }

        /// <summary>
        /// 靜態方法：清除保存的位置和透明度
        /// </summary>
        public static void ClearSavedPosition()
        {
            savedOriginalPosition = null;
            savedOriginalAlpha = null;
            hasSavedPosition = false;
        }
    }
}

