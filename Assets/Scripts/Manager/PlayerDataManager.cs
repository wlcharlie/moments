using System;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public string uuid;
}

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    private const string SAVE_FILE_NAME = "player";

    public string PlayerId { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 在 Start 中初始化，確保 PersistentDataManager 已經 Awake
        InitializePlayerId();
    }

    private void InitializePlayerId()
    {
        if (PersistentDataManager.Instance == null)
        {
            Debug.LogError("PlayerDataManager: PersistentDataManager.Instance 是 null");
            PlayerId = GenerateUUID();
            return;
        }

        // 嘗試從 JSON 讀取已存在的 PlayerId
        if (PersistentDataManager.Instance.HasSaveData(SAVE_FILE_NAME))
        {
            PlayerData data = PersistentDataManager.Instance.LoadData<PlayerData>(SAVE_FILE_NAME);

            if (!string.IsNullOrEmpty(data.uuid))
            {
                PlayerId = data.uuid;
                Debug.Log($"載入已存在的 PlayerId: {PlayerId}");
                return;
            }
        }

        // 沒有存檔或 uuid 為空，產生新的
        PlayerId = GenerateUUID();
        SavePlayerId();
        Debug.Log($"產生新的 PlayerId: {PlayerId}");
    }

    private string GenerateUUID()
    {
        return Guid.NewGuid().ToString();
    }

    private void SavePlayerId()
    {
        if (PersistentDataManager.Instance == null)
        {
            Debug.LogError("PlayerDataManager: 無法儲存，PersistentDataManager.Instance 是 null");
            return;
        }

        PlayerData data = new PlayerData { uuid = PlayerId };
        PersistentDataManager.Instance.SaveData(data, SAVE_FILE_NAME);
    }

    /// <summary>
    /// 重置 PlayerId（用於測試或清除資料）
    /// </summary>
    public void ResetPlayerId()
    {
        PlayerId = GenerateUUID();
        SavePlayerId();
        Debug.Log($"重置 PlayerId: {PlayerId}");
    }
}
