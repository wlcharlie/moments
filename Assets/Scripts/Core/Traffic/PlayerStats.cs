using UnityEngine;
using System;

/// <summary>
/// 玩家狀態管理（單例模式）
/// 集中管理 Heart、Money、Energy 三種狀態
/// </summary>
public class PlayerStats : MonoBehaviour
{
    // 單例
    private static PlayerStats _instance;
    public static PlayerStats Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PlayerStats>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("PlayerStats");
                    _instance = go.AddComponent<PlayerStats>();
                }
            }
            return _instance;
        }
    }

    [Header("初始狀態值")]
    [SerializeField] private int initialHeart = 100;
    [SerializeField] private int initialMoney = 100;
    [SerializeField] private int initialEnergy = 100;

    [Header("最大最小值限制")]
    [SerializeField] private int minValue = 0;
    [SerializeField] private int maxHeart = 100;
    [SerializeField] private int maxMoney = 100;
    [SerializeField] private int maxEnergy = 100;

    [Header("當前狀態值")]
    [SerializeField] private int currentHeart;
    [SerializeField] private int currentMoney;
    [SerializeField] private int currentEnergy;

    // 狀態變化事件
    public event Action<int, int> OnHeartChanged; // (當前值, 最大值)
    public event Action<int, int> OnMoneyChanged;
    public event Action<int, int> OnEnergyChanged;

    // 任意狀態變化時觸發
    public event Action OnAnyStatChanged;

    private void Awake()
    {
        // 確保單例唯一性
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject); // 跨場景保持

        // 初始化狀態
        Initialize();
    }

    /// <summary>
    /// 初始化所有狀態到初始值
    /// </summary>
    public void Initialize()
    {
        currentHeart = initialHeart;
        currentMoney = initialMoney;
        currentEnergy = initialEnergy;

        // 觸發事件通知 UI 更新
        NotifyAllStatsChanged();
    }

    /// <summary>
    /// 重置所有狀態到初始值
    /// </summary>
    public void ResetStats()
    {
        Initialize();
    }

    #region Heart (愛心) 相關

    public int GetHeart() => currentHeart;
    public int GetMaxHeart() => maxHeart;

    /// <summary>
    /// 設定 Heart 值（直接設定）
    /// </summary>
    public void SetHeart(int value)
    {
        int oldValue = currentHeart;
        currentHeart = Mathf.Clamp(value, minValue, maxHeart);

        if (oldValue != currentHeart)
        {
            OnHeartChanged?.Invoke(currentHeart, maxHeart);
            OnAnyStatChanged?.Invoke();
            Debug.Log($"Heart: {oldValue} → {currentHeart}");
        }
    }

    /// <summary>
    /// 增加 Heart
    /// </summary>
    public void AddHeart(int amount)
    {
        SetHeart(currentHeart + amount);
    }

    /// <summary>
    /// 減少 Heart
    /// </summary>
    public void RemoveHeart(int amount)
    {
        SetHeart(currentHeart - amount);
    }

    #endregion

    #region Money (金錢) 相關

    public int GetMoney() => currentMoney;
    public int GetMaxMoney() => maxMoney;

    /// <summary>
    /// 設定 Money 值（直接設定）
    /// </summary>
    public void SetMoney(int value)
    {
        int oldValue = currentMoney;
        currentMoney = Mathf.Clamp(value, minValue, maxMoney);

        if (oldValue != currentMoney)
        {
            OnMoneyChanged?.Invoke(currentMoney, maxMoney);
            OnAnyStatChanged?.Invoke();
            Debug.Log($"Money: {oldValue} → {currentMoney}");
        }
    }

    /// <summary>
    /// 增加 Money
    /// </summary>
    public void AddMoney(int amount)
    {
        SetMoney(currentMoney + amount);
    }

    /// <summary>
    /// 減少 Money
    /// </summary>
    public void RemoveMoney(int amount)
    {
        SetMoney(currentMoney - amount);
    }

    #endregion

    #region Energy (能量) 相關

    public int GetEnergy() => currentEnergy;
    public int GetMaxEnergy() => maxEnergy;

    /// <summary>
    /// 設定 Energy 值（直接設定）
    /// </summary>
    public void SetEnergy(int value)
    {
        int oldValue = currentEnergy;
        currentEnergy = Mathf.Clamp(value, minValue, maxEnergy);

        if (oldValue != currentEnergy)
        {
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
            OnAnyStatChanged?.Invoke();
            Debug.Log($"Energy: {oldValue} → {currentEnergy}");
        }
    }

    /// <summary>
    /// 增加 Energy
    /// </summary>
    public void AddEnergy(int amount)
    {
        SetEnergy(currentEnergy + amount);
    }

    /// <summary>
    /// 減少 Energy
    /// </summary>
    public void RemoveEnergy(int amount)
    {
        SetEnergy(currentEnergy - amount);
    }

    #endregion

    #region 批量操作

    /// <summary>
    /// 一次性修改多個狀態
    /// </summary>
    public void ModifyStats(int heartDelta = 0, int moneyDelta = 0, int energyDelta = 0)
    {
        if (heartDelta != 0) AddHeart(heartDelta);
        if (moneyDelta != 0) AddMoney(moneyDelta);
        if (energyDelta != 0) AddEnergy(energyDelta);
    }

    /// <summary>
    /// 通知所有狀態已變化（用於初始化或刷新 UI）
    /// </summary>
    public void NotifyAllStatsChanged()
    {
        OnHeartChanged?.Invoke(currentHeart, maxHeart);
        OnMoneyChanged?.Invoke(currentMoney, maxMoney);
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        OnAnyStatChanged?.Invoke();
    }

    #endregion

    #region 檢查狀態

    /// <summary>
    /// 檢查是否有足夠的 Money
    /// </summary>
    public bool HasEnoughMoney(int amount)
    {
        return currentMoney >= amount;
    }

    /// <summary>
    /// 檢查是否有足夠的 Energy
    /// </summary>
    public bool HasEnoughEnergy(int amount)
    {
        return currentEnergy >= amount;
    }

    /// <summary>
    /// 檢查 Heart 是否為 0（遊戲結束條件）
    /// </summary>
    public bool IsHeartZero()
    {
        return currentHeart <= 0;
    }

    #endregion

#if UNITY_EDITOR
    [ContextMenu("測試：增加 Heart +10")]
    private void TestAddHeart()
    {
        AddHeart(10);
    }

    [ContextMenu("測試：減少 Heart -10")]
    private void TestRemoveHeart()
    {
        RemoveHeart(10);
    }

    [ContextMenu("測試：增加 Money +20")]
    private void TestAddMoney()
    {
        AddMoney(20);
    }

    [ContextMenu("測試：減少 Energy -15")]
    private void TestRemoveEnergy()
    {
        RemoveEnergy(15);
    }

    [ContextMenu("測試：重置所有狀態")]
    private void TestResetStats()
    {
        ResetStats();
    }
#endif
}
