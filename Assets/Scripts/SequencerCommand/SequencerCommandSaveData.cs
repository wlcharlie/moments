using UnityEngine;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// Sequencer Command: SaveData(key, value, [filepath])
    ///
    /// 用法範例:
    /// - SaveData(playerName, 小明) → 儲存到 dialogue.json
    /// - SaveData(visitedScene1, true) → 儲存布林值
    /// - SaveData(chapter, 3) → 儲存數字
    /// - SaveData(playerName, 小明, player) → 儲存到 player.json
    ///
    /// 注意:
    /// - 第三個參數 filepath 是選填的，預設為 "dialogue"
    /// - 會將資料儲存為 key-value pairs 格式
    /// - 多次呼叫會累積資料，不會覆蓋整個檔案
    /// </summary>
    public class SequencerCommandSaveData : SequencerCommand
    {
        public void Awake()
        {
            // 獲取參數
            string key = GetParameter(0);           // key 名稱
            string value = GetParameter(1);         // value 值
            string filePath = GetParameter(2);      // 選填的檔案路徑

            // 如果沒有提供 filePath，使用預設值 "dialogue"
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = "dialogue";
            }

            // 檢查必要參數
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("SaveData: key 參數不能為空");
                Stop();
                return;
            }

            if (string.IsNullOrEmpty(value))
            {
                Debug.LogWarning($"SaveData: value 參數為空 (key: {key})");
            }

            // 執行儲存
            if (global::PersistentDataManager.Instance != null)
            {
                // 載入現有資料（如果有的話）
                DialogueData data = global::PersistentDataManager.Instance.LoadData<DialogueData>(filePath);

                // 如果資料結構是 null，初始化它
                if (data == null)
                {
                    data = new DialogueData();
                }


                // 新增或更新 key-value
                if (data.ContainsKey(key))
                {
                    Debug.Log($"SaveData: 更新 {key} = {value} (檔案: {filePath}.json)");
                }
                else
                {
                    Debug.Log($"SaveData: 新增 {key} = {value} (檔案: {filePath}.json)");
                }

                data.SetValue(key, value);

                // 儲存回檔案
                global::PersistentDataManager.Instance.SaveData(data, filePath);
            }
            else
            {
                Debug.LogError("SaveData: PersistentDataManager.Instance is null.");
            }

            // 立即完成命令
            Stop();
        }
    }
}
