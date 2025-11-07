using UnityEngine;

namespace Mugio.Core   // 或改成 Mugio.Utils，看你放哪個資料夾
{
    public class BGSlowRotate : MonoBehaviour
    {
        [Header("Rotation Settings")]
        public float speed = 18f; // 每秒旋轉角度

        [Header("Optional Fade")]
        public CanvasGroup fadeGroup;
        public bool autoFadeOut = false;
        public float fadeStartTime = 2f;
        public float fadeDuration = 1f;

        private float timer;

        void Update()
        {
            // 持續旋轉
            transform.Rotate(0, 0, speed * Time.deltaTime);

            // 若設定了自動淡出
            if (autoFadeOut && fadeGroup)
            {
                timer += Time.deltaTime;
                if (timer > fadeStartTime)
                {
                    float t = (timer - fadeStartTime) / fadeDuration;
                    fadeGroup.alpha = Mathf.Lerp(1f, 0f, t);
                }
            }
        }
    }
}
