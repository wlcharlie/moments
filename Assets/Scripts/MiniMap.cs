using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Mini Map 管理器
/// </summary>
public class MiniMap : MonoBehaviour
{
    [Header("動態生成設定")]
    [SerializeField] private GameObject tilePrefab; // 格子 Prefab
    [SerializeField] private int tileCount = 7; // 格子數量
    [SerializeField] private Transform tilesContainer; // 格子容器（有 Horizontal Layout Group）

    [Header("組件引用")]
    [SerializeField] private RectTransform playerIcon; // 玩家小角色圖示
    [SerializeField] private List<MiniMapTile> tiles = new List<MiniMapTile>(); // 所有格子

    [Header("事件圖示")]
    [SerializeField] private Sprite[] eventIcons; // 可用的事件圖示（HOUSE, MRT, PARK等）
    [SerializeField] private bool randomizeIcons = true; // 是否隨機分配圖示
    [SerializeField] private Sprite startIcon; // 起點固定圖示（可選）
    [SerializeField] private Sprite endIcon; // 終點固定圖示（可選）
    [SerializeField] private Vector2 eventIconSize = new Vector2(60, 60); // 事件圖示大小

    [Header("移動動畫設定")]
    [SerializeField] private float moveSpeed = 300f; // 移動速度（像素/秒）
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 移動曲線

    [Header("玩家圖示設定")]
    [SerializeField] private Vector2 playerIconFixedPosition = new Vector2(60, 35); // 玩家圖示固定位置（相對 MiniMap，從左下角開始）
    [SerializeField] private bool playerFixed = true; // 玩家是否固定（true = 地圖移動，false = 玩家移動）

    private int currentTileIndex = 0;
    private bool isMoving = false;

    // 移動完成事件
    public System.Action<int> OnPlayerMoveComplete;

    private void Start()
    {
        // 動態生成格子
        GenerateTiles();

        // 設定玩家圖示固定位置
        if (playerFixed && playerIcon != null)
        {
            // 確保玩家圖示不受 Layout Group 影響
            playerIcon.SetParent(transform); // 確保父物件是 MiniMap
            playerIcon.anchoredPosition = playerIconFixedPosition;
        }

        // 等一幀讓 Layout Group 計算完成
        StartCoroutine(InitializePositionAfterLayout());
    }

    /// <summary>
    /// 等待 Layout 計算完成後初始化位置
    /// </summary>
    private IEnumerator InitializePositionAfterLayout()
    {
        yield return null; // 等一幀讓 Layout Group 計算完成

        // 初始化：讓第一個格子的中心對齊玩家位置
        if (playerFixed && tilesContainer != null && tiles.Count > 0)
        {
            // 使用 CalculateMapOffset 計算正確的初始位置
            float initialOffset = CalculateMapOffset(0);

            RectTransform containerRect = tilesContainer.GetComponent<RectTransform>();
            if (containerRect != null)
            {
                containerRect.anchoredPosition = new Vector2(initialOffset, containerRect.anchoredPosition.y);
            }
        }

        // 更新格子狀態
        UpdateTilesState(0);
    }

    /// <summary>
    /// 動態生成所有格子
    /// </summary>
    private void GenerateTiles()
    {
        // 清空現有格子
        ClearAllTiles();

        if (tilePrefab == null)
        {
            Debug.LogError("MiniMap: Tile Prefab 未設置！");
            return;
        }

        if (tilesContainer == null)
        {
            tilesContainer = transform; // 預設使用自己作為容器
        }

        // 準備隨機圖示列表
        List<Sprite> availableIcons = new List<Sprite>();
        if (randomizeIcons && eventIcons != null && eventIcons.Length > 0)
        {
            availableIcons.AddRange(eventIcons);
        }

        // 生成格子
        for (int i = 0; i < tileCount; i++)
        {
            // 實例化格子
            GameObject tileObj = Instantiate(tilePrefab, tilesContainer);
            tileObj.name = $"Tile_{i}";

            // 獲取 MiniMapTile 組件
            MiniMapTile tile = tileObj.GetComponent<MiniMapTile>();
            if (tile == null)
            {
                Debug.LogError($"Tile Prefab 缺少 MiniMapTile 組件！");
                Destroy(tileObj);
                continue;
            }

            // 設定格子索引
            tile.SetTileIndex(i);

            // 分配事件圖示
            Sprite iconToUse = null;

            if (i == 0 && startIcon != null)
            {
                // 起點使用固定圖示
                iconToUse = startIcon;
            }
            else if (i == tileCount - 1 && endIcon != null)
            {
                // 終點使用固定圖示
                iconToUse = endIcon;
            }
            else if (randomizeIcons && availableIcons.Count > 0)
            {
                // 隨機選擇圖示
                int randomIndex = Random.Range(0, availableIcons.Count);
                iconToUse = availableIcons[randomIndex];
                // 不要移除，允許重複
            }
            else if (eventIcons != null && eventIcons.Length > 0)
            {
                // 循環使用圖示
                iconToUse = eventIcons[i % eventIcons.Length];
            }

            tile.SetEventIcon(iconToUse);

            // 設定事件圖示大小
            Image eventIconImage = tile.transform.Find("EventIcon")?.GetComponent<Image>();
            if (eventIconImage != null)
            {
                RectTransform iconRect = eventIconImage.GetComponent<RectTransform>();
                if (iconRect != null)
                {
                    iconRect.sizeDelta = eventIconSize;
                }
            }

            // 添加到列表
            tiles.Add(tile);
        }

        Debug.Log($"MiniMap: 生成了 {tiles.Count} 個格子");
    }

