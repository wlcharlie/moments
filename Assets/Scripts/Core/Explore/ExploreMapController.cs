using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地圖生成控制器 - 負責根據事件資料動態生成地圖節點
/// </summary>
public class ExploreMapController : MonoBehaviour
{
    [Header("地圖生成")]
    [SerializeField] private EventDatabase eventDatabase;
    [SerializeField] private Transform nodeContainer;
    [SerializeField] private Sprite defaultDotSprite;

    [Header("起點設定")]
    [SerializeField] private bool createStartNode = true;
    [SerializeField] private Sprite startThumbnail;

    [Header("終點設定")]
    [SerializeField] private bool createEndNode = true;
    [SerializeField] private Sprite endThumbnail;

    [Header("地圖排列設定")]
    [SerializeField] private Vector2 startPosition = new(3f, 0f);
    [SerializeField] private float nodeSpacing = 3f;
    [SerializeField] private float curveAmplitude = 1f;

    [Header("線條設定")]
    [SerializeField] private float lineCurvature = 0.3f;
    [SerializeField] private float lineWidth = 0.1f;

    [Header("編輯器預覽")]
    [SerializeField] private GameMode previewMode = GameMode.Story;

    private readonly List<MapNode> generatedNodes = new();

    /// <summary>
    /// 取得起點節點
    /// </summary>
    public MapNode StartNode => generatedNodes.Count > 0 ? generatedNodes[0] : null;

    /// <summary>
    /// 取得所有生成的節點
    /// </summary>
    public IReadOnlyList<MapNode> Nodes => generatedNodes;

    /// <summary>
    /// 根據遊戲模式生成地圖
    /// </summary>
    public MapNode GenerateMap(GameMode mode)
    {
        if (eventDatabase == null)
        {
            Debug.LogError("EventDatabase 未設定");
            return null;
        }

        ClearMap();

        // 取得該模式可用的事件
        List<EventData> events = eventDatabase.GetEventsForMode(mode);
        if (events.Count == 0)
        {
            Debug.LogWarning($"模式 {mode} 沒有可用的事件");
            return null;
        }

        // 打亂事件順序
        ShuffleList(events);

        int nodeIndex = 0;
        MapNode previousNode = null;
        MapNode firstNode = null;

        // 建立起點（如果啟用）
        if (createStartNode)
        {
            Sprite sprite = startThumbnail != null ? startThumbnail : defaultDotSprite;
            MapNode startNodeObj = CreateNodeAt(CalculateNodePosition(nodeIndex));
            startNodeObj.InitializeEmpty("起點", sprite, start: true, lineCurvature: lineCurvature, lineThickness: lineWidth);
            generatedNodes.Add(startNodeObj);
            previousNode = startNodeObj;
            firstNode = startNodeObj;
            nodeIndex++;
        }

        // 建立事件節點
        for (int i = 0; i < events.Count; i++)
        {
            Vector2 position = CalculateNodePosition(nodeIndex);
            MapNode node = CreateNodeAt(position);
            node.Initialize(events[i], defaultDotSprite, lineCurvature, lineWidth);
            generatedNodes.Add(node);

            // 第一個節點設為起點（如果沒有建立起點節點）
            if (firstNode == null)
            {
                firstNode = node;
            }

            // 連結前一個節點
            if (previousNode != null)
            {
                previousNode.SetNextNode(node);
            }
            previousNode = node;
            nodeIndex++;
        }

        // 建立終點（如果啟用）
        if (createEndNode)
        {
            Sprite sprite = endThumbnail != null ? endThumbnail : defaultDotSprite;
            Vector2 endPosition = CalculateNodePosition(nodeIndex);
            MapNode endNode = CreateNodeAt(endPosition);
            endNode.InitializeEmpty("終點", sprite, end: true, lineCurvature: lineCurvature, lineThickness: lineWidth);
            generatedNodes.Add(endNode);
            previousNode?.SetNextNode(endNode);
        }

        Debug.Log($"地圖生成完成: 模式={mode}, 節點數={generatedNodes.Count}");
        return firstNode;
    }

