using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 地圖玩家 - 控制玩家在捷運路線圖上的移動
/// </summary>
public class MapPlayer : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private MapNode startNode;
    [Tooltip("沿曲線移動的時間 (秒)")]
    [SerializeField] private float moveDuration = 0.5f;

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

    private void Start()
    {
        // 初始化位置到起始節點
        if (startNode != null)
        {
            currentNode = startNode;
            transform.position = startNode.transform.position;
        }
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

        // 如果起點是 Start 節點，不計入步數，先移動離開
        if (currentNode.IsStart)
        {
            MapNode targetNode = currentNode.NextNode;
            if (targetNode != null)
            {
                yield return MoveAlongCurve(currentNode);
                currentNode = targetNode;
                OnNodePassed?.Invoke(currentNode);
            }
        }

        while (stepsRemaining > 0)
        {
            // 到達終點節點，停止移動
            if (currentNode.IsEnd)
            {
                Debug.Log($"已到達終點: {currentNode.NodeName}");
                break;
            }

            MapNode targetNode = currentNode.NextNode;

            if (targetNode == null)
            {
                Debug.Log($"已到達路線盡頭: {currentNode.NodeName}");
                break;
            }

            // 沿著曲線移動到下一個節點
            yield return MoveAlongCurve(currentNode);

            currentNode = targetNode;
            OnNodePassed?.Invoke(currentNode);

            // 只有非空節點才計入步數
            if (!currentNode.IsEmpty)
            {
                stepsRemaining--;
                // 每步之間稍微停頓 (只在還有步數時)
                if (stepsRemaining > 0)
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        IsMoving = false;
        Debug.Log($"到達節點: {currentNode.NodeName}");
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
}
