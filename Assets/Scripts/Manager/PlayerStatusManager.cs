using UnityEngine;

/// <summary>
/// 管理玩家角色狀態（心情、金錢、體力）
/// </summary>
public class PlayerStatusManager : MonoBehaviour
{
    public static PlayerStatusManager Instance { get; private set; }

    // 狀態值
    private int statusHeart = 50;
    private int statusMoney = 50;
    private int statusEnergy = 50;

    // 公開屬性
    public int StatusHeart => statusHeart;
    public int StatusMoney => statusMoney;
    public int StatusEnergy => statusEnergy;

    // 事件委派
    public delegate void StatusChangedHandler(int newValue);
    public event StatusChangedHandler OnStatusHeartChanged;
    public event StatusChangedHandler OnStatusMoneyChanged;
    public event StatusChangedHandler OnStatusEnergyChanged;

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

    /// <summary>
    /// 更新心情值
    /// </summary>
    /// <param name="amount">增減量（正數增加，負數減少）</param>
    public void UpdateStatusHeart(int amount)
    {
        statusHeart = Mathf.Clamp(statusHeart + amount, 0, 100);
        OnStatusHeartChanged?.Invoke(statusHeart);
        Debug.Log($"心情值: {statusHeart}");
    }

    /// <summary>
    /// 更新金錢值
    /// </summary>
    /// <param name="amount">增減量（正數增加，負數減少）</param>
    public void UpdateStatusMoney(int amount)
    {
        statusMoney = Mathf.Clamp(statusMoney + amount, 0, 100);
        OnStatusMoneyChanged?.Invoke(statusMoney);
        Debug.Log($"金錢值: {statusMoney}");
    }

    /// <summary>
    /// 更新體力值
    /// </summary>
    /// <param name="amount">增減量（正數增加，負數減少）</param>
    public void UpdateStatusEnergy(int amount)
    {
        statusEnergy = Mathf.Clamp(statusEnergy + amount, 0, 100);
        OnStatusEnergyChanged?.Invoke(statusEnergy);
        Debug.Log($"體力值: {statusEnergy}");
    }

    /// <summary>
    /// 直接設定心情值
    /// </summary>
    public void SetStatusHeart(int value)
    {
        statusHeart = Mathf.Clamp(value, 0, 100);
        OnStatusHeartChanged?.Invoke(statusHeart);
    }

    /// <summary>
    /// 直接設定金錢值
    /// </summary>
    public void SetStatusMoney(int value)
    {
        statusMoney = Mathf.Clamp(value, 0, 100);
        OnStatusMoneyChanged?.Invoke(statusMoney);
    }

    /// <summary>
    /// 直接設定體力值
    /// </summary>
    public void SetStatusEnergy(int value)
    {
        statusEnergy = Mathf.Clamp(value, 0, 100);
        OnStatusEnergyChanged?.Invoke(statusEnergy);
    }

    /// <summary>
    /// 重置所有狀態到初始值
    /// </summary>
    public void ResetAllStatus()
    {
        statusHeart = 50;
        statusMoney = 50;
        statusEnergy = 50;

        OnStatusHeartChanged?.Invoke(statusHeart);
        OnStatusMoneyChanged?.Invoke(statusMoney);
        OnStatusEnergyChanged?.Invoke(statusEnergy);

        Debug.Log("所有狀態已重置");
    }
}
