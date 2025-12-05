using UnityEngine;
using System.Collections;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// 讓 Portrait 從指定起始位置移動到指定結束位置
    /// 用法: PortraitMove(startX, endX, [duration])
    /// - startX: 起始位置的 X 座標（相對於原始位置的偏移量）
    /// - endX: 結束位置的 X 座標（相對於原始位置的偏移量）
    /// - duration: 動畫持續時間（秒），預設為 0.5
    /// 
    /// 注意：起始點會使用 Unity 中設定的原始位置作為基準
    /// 
    /// 範例:
    /// - PortraitMove(300, 600) - 從原始位置 +300 移動到原始位置 +600，使用預設 0.5 秒
    /// - PortraitMove(300, 600, 0.8) - 從原始位置 +300 移動到原始位置 +600，0.8 秒動畫
    /// - PortraitMove(-500, 0) - 從原始位置 -500 移動到原始位置
    /// - PortraitMove(0, 500) - 從原始位置移動到原始位置 +500
    /// </summary>
    public class SequencerCommandPortraitMove : SequencerCommand
    {
        private Coroutine moveCoroutine;
        private RectTransform portraitRectTransform;
        private Vector2 originalAnchoredPosition;

        public void Start()
        {
            // 獲取參數
            float startX = GetParameterAsFloat(0, 0f); // 起始 X 座標偏移量（必需）
            float endX = GetParameterAsFloat(1, 0f); // 結束 X 座標偏移量（必需）
            float duration = GetParameterAsFloat(2, 0.5f); // 預設 0.5 秒

            // 找到 Portrait Image
            portraitRectTransform = FindPortraitImage();
            if (portraitRectTransform == null)
            {
                Debug.LogWarning("PortraitMove: 找不到 Portrait Image");
                Stop();
                return;
            }

            // 記錄原始位置（Unity 中設定的原始位置）
            originalAnchoredPosition = portraitRectTransform.anchoredPosition;
            
            // 保存原始位置到靜態變數，供 PortraitRestore 使用
            SequencerCommandPortraitRestore.SaveOriginalPosition(originalAnchoredPosition);

            // 開始移動動畫
            moveCoroutine = StartCoroutine(MoveCoroutine(startX, endX, duration));
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

        private IEnumerator MoveCoroutine(float startX, float endX, float duration)
        {
            if (portraitRectTransform == null)
            {
                Stop();
                yield break;
            }

            // 計算起始和結束位置（相對於原始位置的偏移量）
            // 原始位置是 Unity 中設定的初始位置
            Vector2 startPosition = new Vector2(
                originalAnchoredPosition.x + startX,
                originalAnchoredPosition.y
            );
            Vector2 endPosition = new Vector2(
                originalAnchoredPosition.x + endX,
                originalAnchoredPosition.y
            );

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
                portraitRectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);

                yield return null;
            }

            // 確保到達目標位置
            portraitRectTransform.anchoredPosition = endPosition;

            Stop();
        }

        public void OnDestroy()
        {
            // 如果命令被中斷，停止協程
            if (portraitRectTransform != null && moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
            }
        }
    }
}

