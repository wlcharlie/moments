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
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float nodeArriveThreshold = 0.01f;

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

        for (int i = 0; i < steps; i++)
        {
            MapNode targetNode = currentNode.NextNode;

            if (targetNode == null)
            {
                Debug.Log($"已到達終點: {currentNode.NodeName}");
                break;
            }

            // 移動到下一個節點
            yield return MoveToPosition(targetNode.transform.position);

            currentNode = targetNode;
            OnNodePassed?.Invoke(currentNode);

            // 每步之間稍微停頓
            yield return new WaitForSeconds(0.1f);
        }

        IsMoving = false;
        Debug.Log($"到達節點: {currentNode.NodeName}");
        OnMoveComplete?.Invoke(currentNode);
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > nodeArriveThreshold)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = targetPosition;
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
