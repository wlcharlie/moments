using UnityEngine;
using System.Collections;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// 讓現有背景圖放大並移動到指定位置
    /// 用法: ShowSceneDetail(zoomScale, duration, reset, [position])
    /// 位置選項: "bottom-left" (左下角，預設), "left-center-up" (左邊中間偏上)
    /// 範例: ShowSceneDetail(2.5, 0.8, false) - 放大 2.5 倍，動畫 0.8 秒，不自動恢復，左下角
    /// 範例: ShowSceneDetail(2.5, 0.8, false, left-center-up) - 放大 2.5 倍，左邊中間偏上
    /// 範例: ShowSceneDetail(2.5, 0.8, true) - 放大 2.5 倍，動畫 0.8 秒，自動恢復原狀
    /// </summary>
    public class SequencerCommandShowSceneDetail : SequencerCommand
    {
        private GameObject backgroundObject;
        private SpriteRenderer backgroundRenderer;
        private Transform backgroundTransform;
        private Coroutine zoomCoroutine;

        // 原始狀態（用於恢復）- 使用靜態變數來追蹤
        private static Vector3 originalPosition;
        private static Vector3 originalScale;
        private static bool isZoomed = false;

        public void Awake()
        {
            // 參數: zoomScale, duration, reset, [position]
            float zoomScale = GetParameterAsFloat(0, 2.5f); // 預設放大 2.5 倍
            float duration = GetParameterAsFloat(1, 0.8f); // 預設動畫時間 0.8 秒
            bool reset = GetParameterAsBool(2, false); // 預設不自動恢復
            string position = GetParameter(3, "bottom-left").ToLower(); // 位置選項，預設左下角

            // 找到背景物件
            backgroundObject = GameObject.FindGameObjectWithTag("Background");
            if (backgroundObject == null)
            {
                Debug.LogWarning("ShowSceneDetail: Background object with tag 'Background' not found.");
                Stop();
                return;
            }

            backgroundRenderer = backgroundObject.GetComponent<SpriteRenderer>();
            backgroundTransform = backgroundObject.transform;

            if (backgroundRenderer == null || backgroundRenderer.sprite == null)
            {
                Debug.LogWarning("ShowSceneDetail: Background object has no SpriteRenderer or sprite.");
                Stop();
                return;
            }

            // 如果已經放大過，先恢復原狀
            if (isZoomed)
            {
                RestoreOriginalState();
            }

            // 儲存原始狀態
            if (!isZoomed)
            {
                originalPosition = backgroundTransform.position;
                originalScale = backgroundTransform.localScale;
                // 同時保存到 RestoreSceneDetail 以便共享
                Debug.Log($"ShowSceneDetail: 保存原始狀態。 {originalPosition}, {originalScale}");
                SequencerCommandRestoreSceneDetail.SaveOriginalState(originalPosition, originalScale);
            }

            // 開始放大動畫
            StartCoroutine(ZoomToDetail(zoomScale, duration, reset, position));
        }

        private IEnumerator ZoomToDetail(float zoomScale, float duration, bool autoReset, string position)
        {
            Vector3 targetScale = originalScale * zoomScale;
            Vector3 targetPosition = CalculateTargetPosition(zoomScale, position);

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // 使用平滑曲線
                t = Mathf.SmoothStep(0f, 1f, t);

                backgroundTransform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                backgroundTransform.position = Vector3.Lerp(originalPosition, targetPosition, t);

                yield return null;
            }

            // 確保到達目標
            backgroundTransform.localScale = targetScale;
            backgroundTransform.position = targetPosition;
            isZoomed = true;

            // 如果設定自動恢復，等待一段時間後恢復
            if (autoReset)
            {
                yield return new WaitForSeconds(2f); // 顯示 2 秒後恢復
                yield return StartCoroutine(RestoreAnimation(duration));
            }

            Stop();
        }

        private Vector3 CalculateTargetPosition(float zoomScale, string position)
        {
            if (Camera.main == null) return originalPosition;

            // 計算螢幕尺寸
            float screenHeight = Camera.main.orthographicSize * 2;
            float screenWidth = screenHeight * Camera.main.aspect;

            // 計算放大後的 sprite 尺寸
            Vector2 spriteSize = backgroundRenderer.sprite.bounds.size;
            Vector2 scaledSize = spriteSize * zoomScale * originalScale.x;

            float margin = 0.3f; // 邊距
            float x, y;

            switch (position)
            {
                case "left-center-up":
                case "left-center":
                    // 左邊中間偏上一點
                    // x: 左邊對齊，確保放大圖的左邊緣對齊或超出螢幕左邊緣（避免露出藍色）
                    // 如果放大圖不夠寬，則讓它居中；如果夠寬，則左對齊
                    if (scaledSize.x >= screenWidth)
                    {
                        // 放大圖夠寬，可以左對齊
                        x = (-screenWidth / 2) + (scaledSize.x / 2) - margin;
                    }
                    else
                    {
                        // 放大圖不夠寬，居中顯示（避免露出左右邊緣）
                        x = 0f; // 螢幕中心
                    }

                    // y: 讓「圖片的中上部分」對齊到「螢幕中心（或稍微偏上）」
                    // 計算邏輯：
                    // 1. 螢幕中心是 y = 0，螢幕上方是 y > 0，下方是 y < 0
                    // 2. 圖片中心是 y（我們要計算的值）
                    // 3. 圖片的上邊緣 = y + scaledSize.y/2
                    // 4. 圖片的中上部分（從上往下 1/4 處）= y + scaledSize.y/2 - scaledSize.y/4 = y + scaledSize.y/4
                    // 5. 我們要讓「圖片的中上部分」對齊到「螢幕中心往上 offset」
                    // 6. 所以：y + scaledSize.y/4 = offset，因此 y = offset - scaledSize.y/4

                    // 目標：讓圖片的中上部分對齊到螢幕中心（或稍微偏上）
                    // 這樣螢幕會顯示圖片的上半部分（中上區域）
                    // 圖片需要往下移動（y 變成負值），讓圖片的上半部分顯示在螢幕上

                    // 計算：讓圖片的中上部分對齊到螢幕中心（或稍微偏上）
                    // 要讓玩家看到圖片的下面一點點，圖片需要往上移動（y 變大）
                    // 圖片的中上部分 = y + scaledSize.y/4（從上往下 1/4 處）
                    // 讓它對齊到螢幕中心往上 offset = screenHeight * offsetPercent
                    float offsetPercent = 0.05f; // 螢幕中心往上 5%（稍微偏上一點）
                    float targetY = screenHeight * offsetPercent; // 目標位置（螢幕中心往上）

                    // 計算圖片中心：讓圖片往上移動，讓玩家看到圖片的下面一點點
                    // 要讓圖片往上移動，需要增加 y 值（讓 y 更接近 0 或正值）
                    // 使用更大的除數（1/7），讓 y 更大（圖片往上移動）
                    y = targetY - (scaledSize.y / 7);

                    // 不檢查邊緣，直接使用計算出的 y 值
                    // 因為放大圖通常會比螢幕大，下邊緣露出是正常的
                    break;

                case "bottom-left":
                default:
                    // 左下角位置（預設）
                    // 確保放大圖能覆蓋螢幕，如果不夠大則調整位置
                    if (scaledSize.x >= screenWidth)
                    {
                        x = (-screenWidth / 2) + (scaledSize.x / 2) - margin;
                    }
                    else
                    {
                        x = 0f; // 居中
                    }

                    if (scaledSize.y >= screenHeight)
                    {
                        y = (-screenHeight / 2) + (scaledSize.y / 2) - margin;
                    }
                    else
                    {
                        y = 0f; // 居中
                    }
                    break;
            }

            return new Vector3(x, y, originalPosition.z);
        }

        private IEnumerator RestoreAnimation(float duration)
        {
            Vector3 startScale = backgroundTransform.localScale;
            Vector3 startPosition = backgroundTransform.position;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = Mathf.SmoothStep(0f, 1f, t);

                backgroundTransform.localScale = Vector3.Lerp(startScale, originalScale, t);
                backgroundTransform.position = Vector3.Lerp(startPosition, originalPosition, t);

                yield return null;
            }

            RestoreOriginalState();
        }

        private void RestoreOriginalState()
        {
            if (backgroundTransform != null)
            {
                backgroundTransform.localScale = originalScale;
                backgroundTransform.position = originalPosition;
            }
            isZoomed = false;
        }

        public void Update()
        {
            // 等待協程完成
        }

        public void OnDestroy()
        {
            // 清理：停止所有協程
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }
        }
    }
}