    /// <summary>
    /// 清空所有生成的格子
    /// </summary>
    private void ClearAllTiles()
    {
        // 銷毀所有格子 GameObject
        foreach (var tile in tiles)
        {
            if (tile != null)
            {
                Destroy(tile.gameObject);
            }
        }

        tiles.Clear();
    }

    /// <summary>
    /// 設定玩家位置（帶或不帶動畫）
    /// </summary>
    public void SetPlayerPosition(int targetIndex, bool animated = true)
    {
        if (targetIndex < 0 || targetIndex >= tiles.Count)
        {
            Debug.LogWarning($"無效的格子索引: {targetIndex}");
            return;
        }

        if (isMoving)
        {
            Debug.LogWarning("玩家正在移動中");
            return;
        }

        int previousIndex = currentTileIndex;
        currentTileIndex = targetIndex;

        // 更新格子狀態
        UpdateTilesState(targetIndex);

        if (animated && previousIndex != targetIndex)
        {
            // 播放移動動畫
            StartCoroutine(MovePlayerAnimation(previousIndex, targetIndex));
        }
        else
        {
            // 直接設定位置
            UpdatePlayerIconPosition(targetIndex);
        }
    }

    /// <summary>
    /// 移動玩家（相對移動）
    /// </summary>
    public void MovePlayer(int steps)
    {
        int targetIndex = Mathf.Clamp(currentTileIndex + steps, 0, tiles.Count - 1);
        SetPlayerPosition(targetIndex, true);
    }

    /// <summary>
    /// 玩家移動動畫
    /// </summary>
    private IEnumerator MovePlayerAnimation(int fromIndex, int toIndex)
    {
        isMoving = true;

        if (fromIndex >= tiles.Count || toIndex >= tiles.Count)
        {
            isMoving = false;
            yield break;
        }

        if (playerFixed)
        {
            // 模式1: 玩家固定，地圖移動
            yield return StartCoroutine(MoveMapAnimation(fromIndex, toIndex));
        }
        else
        {
            // 模式2: 地圖固定，玩家移動（原本的方式）
            yield return StartCoroutine(MovePlayerIconAnimation(fromIndex, toIndex));
        }

        isMoving = false;

        // 觸發移動完成事件
        OnPlayerMoveComplete?.Invoke(toIndex);
    }

    /// <summary>
    /// 移動地圖（玩家固定模式）
    /// </summary>
    private IEnumerator MoveMapAnimation(int fromIndex, int toIndex)
    {
        if (tilesContainer == null) yield break;

        // 計算需要移動的距離（向左移動是負值）
        RectTransform containerRect = tilesContainer.GetComponent<RectTransform>();
        if (containerRect == null) yield break;

        Vector2 startPos = containerRect.anchoredPosition;

        // 計算目標位置：讓 toIndex 的格子移動到玩家圖示位置
        float targetOffset = CalculateMapOffset(toIndex);
        Vector2 endPos = new Vector2(targetOffset, startPos.y);

        float distance = Vector2.Distance(startPos, endPos);
        float duration = distance / moveSpeed;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curveT = moveCurve.Evaluate(t);

            containerRect.anchoredPosition = Vector2.Lerp(startPos, endPos, curveT);

            yield return null;
        }

