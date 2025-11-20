using UnityEngine;
using System.Collections;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// 讓現有背景圖放大並移動到右下角
    /// 用法: ShowSceneDetail(zoomScale, duration, reset)
    /// 範例: ShowSceneDetail(2.5, 0.8, false) - 放大 2.5 倍，動畫 0.8 秒，不自動恢復
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
            // 參數: zoomScale, duration, reset
            float zoomScale = GetParameterAsFloat(0, 2.5f); // 預設放大 2.5 倍
            float duration = GetParameterAsFloat(1, 0.8f); // 預設動畫時間 0.8 秒
            bool reset = GetParameterAsBool(2, false); // 預設不自動恢復

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
                SequencerCommandRestoreSceneDetail.SaveOriginalState(originalPosition, originalScale);
            }

            // 開始放大動畫
            StartCoroutine(ZoomToDetail(zoomScale, duration, reset));
        }

        private IEnumerator ZoomToDetail(float zoomScale, float duration, bool autoReset)
        {
            Vector3 targetScale = originalScale * zoomScale;
            Vector3 targetPosition = CalculateBottomRightPosition(zoomScale);

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

        private Vector3 CalculateBottomRightPosition(float zoomScale)
        {
            if (Camera.main == null) return originalPosition;

            // 計算螢幕尺寸
            float screenHeight = Camera.main.orthographicSize * 2;
            float screenWidth = screenHeight * Camera.main.aspect;

            // 計算放大後的 sprite 尺寸
            Vector2 spriteSize = backgroundRenderer.sprite.bounds.size;
            Vector2 scaledSize = spriteSize * zoomScale * originalScale.x;

            // 計算左下角位置（讓放大圖的左下角對齊螢幕左下角）
            float margin = 0.3f; // 邊距
            float x = (-screenWidth / 2) + (scaledSize.x / 2) - margin;
            float y = (-screenHeight / 2) + (scaledSize.y / 2) - margin;

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

