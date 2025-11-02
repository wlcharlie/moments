using UnityEngine;

public class CaptureManager : MonoBehaviour
{
    [SerializeField] private GameObject captureCamera;
    [SerializeField] private GameObject animal;
    [SerializeField] private Flash flash;

    private CaptureCameraTrigger cameraTrigger;
    private bool isAnimalInRange = false;

    void Start()
    {
        // 獲取 CaptureCamera 上的 CaptureCameraTrigger 組件
        if (captureCamera != null)
        {
            cameraTrigger = captureCamera.GetComponent<CaptureCameraTrigger>();
            if (cameraTrigger != null)
            {
                // 訂閱事件
                cameraTrigger.OnAnimalEnter += HandleAnimalEnter;
                cameraTrigger.OnAnimalExit += HandleAnimalExit;
                cameraTrigger.OnScreenTap += HandleScreenTap;
            }
            else
            {
                Debug.LogWarning("CaptureManager: captureCamera 上缺少 CaptureCameraTrigger 組件！");
            }
        }

        // 檢查 Flash 組件
        if (flash == null)
        {
            Debug.LogWarning("CaptureManager: 缺少 Flash 組件！");
        }
    }

    void OnDestroy()
    {
        // 取消訂閱事件，避免 memory leak
        if (cameraTrigger != null)
        {
            cameraTrigger.OnAnimalEnter -= HandleAnimalEnter;
            cameraTrigger.OnAnimalExit -= HandleAnimalExit;
            cameraTrigger.OnScreenTap -= HandleScreenTap;
        }
    }

    // 當動物進入相機範圍時的 callback
    private void HandleAnimalEnter(GameObject animalObject)
    {
        isAnimalInRange = true;
        Debug.Log($"CaptureManager 收到通知：{animalObject.name} 進入範圍");
        // 這裡可以加入其他邏輯，例如顯示拍照按鈕等
    }

    // 當動物離開相機範圍時的 callback
    private void HandleAnimalExit(GameObject animalObject)
    {
        isAnimalInRange = false;
        Debug.Log($"CaptureManager 收到通知：{animalObject.name} 離開範圍");
        // 這裡可以加入其他邏輯，例如隱藏拍照按鈕等
    }

    // 當螢幕被點擊時的 callback
    private void HandleScreenTap()
    {
        Debug.Log("CaptureManager: 螢幕被點擊");

        if (flash != null)
        {
            flash.TriggerFlash();
            Debug.Log("觸發閃光效果");
        }

        if (captureCamera != null)
        {
            captureCamera.SetActive(false);
            Debug.Log("CaptureCamera 已停用");
        }
    }

    // 公開方法供外部查詢
    public bool IsAnimalInRange()
    {
        return isAnimalInRange;
    }
}
