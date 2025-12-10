# 探索地圖系統 (Explore Map System)

大富翁風格的直線地圖系統，玩家擲骰子後沿著節點前進，到達節點時觸發對應事件。

## 核心元件

### ExploreGameManager
主要控制器，負責整合骰子、地圖、玩家的互動。

**職責：**
- 初始化地圖和玩家位置
- 處理骰子結果，驅動玩家移動
- 管理存檔/讀檔 (僅 Event 模式)
- 處理 Story 模式的強制流程

**Inspector 設定：**
| 欄位 | 說明 |
|------|------|
| Dice Controller | 骰子控制器 |
| Map Controller | 地圖生成控制器 |
| Map Player | 玩家物件 |
| Map Camera Follow | 地圖視角跟隨控制器 |
| Use Override Mode | 勾選後使用指定模式，否則使用 GameManager 的模式 |
| Override Mode | 指定的遊戲模式 |

---

### ExploreMapController
負責根據事件資料動態生成地圖節點。

**職責：**
- 生成起點、事件節點、終點
- 管理節點之間的連線
- 支援使用 seed 生成相同順序的地圖 (Event 模式)
- 提供節點查詢功能

**Inspector 設定：**
| 欄位 | 說明 |
|------|------|
| Event Database | 事件資料庫 |
| Node Container | 節點的父物件容器 |
| Default Dot Sprite | 節點預設圖示 |
| Create Start Node | 是否建立起點節點 |
| Start Thumbnail | 起點圖示 |
| Create End Node | 是否建立終點節點 |
| End Thumbnail | 終點圖示 |
| Start Position | 地圖起始位置 |
| Node Spacing | 節點間距 |
| Curve Amplitude | 節點 Y 軸波動幅度 |
| Line Curvature | 連線彎曲程度 |
| Line Width | 連線寬度 |
| Marked Dot Sprite | 標記節點的圖示 |

**主要方法：**
```csharp
// 生成地圖 (隨機順序)
MapNode GenerateMap(GameMode mode)

// 生成地圖 (指定 seed，用於恢復存檔)
MapNode GenerateMap(GameMode mode, int seed)

// 清除地圖
void ClearMap()

// 根據 ConversationTitle 查找節點
MapNode FindNodeByConversationTitle(string conversationTitle)

// 標記指定節點
MapNode MarkNodeByConversationTitle(string conversationTitle)
```

---

### MapPlayer
控制玩家在地圖上的移動。

**職責：**
- 沿著節點曲線移動
- 處理終點循環回起點
- 發送移動相關事件

**Inspector 設定：**
| 欄位 | 說明 |
|------|------|
| Move Duration | 移動到下一個節點的時間 (秒) |
| Loop Back Node | 到達終點後循環回的節點 (由程式設定) |

**事件：**
```csharp
// 移動完成時觸發
event Action<MapNode> OnMoveComplete

// 每經過一個節點時觸發
event Action<MapNode> OnNodePassed

// 循環回起點時觸發
event Action OnLoopBack
```

**主要方法：**
```csharp
// 移動指定步數
void MoveSteps(int steps)

// 傳送到指定節點
void TeleportToNode(MapNode node)

// 設定循環回的節點
void SetLoopBackNode(MapNode node)

// 計算到目標節點的步數
int CalculateStepsToNode(MapNode targetNode)
```

---

### MapNode
代表地圖上的一個節點。

**屬性：**
| 屬性 | 說明 |
|------|------|
| NodeName | 節點名稱 |
| Thumbnail | 縮圖 |
| ConversationTitle | 對應的對話標題 |
| NextNode | 下一個節點 |
| IsStart | 是否為起點 |
| IsEnd | 是否為終點 |
| IsEmpty | 是否為空節點 |
| IsMarked | 是否被標記 |

---

### MapCameraFollow
地圖視角控制器，移動地圖容器讓玩家保持在畫面範圍內。

**功能：**
- 玩家超出邊界時自動平移地圖
- 支援觸控/滑鼠拖曳平移
- 循環回起點時自動置中

