using UnityEngine;
using System.Collections;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// 恢復背景圖到原始狀態（正常大小和位置）
    /// 用法: RestoreSceneDetail(duration)
    /// 範例: RestoreSceneDetail(0.8) - 用 0.8 秒動畫恢復
    /// 範例: RestoreSceneDetail() - 使用預設 0.8 秒
    /// </summary>
    public class SequencerCommandRestoreSceneDetail : SequencerCommand
    {
        private GameObject backgroundObject;
        private Transform backgroundTransform;
        private Coroutine restoreCoroutine;

        // 原始狀態（從 ShowSceneDetail 保存的）
        private static Vector3 originalPosition;
        private static Vector3 originalScale;
        private static bool hasOriginalState = false;

        public void Awake()
        {
            // 參數: duration
            float duration = GetParameterAsFloat(0, 0.8f); // 預設動畫時間 0.8 秒

            // 找到背景物件
            backgroundObject = GameObject.FindGameObjectWithTag("Background");
            if (backgroundObject == null)
            {
                Debug.LogWarning("RestoreSceneDetail: Background object with tag 'Background' not found.");
                Stop();
                return;
            }

            backgroundTransform = backgroundObject.transform;

            // 如果沒有保存的原始狀態，使用當前狀態作為原始狀態
            if (!hasOriginalState)
            {
                originalPosition = backgroundTransform.position;
                originalScale = backgroundTransform.localScale;
                hasOriginalState = true;
                Debug.Log("RestoreSceneDetail: 使用當前狀態作為原始狀態。");
            }

            // 開始恢復動畫
            StartCoroutine(RestoreAnimation(duration));
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
                // 使用平滑曲線
                t = Mathf.SmoothStep(0f, 1f, t);

                backgroundTransform.localScale = Vector3.Lerp(startScale, originalScale, t);
                backgroundTransform.position = Vector3.Lerp(startPosition, originalPosition, t);

                yield return null;
            }

            // 確保到達目標
            backgroundTransform.localScale = originalScale;
            backgroundTransform.position = originalPosition;

            Stop();
        }

        public void Update()
        {
            // 等待協程完成
        }

        public void OnDestroy()
        {
            // 清理：停止所有協程
            if (restoreCoroutine != null)
            {
                StopCoroutine(restoreCoroutine);
            }
        }

        // 靜態方法：讓 ShowSceneDetail 可以保存原始狀態
        public static void SaveOriginalState(Vector3 position, Vector3 scale)
        {
            originalPosition = position;
            originalScale = scale;
            hasOriginalState = true;
        }
    }
}

