using UnityEngine;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    public class SequencerCommandUpdateStatus : SequencerCommand
    {
        public void Awake()
        {
            // 獲取參數：狀態類型和數值
            string statusType = GetParameter(0); // "Heart", "Money", "Energy"
            int amount = GetParameterAsInt(1);   // 變化數值（可以是正數或負數）

            // 調用 PlayerStatusManager 更新對應的狀態
            if (PlayerStatusManager.Instance != null)
            {
                switch (statusType)
                {
                    case "Heart":
                        PlayerStatusManager.Instance.UpdateStatusHeart(amount);
                        break;
                    case "Money":
                        PlayerStatusManager.Instance.UpdateStatusMoney(amount);
                        break;
                    case "Energy":
                        PlayerStatusManager.Instance.UpdateStatusEnergy(amount);
                        break;
                    default:
                        Debug.LogWarning($"UpdateStatus: Unknown status type '{statusType}'. Use 'Heart', 'Money', or 'Energy'.");
                        break;
                }
            }
            else
            {
                Debug.LogError("UpdateStatus: PlayerStatusManager.Instance is null.");
            }

            // 立即完成命令
            Stop();
        }
    }
}
