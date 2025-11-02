using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class CaptureCameraTrigger : MonoBehaviour
{
    // 定義 event 供外部訂閱
    public event Action<GameObject> OnAnimalEnter;
    public event Action<GameObject> OnAnimalExit;
    public event Action OnScreenTap;

    private TapInputSet tapInput;

    void Awake()
    {
        // 實例化 Input Actions
        tapInput = new TapInputSet();
    }

    void OnEnable()
    {
        // 訂閱 Tap 事件
        tapInput.Tap.Tap.performed += OnTapPerformed;
        tapInput.Enable();
    }

    void OnDisable()
    {
        // 取消訂閱
        tapInput.Tap.Tap.performed -= OnTapPerformed;
        tapInput.Disable();
    }

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

    void OnTapPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("螢幕被點擊！");
        OnScreenTap?.Invoke();
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
