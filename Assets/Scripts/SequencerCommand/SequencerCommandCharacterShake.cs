using UnityEngine;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// 控制角色圖片晃動效果
    /// 用法: CharacterShake(mode, intensity, frequency, duration)
    /// 範例: CharacterShake(start, 0.1, 2, 0) - 開始持續晃動，強度 0.1，頻率 2Hz
    /// 範例: CharacterShake(once, 0.15, 3, 0.5) - 單次晃動 0.5 秒，強度 0.15，頻率 3Hz
    /// 範例: CharacterShake(stop) - 停止晃動
    /// </summary>
    public class SequencerCommandCharacterShake : SequencerCommand
    {
        public void Awake()
        {
            // 參數: mode, intensity, frequency, duration
            string mode = GetParameter(0, "start").ToLower();
            float intensity = GetParameterAsFloat(1, 0.1f);
            float frequency = GetParameterAsFloat(2, 2f);
            float duration = GetParameterAsFloat(3, 0.5f);

            GameObject characterObject = GameObject.Find("CharacterImage");
            if (characterObject == null)
            {
                Debug.LogWarning("CharacterShake: 找不到 CharacterImage 物件");
                Stop();
                return;
            }

            CharacterImageShake shakeComponent = characterObject.GetComponent<CharacterImageShake>();
            if (shakeComponent == null)
            {
                Debug.LogWarning("CharacterShake: CharacterImage 物件沒有 CharacterImageShake 組件");
                Stop();
                return;
            }

            switch (mode)
            {
                case "start":
                case "continuous":
                    // 設定參數
                    shakeComponent.ShakeIntensity = intensity;
                    shakeComponent.ShakeFrequency = frequency;
                    shakeComponent.StartContinuousShake();
                    break;

                case "once":
                case "single":
                    shakeComponent.ShakeOnce(duration > 0 ? duration : null);
                    break;

                case "stop":
                    shakeComponent.StopShake();
                    break;

                default:
                    Debug.LogWarning($"CharacterShake: 未知的模式 '{mode}'，使用 'start', 'once', 或 'stop'");
                    break;
            }

            Stop();
        }
    }
}

