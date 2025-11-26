using UnityEngine;

/// <summary>
/// 轉發呼叫給 PersistentDataManager Singleton
/// 用於 UI 事件綁定，避免直接引用 Singleton
/// 使用方式：在 Unity Editor 建立 ScriptableObject asset，然後在 UI Button 的 onClick 事件中引用靜態方法
/// 注意：帶參數的方法（如 DeleteSaveData）需要在 UI 中手動輸入參數字串
/// </summary>
[CreateAssetMenu(fileName = "PersistentDataManagerBridge", menuName = "Bridges/Persistent Data Manager Bridge")]
public class PersistentDataManagerBridge : ScriptableObject
{
    // 靜態方法可以直接被 Unity UI 事件系統引用

    public static void DeleteSaveData(string fileName)
    {
        if (PersistentDataManager.Instance != null)
            PersistentDataManager.Instance.DeleteSaveData(fileName);
    }

    public static void DeleteAllSaveData()
    {
        if (PersistentDataManager.Instance != null)
            PersistentDataManager.Instance.DeleteAllSaveData();
    }
}
