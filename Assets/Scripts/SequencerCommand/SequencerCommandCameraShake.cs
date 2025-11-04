using UnityEngine;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// Sequencer Command: CameraShake(duration, intensity)
    /// 
    /// 使用範例：
    /// - CameraShake(0.3, 0.2)      // 輕微晃動 0.3 秒
    /// - CameraShake(0.5, 0.5)      // 中等晃動 0.5 秒
    /// - CameraShake(1.0, 0.8)      // 劇烈晃動 1 秒
    /// - CameraShake()              // 使用預設值（0.3 秒, 強度 0.2）
    /// 
    /// 參數說明：
    /// - duration: 晃動持續時間（秒），預設 0.3
    /// - intensity: 晃動強度，預設 0.2
    ///   - 0.1 = 輕微（輕輕碰撞）
    ///   - 0.2 = 中等（踩到東西）
    ///   - 0.3-0.5 = 劇烈（跌倒、爆炸）
    ///   - 0.8+ = 超劇烈（地震）
    /// </summary>
    public class SequencerCommandCameraShake : SequencerCommand
    {
        private float duration;
        private float intensity;
        private float elapsed = 0f;

        public void Awake()
        {
            // 獲取參數
            duration = GetParameterAsFloat(0, 0.3f);   // 預設 0.3 秒
            intensity = GetParameterAsFloat(1, 0.2f);  // 預設強度 0.2

            // 確保 CameraShakeController 存在
            if (CameraShakeController.Instance == null)
            {
                // 嘗試尋找場景中的 CameraShakeController
                CameraShakeController existing = GameObject.FindObjectOfType<CameraShakeController>();
                
                if (existing == null)
                {
                    // 如果找不到，自動創建一個
                    GameObject shakeObj = new GameObject("CameraShakeController");
                    shakeObj.AddComponent<CameraShakeController>();
                    Debug.Log("[CameraShake] 自動創建 CameraShakeController");
                }
            }

            // 觸發晃動
            if (CameraShakeController.Instance != null)
            {
                CameraShakeController.Instance.Shake(duration, intensity);
                Debug.Log($"[CameraShake] 觸發晃動: duration={duration}s, intensity={intensity}");
            }
            else
            {
                Debug.LogError("[CameraShake] 無法找到或創建 CameraShakeController");
                Stop();
                return;
            }
        }

        public void Update()
        {
            // 等待晃動完成
            elapsed += Time.deltaTime;
            if (elapsed >= duration)
            {
                Stop();
            }
        }

        public void OnDestroy()
        {
            // 清理工作（如果需要）
        }
    }
}

