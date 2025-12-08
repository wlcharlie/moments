using UnityEngine;

/// <summary>
/// 地圖節點 - 代表捷運路線圖上的一個站點
/// 使用單向 Linked List 結構連接各節點
/// </summary>
[ExecuteInEditMode]
public class MapNode : MonoBehaviour
{
    [Header("節點資訊")]
    [SerializeField] private string nodeName;
    [SerializeField] private Sprite thumbnail;
    [Tooltip("空節點僅作為路徑點，不計入步數")]
    [SerializeField] private bool isEmpty;
    [Tooltip("起點節點")]
    [SerializeField] private bool isStart;
    [Tooltip("終點節點")]
    [SerializeField] private bool isEnd;

    [Header("視覺設定")]
    [SerializeField] private Sprite dotSprite;
    [SerializeField] private float thumbnailScale = 0.5f;
    [SerializeField] private Vector2 thumbnailOffset = new(0, 1f);

    [Header("連結")]
    [SerializeField] private MapNode nextNode;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private Color lineColor = new(0.831f, 0.741f, 0.639f, 1f); // #D4BDA3
    [Tooltip("曲線彎曲程度，0 = 直線")]
    [SerializeField] private float curvature = 0.5f;
    [SerializeField] private int curveSegments = 20;

    [Header("事件")]
    [SerializeField] private string conversationTitle;

    private SpriteRenderer dotRenderer;
    private SpriteRenderer thumbnailRenderer;
    private LineRenderer lineRenderer;
    private Vector3 lastPosition;
    private Vector3 lastNextNodePosition;

    public string NodeName => nodeName;
    public Sprite Thumbnail => thumbnail;
    public MapNode NextNode => nextNode;
    public string ConversationTitle => conversationTitle;
    public bool IsEmpty => isEmpty;
    public bool IsStart => isStart;
    public bool IsEnd => isEnd;

    /// <summary>
    /// 從 EventData 初始化節點
    /// </summary>
    public void Initialize(EventData eventData, Sprite defaultDotSprite, float lineCurvature = 0.5f, float lineThickness = 0.1f)
    {
        nodeName = eventData.name;
        thumbnail = eventData.thumbnail;
        conversationTitle = eventData.conversationTitle;
        dotSprite = defaultDotSprite;
        curvature = lineCurvature;
        lineWidth = lineThickness;
        isEmpty = false;
        isStart = false;
        isEnd = false;

        SetupVisuals();
    }

    /// <summary>
    /// 初始化為空節點 (起點/終點/路徑點)
    /// </summary>
    public void InitializeEmpty(string name, Sprite defaultDotSprite, bool start = false, bool end = false, float lineCurvature = 0.5f, float lineThickness = 0.1f)
    {
        nodeName = name;
        thumbnail = null;
        conversationTitle = null;
        dotSprite = defaultDotSprite;
        curvature = lineCurvature;
        lineWidth = lineThickness;
        isEmpty = !start && !end; // 起點終點不算空節點
        isStart = start;
        isEnd = end;

        SetupVisuals();
    }

    /// <summary>
    /// 設定下一個節點
    /// </summary>
    public void SetNextNode(MapNode node)
    {
        nextNode = node;
        SetupLine();
        lastNextNodePosition = nextNode != null ? nextNode.transform.position : Vector3.zero;
    }

    private void OnEnable()
    {
        SetupVisuals();
        SetupLine();
        lastPosition = transform.position;
        lastNextNodePosition = nextNode != null ? nextNode.transform.position : Vector3.zero;
    }

    private void Update()
    {
        // 檢查位置是否改變，更新連線
        if (lineRenderer != null && nextNode != null)
        {
            bool positionChanged = transform.position != lastPosition;
            bool nextNodeMoved = nextNode.transform.position != lastNextNodePosition;

            if (positionChanged || nextNodeMoved)
            {
                UpdateLinePositions();
                lastPosition = transform.position;
                lastNextNodePosition = nextNode.transform.position;
            }
        }
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        // 延遲更新，避免在 OnValidate 中直接操作物件
        UnityEditor.EditorApplication.delayCall += UpdateVisuals;
        UnityEditor.EditorApplication.delayCall += UpdateLine;
#endif
    }

    private void UpdateVisuals()
    {
        if (this == null) return;

        // 更新 Dot
        if (dotRenderer != null)
        {
            dotRenderer.sprite = dotSprite;
        }

        // 更新 Thumbnail
        if (thumbnailRenderer != null)
        {
            thumbnailRenderer.sprite = thumbnail;
            thumbnailRenderer.transform.localPosition = new Vector3(thumbnailOffset.x, thumbnailOffset.y, 0);
            thumbnailRenderer.transform.localScale = Vector3.one * thumbnailScale;
        }

        // 如果還沒建立，重新建立
        if ((dotSprite != null && dotRenderer == null) || (thumbnail != null && thumbnailRenderer == null))
        {
            SetupVisuals();
        }
    }

    private void UpdateLine()
    {
        if (this == null) return;

        if (lineRenderer != null)
        {
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
            UpdateLinePositions();
        }

        // 如果有 nextNode 但沒有 LineRenderer，建立它
        if (nextNode != null && lineRenderer == null)
        {
            SetupLine();
        }
        // 如果沒有 nextNode 但有 LineRenderer，移除它
        else if (nextNode == null && lineRenderer != null)
        {
            DestroyImmediate(lineRenderer.gameObject);
            lineRenderer = null;
        }
    }

