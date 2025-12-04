using UnityEngine;
using System.Collections;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// Sequencer command to switch conversations with transition effect.
    /// 用法: SwitchConversation(conversationTitle[, transitionType])
    /// - conversationTitle: 要啟動的對話 Title (必需)
    /// - transitionType: 轉場類型 (可選，預設為 "None")
    ///   選項: "Cover" (滑動遮罩), "Splash" (白色閃光), "FadeIn" (淡入淡出), "None" (無轉場)
    ///
    /// 範例:
    /// - SwitchConversation(CH01_SC02_SE02)
    /// - SwitchConversation(CH01_SC02_SE02, None)
    /// - SwitchConversation(CH01_SC02_SE02, Cover)
    /// - SwitchConversation(CH01_SC02_SE02, Splash)
    /// - SwitchConversation(CH01_SC02_SE02, FadeIn)
    /// </summary>
    public class SequencerCommandSwitchConversation : SequencerCommand
    {
        public void Start()
        {
            // 獲取對話 Title 參數
            string conversationTitle = GetParameter(0);

            // 驗證對話 Title
            if (string.IsNullOrEmpty(conversationTitle))
            {
                Debug.LogWarning("SwitchConversation: Conversation Title is required.");
                Stop();
                return;
            }

            // 獲取轉場類型參數 (可選，預設為 None)
            string transitionTypeString = GetParameter(1, "None");
            TransitionType transitionType = TransitionType.Cover;
            bool useTransition = false;

            // 解析轉場類型
            if (!transitionTypeString.Equals("None", System.StringComparison.OrdinalIgnoreCase))
            {
                if (System.Enum.TryParse<TransitionType>(transitionTypeString, true, out transitionType))
                {
                    useTransition = true;
                }
            }

            // 使用協程執行切換
            StartCoroutine(SwitchConversationCoroutine(conversationTitle, useTransition, transitionType));
        }

        private IEnumerator SwitchConversationCoroutine(string conversationTitle, bool useTransition, TransitionType transitionType)
        {
            // 等待一幀，確保當前對話狀態穩定
            yield return null;

            // 執行切換
            if (useTransition && TransitionManager.Instance != null)
            {
                // 使用轉場效果切換對話
                TransitionManager.Instance.DoConversationTransition(transitionType, () =>
                {
                    // 在 TransitionManager 上啟動協程
                    TransitionManager.Instance.StartCoroutine(StartNewConversationDelayed(conversationTitle));
                });
            }
            else
            {
                // 直接切換對話（無轉場）
                yield return StartCoroutine(StartNewConversationDelayed(conversationTitle));
            }

            Stop();
        }

        private IEnumerator StartNewConversationDelayed(string conversationTitle)
        {
            Debug.Log($"SwitchConversation: 開始啟動對話 '{conversationTitle}'");
            
            // 停止所有對話
            DialogueManager.StopAllConversations();
            
            // 等待更多幀確保對話完全停止和 Sequencer 清理完成
            // 需要足夠的時間讓舊對話的 Sequencer 完全停止
            yield return null;
            yield return null;
            yield return null;
            yield return new WaitForEndOfFrame();
            
            // 檢查 DialogueManager
            if (DialogueManager.instance == null)
            {
                Debug.LogError($"SwitchConversation: DialogueManager.instance 為 null，無法啟動對話 '{conversationTitle}'");
                yield break;
            }
            
            // 啟動新對話
            Debug.Log($"SwitchConversation: 啟動對話 '{conversationTitle}'");
            DialogueManager.StartConversation(conversationTitle);
            
            // 等待對話完全初始化，包括 Sequencer 的設置
            yield return null;
            yield return null;
            yield return new WaitForEndOfFrame();
            
            if (DialogueManager.isConversationActive)
            {
                Debug.Log($"SwitchConversation: 對話 '{conversationTitle}' 已成功啟動");
            }
            else
            {
                Debug.LogWarning($"SwitchConversation: 對話 '{conversationTitle}' 可能未成功啟動");
            }
        }

        public void OnDestroy()
        {
            // No cleanup needed for this command.
        }
    }
}
