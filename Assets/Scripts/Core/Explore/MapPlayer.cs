using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 地圖玩家 - 控制玩家在捷運路線圖上的移動
/// </summary>
public class MapPlayer : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("沿曲線移動的時間 (秒)")]
    [SerializeField] private float moveDuration = 0.5f;

    [Header("循環設定")]
    [Tooltip("到達終點後循環回的目標節點 (由 ExploreMapController 設定)")]
    [SerializeField] private MapNode loopBackNode;

    [Header("狀態 (唯讀)")]
    [SerializeField] private MapNode currentNode;

    public MapNode CurrentNode => currentNode;
    public bool IsMoving { get; private set; }

    /// <summary>
    /// 移動完成事件，傳回到達的節點
    /// </summary>
    public event Action<MapNode> OnMoveComplete;

    /// <summary>
    /// 每經過一個節點時觸發
    /// </summary>
    public event Action<MapNode> OnNodePassed;

    /// <summary>
    /// 循環回起點時觸發
    /// </summary>
    public event Action OnLoopBack;

    /// <summary>
    /// 設定到達終點後要循環回的節點
    /// </summary>
    public void SetLoopBackNode(MapNode node)
    {
        loopBackNode = node;
    }

    /// <summary>
    /// 移動指定步數
    /// </summary>
    public void MoveSteps(int steps)
    {
        if (IsMoving)
        {
            Debug.LogWarning("玩家正在移動中");
            return;
        }

        if (steps <= 0)
        {
            Debug.LogWarning("移動步數必須大於 0");
            return;
        }

        if (currentNode == null)
        {
            Debug.LogError("玩家沒有設定起始節點");
            return;
        }

        StartCoroutine(MoveCoroutine(steps));
    }

    private IEnumerator MoveCoroutine(int steps)
    {
        IsMoving = true;
        int stepsRemaining = steps;

        Debug.Log($"[MapPlayer] 開始移動，步數: {steps}");

        while (stepsRemaining > 0)
        {
            MapNode targetNode = currentNode.NextNode;

            // 到達終點節點，循環回第一格
            if (currentNode.IsEnd || targetNode == null)
            {
                if (loopBackNode != null)
                {
                    Debug.Log($"[MapPlayer] 到達終點，循環回: {loopBackNode.NodeName}");
                    // 直接傳送到循環節點
                    currentNode = loopBackNode;
                    transform.position = currentNode.transform.position;
                    stepsRemaining--;
                    OnLoopBack?.Invoke();
                    OnNodePassed?.Invoke(currentNode);

                    if (stepsRemaining > 0)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }
                    continue;
                }
                else
                {
                    Debug.Log($"[MapPlayer] 已到達終點且無循環設定: {currentNode.NodeName}");
                    break;
                }
            }

            // 沿著曲線移動到下一個節點
            yield return MoveAlongCurve(currentNode);

            currentNode = targetNode;
            stepsRemaining--;
            OnNodePassed?.Invoke(currentNode);

            Debug.Log($"[MapPlayer] 到達 {currentNode.NodeName}，剩餘步數: {stepsRemaining}");

            // 每步之間稍微停頓 (只在還有步數時)
            if (stepsRemaining > 0)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        IsMoving = false;
        Debug.Log($"[MapPlayer] 移動完成，最終節點: {currentNode.NodeName}");
        OnMoveComplete?.Invoke(currentNode);
    }

    private IEnumerator MoveAlongCurve(MapNode fromNode)
    {
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);

            // 使用 ease-out 讓移動更自然
            float easedT = 1f - (1f - t) * (1f - t);

            transform.position = fromNode.GetPositionOnCurve(easedT);
            yield return null;
        }

        // 確保到達終點
        transform.position = fromNode.GetPositionOnCurve(1f);
    }

    /// <summary>
    /// 立即傳送到指定節點 (不播放動畫)
    /// </summary>
    public void TeleportToNode(MapNode node)
    {
        if (node == null) return;

        currentNode = node;
        transform.position = node.transform.position;
    }

    /// <summary>
    /// 計算從當前節點到目標節點的步數
    /// </summary>
    /// <returns>步數，如果無法到達則返回 -1</returns>
    public int CalculateStepsToNode(MapNode targetNode)
    {
        if (currentNode == null || targetNode == null) return -1;

        MapNode node = currentNode;
        int steps = 0;
        int maxSteps = 100; // 防止無限迴圈

        while (node != null && node != targetNode && steps < maxSteps)
        {
            node = node.NextNode;
            steps++;
        }

        if (node == targetNode)
        {
            return steps;
        }

        return -1; // 無法到達
    }
}
