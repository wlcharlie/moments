using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// 讓 Portrait 從左往右滑入定位
    /// 用法: PortraitSlideIn([duration], [offsetX])
    /// - duration: 動畫持續時間（秒），預設為 0.5
    /// - offsetX: 起始位置的 X 偏移量（相對於目標位置），預設為 -500（在左側外面）
    /// 
    /// 範例:
    /// - PortraitSlideIn() - 使用預設值（0.5秒，-500偏移）
    /// - PortraitSlideIn(0.8) - 0.8秒動畫
    /// - PortraitSlideIn(0.8, -600) - 0.8秒動畫，從更左側開始
    /// </summary>
    public class SequencerCommandPortraitSlideIn : SequencerCommand
    {
        private Coroutine slideCoroutine;
        private RectTransform portraitRectTransform;
        private CanvasGroup portraitCanvasGroup;
        private Image portraitImage;

        private Vector2 baseAnchoredPosition;
        private Vector2 targetAnchoredPosition;
        private bool hasTargetPosition;

        private float targetAlpha = 1f;
        private bool hasTargetAlpha;

        public void Start()
        {
            // 獲取參數
            float duration = GetParameterAsFloat(0, 0.5f); // 預設 0.5 秒
            float offsetX = GetParameterAsFloat(1, -500f); // 預設從左側 -500 開始

            // 找到 Portrait Image
            portraitRectTransform = FindPortraitImage();
            if (portraitRectTransform == null)
            {
                Debug.LogWarning("PortraitSlideIn: 找不到 Portrait Image");
                Stop();
                return;
            }

            // 取得可調整透明度的組件
            portraitCanvasGroup = portraitRectTransform.GetComponent<CanvasGroup>();
            if (portraitCanvasGroup == null)
            {
                portraitImage = portraitRectTransform.GetComponent<Image>();
            }

            // 取得基準原始位置：優先使用已保存的原始定位（避免 SlideOut/Move 後，SlideIn 把「當前位置」當目標）
            if (!SequencerCommandPortraitRestore.TryGetSavedOriginalPosition(out baseAnchoredPosition))
            {
                baseAnchoredPosition = portraitRectTransform.anchoredPosition;
                SequencerCommandPortraitRestore.SaveOriginalPosition(baseAnchoredPosition);
            }

            // 取得目標透明度：優先使用已保存的原始透明度（避免 SlideOut 淡到 0 後 SlideIn 還是透明）
            if (SequencerCommandPortraitRestore.TryGetSavedOriginalAlpha(out targetAlpha))
            {
                hasTargetAlpha = true;
            }
            else
            {
                hasTargetAlpha = true;
                targetAlpha = 1f;
            }

            // 目標位置就是基準位置（若被 Continue 中斷，OnDestroy 會直接 snap 到此位置避免停在半路）
            targetAnchoredPosition = baseAnchoredPosition;
            hasTargetPosition = true;

            // 確保一開始就恢復到可見（至少不會維持在 SlideOut 的 0 alpha）
            if (hasTargetAlpha)
            {
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

            // 開始滑入動畫
            slideCoroutine = StartCoroutine(SlideInCoroutine(duration, offsetX));
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

        private IEnumerator SlideInCoroutine(float duration, float offsetX)
        {
            if (portraitRectTransform == null)
            {
                Stop();
                yield break;
            }

            // 計算起始位置（目標位置 + 偏移量）
            Vector2 startPosition = new Vector2(
                baseAnchoredPosition.x + offsetX,
                baseAnchoredPosition.y
            );
            Vector2 targetPosition = baseAnchoredPosition;

            // 設置起始位置
            portraitRectTransform.anchoredPosition = startPosition;

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
            // 如果命令被中斷，停止協程並落到終態，避免停在半路（位置/透明度）
            if (portraitRectTransform == null) return;

            if (slideCoroutine != null)
            {
                StopCoroutine(slideCoroutine);
            }

            if (hasTargetPosition)
            {
                portraitRectTransform.anchoredPosition = targetAnchoredPosition;
            }

            if (hasTargetAlpha)
            {
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
    }
}