    private void SetupLine()
    {
        if (nextNode == null) return;

        Transform existing = transform.Find("Line");
        if (existing != null)
        {
            lineRenderer = existing.GetComponent<LineRenderer>();
        }
        else
        {
            GameObject lineObj = new("Line");
            lineObj.transform.SetParent(transform);
            lineObj.transform.localPosition = Vector3.zero;
            lineRenderer = lineObj.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.sortingOrder = 3;

            // 使用預設的 Sprite 材質
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.positionCount = curveSegments + 1;

        UpdateLinePositions();
    }

    private void UpdateLinePositions()
    {
        if (lineRenderer == null || nextNode == null) return;

        Vector3 start = transform.position;
        Vector3 end = nextNode.transform.position;

        // 計算曲線控制點 (垂直於連線方向偏移)
        Vector3 direction = (end - start).normalized;
        Vector3 perpendicular = new(-direction.y, direction.x, 0);
        Vector3 midPoint = (start + end) / 2f;
        Vector3 controlPoint = midPoint + perpendicular * curvature;

        // 使用二次貝茲曲線
        lineRenderer.positionCount = curveSegments + 1;
        for (int i = 0; i <= curveSegments; i++)
        {
            float t = i / (float)curveSegments;
            Vector3 point = QuadraticBezier(start, controlPoint, end, t);
            lineRenderer.SetPosition(i, point);
        }
    }

    private Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        // B(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2
        float u = 1 - t;
        return u * u * p0 + 2 * u * t * p1 + t * t * p2;
    }

    /// <summary>
    /// 取得從此節點到下一節點的曲線上的位置 (t: 0~1)
    /// </summary>
    public Vector3 GetPositionOnCurve(float t)
    {
        if (nextNode == null) return transform.position;

        Vector3 start = transform.position;
        Vector3 end = nextNode.transform.position;

        // 如果沒有彎曲，直接線性插值
        if (Mathf.Approximately(curvature, 0f))
        {
            return Vector3.Lerp(start, end, t);
        }

        // 計算曲線控制點
        Vector3 direction = (end - start).normalized;
        Vector3 perpendicular = new(-direction.y, direction.x, 0);
        Vector3 midPoint = (start + end) / 2f;
        Vector3 controlPoint = midPoint + perpendicular * curvature;

        return QuadraticBezier(start, controlPoint, end, t);
    }

    private void SetupVisuals()
    {
        // 建立節點底座 (SpotDot)
        if (dotSprite != null && dotRenderer == null)
        {
            Transform existing = transform.Find("Dot");
            if (existing != null)
            {
                dotRenderer = existing.GetComponent<SpriteRenderer>();
            }
            else
            {
                GameObject dotObj = new("Dot");
                dotObj.transform.SetParent(transform);
                dotObj.transform.localPosition = Vector3.zero;
                dotRenderer = dotObj.AddComponent<SpriteRenderer>();
            }
            dotRenderer.sprite = dotSprite;
            dotRenderer.sortingOrder = 4;
        }

        // 建立縮圖
        if (thumbnail != null && thumbnailRenderer == null)
        {
            Transform existing = transform.Find("Thumbnail");
            if (existing != null)
            {
                thumbnailRenderer = existing.GetComponent<SpriteRenderer>();
            }
            else
            {
                GameObject thumbObj = new("Thumbnail");
                thumbObj.transform.SetParent(transform);
                thumbObj.transform.localPosition = new Vector3(thumbnailOffset.x, thumbnailOffset.y, 0);
                thumbObj.transform.localScale = Vector3.one * thumbnailScale;
                thumbnailRenderer = thumbObj.AddComponent<SpriteRenderer>();
            }
            thumbnailRenderer.sprite = thumbnail;
            thumbnailRenderer.sortingOrder = 5;
        }
    }

    /// <summary>
    /// 取得從此節點往前 steps 步的節點
    /// </summary>
    public MapNode GetNodeAtSteps(int steps)
    {
        MapNode current = this;
        for (int i = 0; i < steps; i++)
        {
            if (current.nextNode == null)
            {
                Debug.LogWarning($"已到達路線終點: {current.nodeName}");
                break;
            }
            current = current.nextNode;
        }
        return current;
    }

    private void OnDrawGizmos()
    {
        // 在 Scene 視窗繪製節點連線，方便編輯
        if (nextNode != null)
        {
            Gizmos.color = new Color(0f, 0.6f, 0.6f, 1f);
            Gizmos.DrawLine(transform.position, nextNode.transform.position);

            // 繪製箭頭方向
            Vector3 direction = (nextNode.transform.position - transform.position).normalized;
            Vector3 midPoint = (transform.position + nextNode.transform.position) / 2f;
            Vector3 arrowRight = Quaternion.Euler(0, 0, 135) * direction * 0.3f;
            Vector3 arrowLeft = Quaternion.Euler(0, 0, -135) * direction * 0.3f;
            Gizmos.DrawLine(midPoint, midPoint + arrowRight);
            Gizmos.DrawLine(midPoint, midPoint + arrowLeft);
        }

        // 繪製節點圓圈
        Gizmos.color = new Color(0.8f, 0.65f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}