**Inspector 設定：**
| 欄位 | 說明 |
|------|------|
| Target | 跟隨目標 (MapPlayer) |
| Map Container | 地圖容器 |
| Player Screen X | 玩家要保持的 X 位置 |
| Move Threshold | 觸發平移的距離閾值 |
| Move Amount | 平移距離 |
| Smooth Speed | 平滑速度 |
| Enable Drag | 啟用拖曳 |
| Drag Sensitivity | 拖曳靈敏度 |

**自動跟隨邏輯：**
- 玩家與中心距離 < `moveThreshold`：不移動
- 玩家與中心距離 >= `moveThreshold`：地圖平移 `moveAmount`

---

### DiceController
骰子控制器。

**事件：**
```csharp
// 擲骰完成時觸發，回傳結果 (1-6)
event Action<int> OnRollComplete
```

**主要方法：**
```csharp
// 隨機擲骰
void Roll()

// 擲出指定結果
void RollWithResult(int result)
```

---

### EventDatabase
事件資料庫 (ScriptableObject)。

**Story 模式取得事件邏輯：**
1. 優先取得故事專屬事件 (`canStoryMode && !canEventMode`)
2. 不夠則從共用事件補充 (`canStoryMode && canEventMode`)，補充前會先打亂
3. 最多 6 個
4. 最後打亂整體順序

**Event 模式取得事件邏輯：**
- 取得所有 `canEventMode` 的事件
- 由 ExploreMapController 使用 seed 打亂

---

### EventData
單一事件資料。

| 欄位 | 說明 |
|------|------|
| name | 事件名稱 |
| thumbnail | 縮圖 |
| conversationTitle | 對應的對話標題 |
| canStoryMode | 在故事模式中可用 |
| canEventMode | 在事件模式中可用 |

---

### ExploreMapSaveData
地圖存檔資料 (僅 Event 模式使用)。

| 欄位 | 說明 |
|------|------|
| mode | 遊戲模式 |
| seed | 隨機種子 |
| playerNodeIndex | 玩家位置索引 |
| isValid | 是否有效 |

---

## 遊戲模式差異

| | Story 模式 | Event 模式 |
|---|---|---|
| 事件數量 | 最多 6 個 | 無限制 |
| 事件來源 | 優先故事專屬 | 所有 canEventMode |
| 順序 | 每次隨機 (不存檔) | 使用 seed (會存檔) |
| 存檔 | 不存檔 | 存檔位置和順序 |
| 強制流程 | 支援 | 不支援 |

---

## 場景層級結構建議

```
Scene
├── Main Camera
├── UI Canvas
├── ExploreGameManager
├── DiceController
├── MapCameraFollow
└── NodeContainer (會被 MapCameraFollow 移動)
    ├── MapNode (起點)
    ├── MapNode (事件1)
    ├── MapNode (事件2)
    ├── ...
    ├── MapNode (終點)
    └── MapPlayer
```

---

## 流程說明

### 初始化流程
1. ExploreGameManager.Start()
2. 根據模式決定是否載入存檔
3. ExploreMapController.GenerateMap() 生成地圖
4. MapPlayer.TeleportToNode() 傳送玩家到起點/存檔位置
5. 設定 MapPlayer.loopBackNode 為第一個事件節點

### 擲骰流程
1. 外部呼叫 ExploreGameManager.RollDice()
2. DiceController.Roll() 執行擲骰動畫
3. DiceController.OnRollComplete 事件觸發
4. MapCameraFollow.ResetDragState() 重置拖曳狀態
5. MapPlayer.MoveSteps() 開始移動
6. 每經過節點觸發 MapPlayer.OnNodePassed
7. 移動完成觸發 MapPlayer.OnMoveComplete
8. 如果有 ConversationTitle，進入對話場景

### 終點循環流程
1. MapPlayer 到達終點節點
2. 檢查 loopBackNode 是否存在
3. 傳送到 loopBackNode
4. 觸發 MapPlayer.OnLoopBack
5. MapCameraFollow.SnapToCenter() 重置視角
