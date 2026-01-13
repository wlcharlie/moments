using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// 讓 Portrait 從原定位往右滑出並淡出
    /// 用法: PortraitSlideOut([duration], [offsetX])
    /// - duration: 動畫持續時間（秒），預設為 0.5
    /// - offsetX: 目標位置的 X 偏移量（相對於原始位置），預設為 500（向右移動）
    /// 
    /// 範例:
    /// - PortraitSlideOut() - 使用預設值（0.5秒，向右移動 500，同時淡出）
    /// - PortraitSlideOut(0.8) - 0.8秒動畫
    /// - PortraitSlideOut(0.8, 600) - 0.8秒動畫，向右移動 600
    /// </summary>
    public class SequencerCommandPortraitSlideOut : SequencerCommand
    {
        private Coroutine slideCoroutine;
        private RectTransform portraitRectTransform;
        private CanvasGroup portraitCanvasGroup;
        private Image portraitImage;
        private Vector2 originalAnchoredPosition;
        private float originalAlpha;
        private Vector2 targetAnchoredPosition;
        private bool hasTargetPosition;

        public void Start()
        {
            // 獲取參數
            float duration = GetParameterAsFloat(0, 0.5f); // 預設 0.5 秒
            float offsetX = GetParameterAsFloat(1, 500f); // 預設向右移動 500

            // 找到 Portrait Image
            portraitRectTransform = FindPortraitImage();
            if (portraitRectTransform == null)
            {
                Debug.LogWarning("PortraitSlideOut: 找不到 Portrait Image");
                Stop();
                return;
            }

            // 記錄原始位置
            originalAnchoredPosition = portraitRectTransform.anchoredPosition;
            
            // 保存原始位置到靜態變數，供 PortraitRestore 使用
            SequencerCommandPortraitRestore.SaveOriginalPosition(originalAnchoredPosition);

            // 獲取 CanvasGroup 或 Image 組件來控制透明度
            portraitCanvasGroup = portraitRectTransform.GetComponent<CanvasGroup>();
            if (portraitCanvasGroup == null)
            {
                portraitImage = portraitRectTransform.GetComponent<Image>();
                if (portraitImage != null)
                {
                    originalAlpha = portraitImage.color.a;
                }
                else
                {
                    originalAlpha = 1f;
                }
            }
            else
            {
                originalAlpha = portraitCanvasGroup.alpha;
            }

            // 保存原始透明度到靜態變數，供 PortraitRestore 使用
            SequencerCommandPortraitRestore.SaveOriginalAlpha(originalAlpha);

            // 預先計算目標位置（若被 Continue 中斷，OnDestroy 會直接 snap 到此位置避免停在半路）
            targetAnchoredPosition = new Vector2(
                originalAnchoredPosition.x + offsetX,
                originalAnchoredPosition.y
            );
            hasTargetPosition = true;

            // 開始滑出動畫
            slideCoroutine = StartCoroutine(SlideOutCoroutine(duration, offsetX));
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

        private IEnumerator SlideOutCoroutine(float duration, float offsetX)
        {
            if (portraitRectTransform == null)
            {
                Stop();
                yield break;
            }

            // 計算目標位置（原始位置 + 偏移量，向右移動）
            Vector2 startPosition = originalAnchoredPosition;
            Vector2 targetPosition = new Vector2(
                originalAnchoredPosition.x + offsetX,
                originalAnchoredPosition.y
            );

            // 執行動畫（同時移動和淡出）
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                
                // 使用平滑曲線讓動畫更自然
                t = Mathf.SmoothStep(0f, 1f, t);

                // 插值移動
                portraitRectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);

                // 插值透明度（從原始透明度淡出到 0）
                float alpha = Mathf.Lerp(originalAlpha, 0f, t);
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

            // 確保到達目標位置和完全透明
            portraitRectTransform.anchoredPosition = targetPosition;
            if (portraitCanvasGroup != null)
            {
                portraitCanvasGroup.alpha = 0f;
            }
            else if (portraitImage != null)
            {
                Color color = portraitImage.color;
                color.a = 0f;
                portraitImage.color = color;
            }

            Stop();
        }

        public void OnDestroy()
        {
            // 如果命令被中斷，停止協程並落到終態，避免停在半路（位置/透明度）
            if (portraitRectTransform != null && slideCoroutine != null)
            {
                StopCoroutine(slideCoroutine);
                if (hasTargetPosition)
                {
                    portraitRectTransform.anchoredPosition = targetAnchoredPosition;
                }

                if (portraitCanvasGroup != null)
                {
                    portraitCanvasGroup.alpha = 0f;
                }
                else if (portraitImage != null)
                {
                    Color color = portraitImage.color;
                    color.a = 0f;
                    portraitImage.color = color;
                }
            }
        }
    }
}

