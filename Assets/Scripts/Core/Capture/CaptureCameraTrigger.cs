using UnityEngine;
using System;

public class CaptureCameraTrigger : MonoBehaviour
{
    // 定義 event 供外部訂閱
    public event Action<GameObject> OnAnimalEnter;
    public event Action<GameObject> OnAnimalExit;

    void Start()
    {
        // 確保有 Collider2D 且設為 Trigger
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            Debug.LogWarning("CaptureCameraTrigger: 缺少 Collider2D 組件！");
        }
        else if (!collider.isTrigger)
        {
            Debug.LogWarning("CaptureCameraTrigger: Collider2D 應該設為 isTrigger = true");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 檢查物體名字是否為 Animal
        if (other.gameObject.name == "Animal")
        {
            Debug.Log($"動物進入相機範圍：{other.gameObject.name}");
            OnAnimalEnter?.Invoke(other.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.name == "Animal")
        {
            Debug.Log($"動物離開相機範圍：{other.gameObject.name}");
            OnAnimalExit?.Invoke(other.gameObject);
        }
    }
}
