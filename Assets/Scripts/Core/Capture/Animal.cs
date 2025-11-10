using UnityEngine;

public class Animal : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float minMoveDistance = 2f; // 最小移動距離
    [SerializeField] private float targetMoveTime = 1f; // 移動到目標的預期時間（秒）
    [SerializeField, Range(0.01f, 0.5f)] private float smoothness = 0.1f; // 平滑度（越小越順暢，但移動越慢）
    [SerializeField] private float switchTargetDistance = 1f; // 距離目標多近時切換到下一個點（不會真的停下）

    [Header("範圍設定")]
    [SerializeField] private Vector2 movementAreaSize = new Vector2(10f, 8f); // 可移動範圍大小
    [SerializeField] private Vector2 movementAreaCenter = Vector2.zero; // 可移動範圍中心點

    [Header("回到中央設定")]
    [SerializeField] private float returnToCenterTime = 1f; // 回到中央的時間（秒）

    private Vector2 currentTarget;
    private Bounds movementBounds;
    private bool canMove = true;
    private bool isReturningToCenter = false;
    private Vector2 centerPosition;

    void Start()
    {
        // 計算移動範圍邊界
        UpdateMovementBounds();

        // 設定初始位置在範圍內
        transform.position = GetClampedPosition(transform.position);

        // 選擇第一個目標點
        PickNewTarget();
    }

    void Update()
    {
        if (!canMove)
            return;

        if (isReturningToCenter)
        {
            // 回到中央的移動邏輯
            Vector2 currentPos = transform.position;
            float step = (1f / returnToCenterTime) * Time.deltaTime;
            Vector2 newPos = Vector2.Lerp(currentPos, centerPosition, step);
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);

            // 到達中央後停止
            if (Vector2.Distance(currentPos, centerPosition) < 0.01f)
            {
                transform.position = new Vector3(centerPosition.x, centerPosition.y, transform.position.z);
                canMove = false;
                isReturningToCenter = false;
            }
        }
        else
        {
            // 原本的隨機移動邏輯
            Vector2 currentPos = transform.position;

            // 根據距離和目標時間計算動態速度
            float distanceToTarget = Vector2.Distance(currentPos, currentTarget);
            float dynamicSpeed = distanceToTarget / targetMoveTime;

            // 使用 Lerp 進行平滑移動
            float lerpSpeed = smoothness / Time.deltaTime;
            Vector2 newPos = Vector2.Lerp(currentPos, currentTarget, dynamicSpeed * Time.deltaTime * lerpSpeed);
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);

            // 當接近目標時，選擇下一個目標（不等完全到達）
            if (distanceToTarget < switchTargetDistance)
            {
                PickNewTarget();
            }
        }
    }

    private void PickNewTarget()
    {
        MoveToRandomPosition();
    }

    private void MoveToRandomPosition()
    {
        Vector2 newTarget;
        int maxAttempts = 20; // 最大嘗試次數，避免無限循環
        int attempts = 0;

        do
        {
            // 在範圍內生成隨機位置
            float randomX = Random.Range(movementBounds.min.x, movementBounds.max.x);
            float randomY = Random.Range(movementBounds.min.y, movementBounds.max.y);
            newTarget = new Vector2(randomX, randomY);
            attempts++;

            // 如果嘗試太多次，接受當前位置
            if (attempts >= maxAttempts)
            {
                break;
            }
        }
        while (Vector2.Distance(transform.position, newTarget) < minMoveDistance);

        currentTarget = newTarget;
    }

    private void UpdateMovementBounds()
    {
        movementBounds = new Bounds(movementAreaCenter, new Vector3(movementAreaSize.x, movementAreaSize.y, 0));
    }

    private Vector2 GetClampedPosition(Vector2 position)
    {
        float clampedX = Mathf.Clamp(position.x, movementBounds.min.x, movementBounds.max.x);
        float clampedY = Mathf.Clamp(position.y, movementBounds.min.y, movementBounds.max.y);
        return new Vector2(clampedX, clampedY);
    }

    private void OnValidate()
    {
        // 當 Inspector 中的值改變時更新邊界
        UpdateMovementBounds();
    }

    // 公開方法：停止移動並回到中央
    public void StopAndReturnToCenter()
    {
        isReturningToCenter = true;
        canMove = true;
        centerPosition = Vector2.zero; // 畫面中央是 (0, 0)
    }

    private void OnDrawGizmos()
    {
        // 更新邊界以反映最新設定
        UpdateMovementBounds();

        // 畫出移動範圍（綠色邊框）
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(movementAreaCenter, new Vector3(movementAreaSize.x, movementAreaSize.y, 0));

        // 如果在遊戲運行中，顯示當前目標點和路徑
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentTarget, 0.3f);

            // 畫出從當前位置到目標的線
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentTarget);

            // 顯示切換目標的距離範圍（藍色圓圈）
            Gizmos.color = Color.cyan;
            DrawCircle(currentTarget, switchTargetDistance, 24);
        }

        // 畫出最小移動距離範圍（半透明紅色圓圈）
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        DrawCircle(transform.position, minMoveDistance, 32);
    }

    private void DrawCircle(Vector2 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector2(radius, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}
