using System;
using System.IO;
using UnityEngine;

public class PersistentDataManager : MonoBehaviour
{
    // Singleton 模式
    public static PersistentDataManager Instance { get; private set; }

    private string saveFolderPath;

    void Awake()
    {
        // 確保只有一個 PersistentDataManager 存在
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 切換場景時不會被銷毀

            // 設定存檔資料夾路徑
            saveFolderPath = Application.persistentDataPath;
            Debug.Log($"存檔資料夾: {saveFolderPath}");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 儲存資料到 JSON 檔案
    /// 用法: SaveData(playerData, "player") 會儲存到 player.json
    /// </summary>
    public void SaveData<T>(T data, string fileName)
    {
        try
        {
            // 將資料轉換為 JSON 格式
            string json = JsonUtility.ToJson(data, true); // true = 美化格式

            // 建立完整檔案路徑
            string filePath = Path.Combine(saveFolderPath, fileName + ".json");

            // 寫入檔案
            File.WriteAllText(filePath, json);

            Debug.Log($"資料已儲存: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"儲存資料失敗 ({fileName}.json): {e.Message}");
        }
    }

    /// <summary>
    /// 從 JSON 檔案讀取資料
    /// 用法: LoadData<Player>("player") 會從 player.json 讀取
    /// </summary>
    public T LoadData<T>(string fileName) where T : new()
    {
        try
        {
            // 建立完整檔案路徑
            string filePath = Path.Combine(saveFolderPath, fileName + ".json");

            // 檢查檔案是否存在
            if (File.Exists(filePath))
            {
                // 讀取檔案內容
                string json = File.ReadAllText(filePath);

                // 將 JSON 轉換為物件
                T data = JsonUtility.FromJson<T>(json);

                Debug.Log($"資料已載入: {filePath}");
                return data;
            }
            else
            {
                Debug.LogWarning($"存檔檔案不存在 ({fileName}.json)，返回預設資料");
                return new T(); // 返回預設建構的物件
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"載入資料失敗 ({fileName}.json): {e.Message}");
            return new T();
        }
    }

    /// <summary>
    /// 刪除指定的存檔檔案
    /// </summary>
    public void DeleteSaveData(string fileName)
    {
        try
        {
            string filePath = Path.Combine(saveFolderPath, fileName + ".json");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"存檔已刪除: {fileName}.json");
            }
            else
            {
                Debug.LogWarning($"存檔檔案不存在: {fileName}.json");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"刪除存檔失敗 ({fileName}.json): {e.Message}");
        }
    }

    /// <summary>
    /// 檢查指定存檔是否存在
    /// </summary>
    public bool HasSaveData(string fileName)
    {
        string filePath = Path.Combine(saveFolderPath, fileName + ".json");
        return File.Exists(filePath);
    }

    /// <summary>
    /// 取得存檔檔案的完整路徑
    /// </summary>
    public string GetSaveFilePath(string fileName)
    {
        return Path.Combine(saveFolderPath, fileName + ".json");
    }
}
