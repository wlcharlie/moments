using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// Sequencer Command: SetEventAble(conversationTitle, true/false)
    /// 設定指定事件的啟用狀態 (使用 conversationTitle)
    ///
    /// 用法:
    /// SetEventAble(CH01_EVENT01, true)   - 啟用事件
    /// SetEventAble(CH01_EVENT01, false)  - 停用事件
    /// </summary>
    public class SequencerCommandSetEventAble : SequencerCommand
    {
        void Awake()
        {
            string conversationTitle = GetParameter(0);
            bool isAble = GetParameterAsBool(1, true);

            if (string.IsNullOrEmpty(conversationTitle))
            {
                Debug.LogWarning("[SetEventAble] 缺少 conversationTitle 參數");
                Stop();
                return;
            }

            if (EventAbleManager.Instance == null)
            {
                Debug.LogWarning("[SetEventAble] EventAbleManager 未初始化");
                Stop();
                return;
            }

            EventAbleManager.Instance.SetAble(conversationTitle, isAble);
            Stop();
        }
    }
}
