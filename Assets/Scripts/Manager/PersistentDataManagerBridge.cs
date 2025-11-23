using UnityEngine;

/// <summary>
/// 轉發呼叫給 PersistentDataManager Singleton
/// 用於 UI 事件綁定，避免直接引用 Singleton
/// </summary>
public class PersistentDataManagerBridge : MonoBehaviour
{
    public void DeleteSaveData(string fileName)
    {
        if (PersistentDataManager.Instance != null)
            PersistentDataManager.Instance.DeleteSaveData(fileName);
    }

    public void DeleteAllSaveData()
    {
        if (PersistentDataManager.Instance != null)
            PersistentDataManager.Instance.DeleteAllSaveData();
    }
}
