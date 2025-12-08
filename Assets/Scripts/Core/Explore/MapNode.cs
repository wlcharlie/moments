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

    [Header("視覺設定")]
    [SerializeField] private Sprite dotSprite;
    [SerializeField] private float thumbnailScale = 0.5f;
    [SerializeField] private Vector2 thumbnailOffset = new(0, 1f);

    [Header("連結")]
    [SerializeField] private MapNode nextNode;

    [Header("事件")]
    [SerializeField] private string conversationId;

    private SpriteRenderer dotRenderer;
    private SpriteRenderer thumbnailRenderer;

    public string NodeName => nodeName;
    public Sprite Thumbnail => thumbnail;
    public MapNode NextNode => nextNode;
    public string ConversationId => conversationId;

    private void OnEnable()
    {
        SetupVisuals();
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        // 延遲更新，避免在 OnValidate 中直接操作物件
        UnityEditor.EditorApplication.delayCall += UpdateVisuals;
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
            dotRenderer.sortingOrder = 0;
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
            thumbnailRenderer.sortingOrder = 1;
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