        // 確保最終位置正確
        containerRect.anchoredPosition = endPos;
    }

    /// <summary>
    /// 移動玩家圖示（玩家移動模式）
    /// </summary>
    private IEnumerator MovePlayerIconAnimation(int fromIndex, int toIndex)
    {
        if (playerIcon == null) yield break;

        Vector3 startPos = tiles[fromIndex].GetWorldPosition();
        Vector3 endPos = tiles[toIndex].GetWorldPosition();

        float distance = Vector3.Distance(startPos, endPos);
        float duration = distance / moveSpeed;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curveT = moveCurve.Evaluate(t);

            playerIcon.position = Vector3.Lerp(startPos, endPos, curveT);

            yield return null;
        }

        // 確保最終位置正確
        playerIcon.position = endPos;
    }

    /// <summary>
    /// 計算地圖偏移量，讓指定格子對齊玩家位置
    /// </summary>
    private float CalculateMapOffset(int tileIndex)
    {
        if (tileIndex >= tiles.Count) return 0f;

        // 獲取格子的 RectTransform
        RectTransform tileRect = tiles[tileIndex].GetComponent<RectTransform>();
        if (tileRect == null) return 0f;

        // 獲取格子相對於 tilesContainer 的位置
        float tileLocalX = tileRect.anchoredPosition.x;

        // 玩家圖示的固定位置（相對 MiniMap）
        float playerX = playerIconFixedPosition.x;

        // 需要的偏移量 = 玩家位置 - 格子位置
        // 這樣當前格子會移動到玩家位置
        float offset = playerX - tileLocalX;

        return offset;
    }

    /// <summary>
    /// 更新玩家圖示位置（無動畫）
    /// </summary>
    private void UpdatePlayerIconPosition(int tileIndex)
    {
        if (playerFixed)
        {
            // 玩家固定模式：移動地圖
            if (tilesContainer != null && tiles.Count > 0)
            {
                RectTransform containerRect = tilesContainer.GetComponent<RectTransform>();
                if (containerRect != null)
                {
                    float targetOffset = CalculateMapOffset(tileIndex);
                    containerRect.anchoredPosition = new Vector2(targetOffset, containerRect.anchoredPosition.y);
                }
            }

            // 確保玩家圖示在固定位置
            if (playerIcon != null)
            {
                playerIcon.anchoredPosition = playerIconFixedPosition;
            }
        }
        else
        {
            // 玩家移動模式：移動玩家
            if (playerIcon == null || tileIndex >= tiles.Count) return;
            Vector3 targetPos = tiles[tileIndex].GetWorldPosition();
            playerIcon.position = targetPos;
        }
    }

    /// <summary>
    /// 更新所有格子的狀態
    /// </summary>
    private void UpdateTilesState(int currentIndex)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] == null) continue;

            tiles[i].SetCurrent(i == currentIndex);
            tiles[i].SetPassed(i <= currentIndex);
        }
    }

    /// <summary>
    /// 獲取當前玩家位置索引
    /// </summary>
    public int GetCurrentPosition()
    {
        return currentTileIndex;
    }

    /// <summary>
    /// 是否正在移動
    /// </summary>
    public bool IsMoving()
    {
        return isMoving;
    }

    /// <summary>
    /// 添加格子到列表
    /// </summary>
    public void AddTile(MiniMapTile tile)
    {
        if (!tiles.Contains(tile))
        {
            tiles.Add(tile);
        }
    }

    /// <summary>
    /// 重新生成地圖（用於重置或更改設定）
    /// </summary>
    [ContextMenu("重新生成地圖")]
    public void RegenerateMap()
    {
        GenerateTiles();
        SetPlayerPosition(0, false);
    }

    /// <summary>
    /// 設定格子數量並重新生成
    /// </summary>
    public void SetTileCount(int count)
    {
        tileCount = Mathf.Max(2, count); // 至少2個格子（起點和終點）
        RegenerateMap();
    }

    /// <summary>
    /// 獲取格子數量
    /// </summary>
    public int GetTileCount()
    {
        return tileCount;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 在編輯器中自動收集子物件的格子
    /// </summary>
    [ContextMenu("自動收集格子")]
    private void CollectTiles()
    {
        tiles.Clear();
        MiniMapTile[] foundTiles = GetComponentsInChildren<MiniMapTile>();
        tiles.AddRange(foundTiles);

        // 設定索引
        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].SetTileIndex(i);
        }

        Debug.Log($"收集到 {tiles.Count} 個格子");
    }
#endif
}
