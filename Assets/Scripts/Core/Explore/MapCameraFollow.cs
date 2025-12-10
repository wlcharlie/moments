using UnityEngine;

/// <summary>
/// 地圖跟隨控制器 - 讓玩家保持在畫面固定位置
/// 支援觸控/滑鼠拖曳平移
/// </summary>
public class MapCameraFollow : MonoBehaviour
{
    [Header("跟隨目標")]
    [Tooltip("Player 的 Transform (在 nodeContainer 內)")]
    [SerializeField] private Transform target;

    [Header("地圖容器")]
    [Tooltip("包含所有節點和 Player 的容器")]
    [SerializeField] private Transform mapContainer;

    [Header("自動跟隨設定")]
    [Tooltip("玩家要保持在畫面的 X 位置 (世界座標)")]
    [SerializeField] private float playerScreenX = 0f;

    [Tooltip("玩家與固定位置的距離超過此值時才移動地圖")]
    [SerializeField] private float moveThreshold = 4f;

    [Tooltip("平滑速度 (數值越大越即時)")]
    [SerializeField] private float smoothSpeed = 8f;

    [Header("拖曳設定")]
    [Tooltip("啟用觸控/滑鼠拖曳")]
    [SerializeField] private bool enableDrag = true;

    [Tooltip("拖曳靈敏度")]
    [SerializeField] private float dragSensitivity = 1f;

    private bool isDragging;
    private Vector2 lastDragPosition;
    private bool userDragged;

    void Update()
    {
        if (!enableDrag || mapContainer == null) return;

        HandleDragInput();
    }

    void LateUpdate()
    {
        if (target == null || mapContainer == null) return;

        // 如果用戶正在拖曳，不自動跟隨
        if (isDragging || userDragged) return;

        // 玩家的世界座標 X
        float playerWorldX = target.position.x;

        // 計算玩家與固定位置的距離
        float distance = playerWorldX - playerScreenX;

        // 只有距離超過閾值時才移動地圖
        if (Mathf.Abs(distance) < moveThreshold) return;

        // 計算容器需要移動多少 (只移動超出閾值的部分)
        float offsetX = distance > 0
            ? playerScreenX + moveThreshold - playerWorldX
            : playerScreenX - moveThreshold - playerWorldX;

        // 平滑移動容器
        Vector3 pos = mapContainer.localPosition;
        pos.x = Mathf.Lerp(pos.x, pos.x + offsetX, smoothSpeed * Time.deltaTime);
        mapContainer.localPosition = pos;
    }

    private void HandleDragInput()
    {
        // 觸控輸入
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    isDragging = true;
                    lastDragPosition = touch.position;
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        Vector2 delta = touch.position - lastDragPosition;
                        DragMap(delta.x);
                        lastDragPosition = touch.position;
                        userDragged = true;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isDragging = false;
                    break;
            }
        }
        // 滑鼠輸入
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                lastDragPosition = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0) && isDragging)
            {
                Vector2 currentPos = Input.mousePosition;
                Vector2 delta = currentPos - lastDragPosition;
                DragMap(delta.x);
                lastDragPosition = currentPos;
                userDragged = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
        }
    }

    private void DragMap(float deltaX)
    {
        // 將螢幕像素轉換為世界單位
        float worldDelta = deltaX * dragSensitivity * Camera.main.orthographicSize / Screen.height * 2f;

        Vector3 pos = mapContainer.localPosition;
        pos.x += worldDelta;
        mapContainer.localPosition = pos;
    }

    /// <summary>
    /// 重置用戶拖曳狀態，讓自動跟隨重新生效
    /// </summary>
    public void ResetDragState()
    {
        userDragged = false;
    }

    /// <summary>
    /// 立即對齊讓玩家在閾值邊界內 (不使用平滑)
    /// </summary>
    public void Snap()
    {
        if (target == null || mapContainer == null) return;

        float playerWorldX = target.position.x;
        float distance = playerWorldX - playerScreenX;

        if (Mathf.Abs(distance) < moveThreshold) return;

        float offsetX = distance > 0
            ? playerScreenX + moveThreshold - playerWorldX
            : playerScreenX - moveThreshold - playerWorldX;

        Vector3 pos = mapContainer.localPosition;
        pos.x += offsetX;
        mapContainer.localPosition = pos;
    }

    /// <summary>
    /// 強制讓玩家置中 (用於循環回起點等大幅移動)
    /// </summary>
    public void SnapToCenter()
    {
        if (target == null || mapContainer == null) return;

        float offsetX = playerScreenX - target.position.x;
        Vector3 pos = mapContainer.localPosition;
        pos.x += offsetX;
        mapContainer.localPosition = pos;

        // 重置拖曳狀態
        userDragged = false;
    }
}
