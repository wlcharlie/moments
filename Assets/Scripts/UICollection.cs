using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 管理收集介面，從 AnimalDatabase 渲染動物收集項目
/// </summary>
public class UICollection : MonoBehaviour
{
    [Header("Database Reference")]
    [SerializeField] private AnimalDatabase animalDatabase;

    [Header("UI References")]
    [SerializeField] private GameObject collectionItemPrefab;

    [Header("Layout Settings")]
    [SerializeField] private bool refreshOnEnable = true;

    private List<GameObject> spawnedItems = new List<GameObject>();

    void OnEnable()
    {
        if (refreshOnEnable)
        {
            RefreshCollection();
        }
    }

    void Start()
    {
        RefreshCollection();
    }

    /// <summary>
    /// 重新渲染整個收集介面
    /// </summary>
    public void RefreshCollection()
    {
        if (animalDatabase == null)
        {
            Debug.LogWarning("AnimalDatabase 未設定！");
            return;
        }

        if (collectionItemPrefab == null)
        {
            Debug.LogWarning("Collection Item Prefab 未設定！");
            return;
        }

        // 清除現有項目
        ClearItems();

        // 取得所有動物資料
        List<AnimalData> animals = animalDatabase.GetAllAnimals();

        Debug.Log($"開始渲染收集項目，共 {animals.Count} 個動物");

        // 為每個動物創建 UI 項目
        foreach (AnimalData animal in animals)
        {
            GameObject itemObj = Instantiate(collectionItemPrefab, transform);
            spawnedItems.Add(itemObj);

            // 取得 UICollectionItem 組件並設定資料
            UICollectionItem itemComponent = itemObj.GetComponent<UICollectionItem>();
            if (itemComponent != null)
            {
                itemComponent.Setup(animal);
            }
            else
            {
                Debug.LogWarning("Prefab 上沒有 UICollectionItem 組件！");
            }

            Debug.Log($"創建項目: {animal.animalName} (No.{animal.no})");
        }

        Debug.Log($"渲染完成，已創建 {spawnedItems.Count} 個項目");
    }

    /// <summary>
    /// 清除所有已生成的項目
    /// </summary>
    private void ClearItems()
    {
        foreach (GameObject item in spawnedItems)
        {
            Destroy(item);
        }
        spawnedItems.Clear();
    }

    /// <summary>
    /// 設定資料庫參考（可用於動態更換資料庫）
    /// </summary>
    public void SetDatabase(AnimalDatabase database)
    {
        animalDatabase = database;
        RefreshCollection();
    }
}
