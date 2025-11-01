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

            // 調用 GameManager 更新對應的狀態
            if (GameManager.Instance != null)
            {
                switch (statusType)
                {
                    case "Heart":
                        GameManager.Instance.UpdateStatusHeart(amount);
                        break;
                    case "Money":
                        GameManager.Instance.UpdateStatusMoney(amount);
                        break;
                    case "Energy":
                        GameManager.Instance.UpdateStatusEnergy(amount);
                        break;
                    default:
                        Debug.LogWarning($"UpdateStatus: Unknown status type '{statusType}'. Use 'Heart', 'Money', or 'Energy'.");
                        break;
                }
            }
            else
            {
                Debug.LogError("UpdateStatus: GameManager.Instance is null.");
            }

            // 立即完成命令
            Stop();
        }
    }
}
