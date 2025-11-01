using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimalDatabase", menuName = "Game/Animal Database")]
public class AnimalDatabase : ScriptableObject
{
    [SerializeField] private List<AnimalData> animals = new List<AnimalData>();

    // ===== 查詢方法 =====

    /// <summary>
    /// 根據序號取得動物資料
    /// </summary>
    public AnimalData GetAnimalByNo(string no)
    {
        return animals.Find(animal => animal.no == no);
    }

    /// <summary>
    /// 根據名稱取得動物資料
    /// </summary>
    public AnimalData GetAnimalByName(string animalName)
    {
        return animals.Find(animal => animal.animalName == animalName);
    }

    /// <summary>
    /// 取得所有動物資料
    /// </summary>
    public List<AnimalData> GetAllAnimals()
    {
        return animals;
    }

    /// <summary>
    /// 檢查是否存在指定序號的動物
    /// </summary>
    public bool HasAnimal(string no)
    {
        return animals.Exists(animal => animal.no == no);
    }

    // ===== 狀態更新方法 =====

    /// <summary>
    /// 標記動物為已遇見
    /// </summary>
    public void MarkAsSeen(string no)
    {
        AnimalData animal = GetAnimalByNo(no);
        if (animal != null)
        {
            animal.seen = true;
            Debug.Log($"動物 {animal.animalName} (No.{no}) 已標記為已遇見");
        }
        else
        {
            Debug.LogWarning($"找不到序號為 {no} 的動物");
        }
    }

    /// <summary>
    /// 標記動物為已收集（同時也會標記為已遇見）
    /// </summary>
    public void MarkAsCollected(string no)
    {
        AnimalData animal = GetAnimalByNo(no);
        if (animal != null)
        {
            animal.collected = true;
            animal.seen = true;
            Debug.Log($"動物 {animal.animalName} (No.{no}) 已標記為已收集");
        }
        else
        {
            Debug.LogWarning($"找不到序號為 {no} 的動物");
        }
    }

    // ===== 統計方法 =====

    /// <summary>
    /// 取得已收集的動物數量
    /// </summary>
    public int GetCollectedCount()
    {
        return animals.Count(animal => animal.collected);
    }

    /// <summary>
    /// 取得已遇見的動物數量
    /// </summary>
    public int GetSeenCount()
    {
        return animals.Count(animal => animal.seen);
    }

    /// <summary>
    /// 取得動物總數
    /// </summary>
    public int GetTotalCount()
    {
        return animals.Count;
    }

    /// <summary>
    /// 取得收集進度百分比
    /// </summary>
    public float GetCollectionProgress()
    {
        if (animals.Count == 0) return 0f;
        return (float)GetCollectedCount() / animals.Count * 100f;
    }

    /// <summary>
    /// 取得遇見進度百分比
    /// </summary>
    public float GetSeenProgress()
    {
        if (animals.Count == 0) return 0f;
        return (float)GetSeenCount() / animals.Count * 100f;
    }

    // ===== 篩選查詢方法 =====

    /// <summary>
    /// 取得所有已收集的動物
    /// </summary>
    public List<AnimalData> GetCollectedAnimals()
    {
        return animals.Where(animal => animal.collected).ToList();
    }

    /// <summary>
    /// 取得所有已遇見的動物
    /// </summary>
    public List<AnimalData> GetSeenAnimals()
    {
        return animals.Where(animal => animal.seen).ToList();
    }

    /// <summary>
    /// 取得所有未收集的動物
    /// </summary>
    public List<AnimalData> GetUncollectedAnimals()
    {
        return animals.Where(animal => !animal.collected).ToList();
    }

    // ===== 除錯與工具方法 =====

    /// <summary>
    /// 重置所有動物的收集狀態（除錯用）
    /// </summary>
    [ContextMenu("重置所有收集狀態")]
    public void ResetAllCollectionStatus()
    {
        foreach (var animal in animals)
        {
            animal.collected = false;
            animal.seen = false;
        }
        Debug.Log("已重置所有動物的收集狀態");
    }

    /// <summary>
    /// 解鎖所有動物（除錯用）
    /// </summary>
    [ContextMenu("解鎖所有動物")]
    public void UnlockAllAnimals()
    {
        foreach (var animal in animals)
        {
            animal.collected = true;
            animal.seen = true;
        }
        Debug.Log("已解鎖所有動物");
    }
}
