using UnityEngine;
using System.Collections;

public class StatusToast : MonoBehaviour
{
    [SerializeField] private float displayDuration = 2f;

    private RectTransform rectTransform;

    private Coroutine hideCoroutine;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(-150, -154); // 初始隱藏位置

        // 訂閱狀態變化事件
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStatusHeartChanged += OnStatusChanged;
            GameManager.Instance.OnStatusMoneyChanged += OnStatusChanged;
            GameManager.Instance.OnStatusEnergyChanged += OnStatusChanged;
        }
    }


    private void OnStatusChanged(int newValue)
    {
        Debug.Log("狀態改變，顯示 Toast");
        ShowToast();
    }

    private void ShowToast()
    {
        // 停止之前的隱藏協程
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        // 顯示 Toast
        rectTransform.anchoredPosition = new Vector2(99, -154); // 移動到顯示位置

        // 開始新的隱藏協程
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        rectTransform.anchoredPosition = new Vector2(-150, -154); // 移動到隱藏位置
    }
}