    /// <summary>
    /// 清除地圖
    /// </summary>
    public void ClearMap()
    {
        int count = generatedNodes.Count;

        // 清除 generatedNodes 列表中的節點
        foreach (MapNode node in generatedNodes)
        {
            if (node != null)
            {
                if (Application.isPlaying)
                    Destroy(node.gameObject);
                else
                    DestroyImmediate(node.gameObject);
            }
        }
        generatedNodes.Clear();

        // 清除 nodeContainer 中的所有子物件
        Transform container = nodeContainer != null ? nodeContainer : transform;
        int containerCount = container.childCount;
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Transform child = container.GetChild(i);
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }

        Debug.Log($"[ExploreMapController] ClearMap 執行完成，已清除 {count} 個追蹤節點，{containerCount} 個容器子物件");
    }

    private MapNode CreateNodeAt(Vector2 position)
    {
        GameObject nodeObj = new("MapNode");
        if (nodeContainer != null)
        {
            nodeObj.transform.SetParent(nodeContainer);
            nodeObj.transform.localPosition = new Vector3(position.x, position.y, 0);
        }
        else
        {
            nodeObj.transform.SetParent(transform);
            nodeObj.transform.localPosition = new Vector3(position.x, position.y, 0);
        }

        MapNode node = nodeObj.AddComponent<MapNode>();
        return node;
    }

    private Vector2 CalculateNodePosition(int index)
    {
        float x = startPosition.x + index * nodeSpacing;
        float y = startPosition.y + Mathf.Sin(index * 0.8f) * curveAmplitude;
        return new Vector2(x, y);
    }

    private static void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 編輯器預覽地圖生成
    /// </summary>
    [ContextMenu("預覽地圖")]
    public void PreviewMap()
    {
        if (eventDatabase == null)
        {
            Debug.LogError("EventDatabase 未設定");
            return;
        }

        ClearMap();

        List<EventData> events = eventDatabase.GetEventsForMode(previewMode);
        if (events.Count == 0)
        {
            Debug.LogWarning($"模式 {previewMode} 沒有可用的事件");
            return;
        }

        // 預覽不打亂順序，方便檢視
        int nodeIndex = 0;
        MapNode previousNode = null;

        // 建立起點（如果啟用）
        if (createStartNode)
        {
            Sprite sprite = startThumbnail != null ? startThumbnail : defaultDotSprite;
            MapNode startNodeObj = CreateNodeAt(CalculateNodePosition(nodeIndex));
            startNodeObj.InitializeEmpty("起點", sprite, start: true, lineCurvature: lineCurvature, lineThickness: lineWidth);
            generatedNodes.Add(startNodeObj);
            previousNode = startNodeObj;
            nodeIndex++;
        }

        // 建立事件節點
        for (int i = 0; i < events.Count; i++)
        {
            Vector2 position = CalculateNodePosition(nodeIndex);
            MapNode node = CreateNodeAt(position);
            node.Initialize(events[i], defaultDotSprite, lineCurvature, lineWidth);
            generatedNodes.Add(node);

            if (previousNode != null)
            {
                previousNode.SetNextNode(node);
            }
            previousNode = node;
            nodeIndex++;
        }

        // 建立終點（如果啟用）
        if (createEndNode)
        {
            Sprite sprite = endThumbnail != null ? endThumbnail : defaultDotSprite;
            Vector2 endPosition = CalculateNodePosition(nodeIndex);
            MapNode endNode = CreateNodeAt(endPosition);
            endNode.InitializeEmpty("終點", sprite, end: true, lineCurvature: lineCurvature, lineThickness: lineWidth);
            generatedNodes.Add(endNode);
            previousNode?.SetNextNode(endNode);
        }

        Debug.Log($"預覽地圖生成完成: 模式={previewMode}, 節點數={generatedNodes.Count}");
    }

    /// <summary>
    /// 清除預覽地圖
    /// </summary>
    [ContextMenu("清除預覽")]
    public void ClearPreview()
    {
        ClearMap();
        Debug.Log("預覽地圖已清除");
    }
#endif
}
