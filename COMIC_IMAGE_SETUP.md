# ComicImageLeft 和 ComicImageRight 設置指南

## 概述

現在系統支援三個位置的 Comic Image：
- **ComicImage**（原有的，向後相容）
- **ComicImageLeft**（新增）
- **ComicImageRight**（新增）

## Unity Editor 設置步驟

### 1. 在場景中創建新的 GameObject

1. 打開 `MainStoryScene`（或其他需要使用 Comic Image 的場景）
2. 在 Hierarchy 中創建兩個新的 GameObject：
   - 命名為 `ComicImageLeft`
   - 命名為 `ComicImageRight`

### 2. 設置 Transform 組件

為每個 GameObject 設置合適的位置：

**ComicImageLeft（左側位置）：**
- Position: 例如 `{x: -3, y: 2, z: 0}`（根據你的需求調整）

**ComicImageRight（右側位置）：**
- Position: 例如 `{x: 3, y: 2, z: 0}`（根據你的需求調整）

### 3. 添加組件

為每個 GameObject 添加以下組件：

1. **Sprite Renderer** 組件
   - 初始 Sprite 可以留空（會由對話系統自動設置）
   - 調整 Sorting Layer 和 Order in Layer 以確保正確的渲染順序

2. **ComicImageFader** 組件
   - 在 Inspector 中將 Sprite Renderer 拖拽到 `Sprite Renderer` 欄位
   - 調整 `Fade Duration`（預設為 0.5 秒）

### 4. 設置範例

```
MainStoryScene
├── Main Camera
├── Background
├── ComicImage (原有)
│   ├── Transform: {x: 0, y: 2, z: 0}
│   ├── Sprite Renderer
│   └── ComicImageFader
├── ComicImageLeft (新增)
│   ├── Transform: {x: -3, y: 2, z: 0}
│   ├── Sprite Renderer
│   └── ComicImageFader
└── ComicImageRight (新增)
    ├── Transform: {x: 3, y: 2, z: 0}
    ├── Sprite Renderer
    └── ComicImageFader
```

## 對話系統使用方式

### 在 Dialogue Database 中添加欄位

在對話條目中，你可以使用以下三個欄位：

1. **Comic Image**（原有）
   - 設置到 `ComicImage` GameObject

2. **Comic Image Left**（新增）
   - 設置到 `ComicImageLeft` GameObject
   - 欄位名稱：`Comic Image Left`

3. **Comic Image Right**（新增）
   - 設置到 `ComicImageRight` GameObject
   - 欄位名稱：`Comic Image Right`

### 欄位值格式

使用 Addressables 路徑，例如：
- `"Assets/Arts/Comics/comic_panel_01.png"`
- `"Assets/Arts/Comics/comic_panel_left.png"`
- `"Assets/Arts/Comics/comic_panel_right.png"`

如果欄位值為空或 null，對應的 Comic Image 會被清空（淡出）。

## 功能說明

- ✅ **自動淡入淡出**：所有 Comic Image 都使用 `ComicImageFader` 組件，當 Sprite 改變時會自動淡入淡出
- ✅ **向後相容**：原有的 `Comic Image` 欄位仍然可以正常使用
- ✅ **獨立控制**：三個位置可以獨立設置，互不影響
- ✅ **空值處理**：如果欄位為空，對應的圖片會淡出

## 注意事項

1. GameObject 名稱必須完全匹配：
   - `ComicImage`（區分大小寫）
   - `ComicImageLeft`（區分大小寫）
   - `ComicImageRight`（區分大小寫）

2. 每個 GameObject 都需要：
   - SpriteRenderer 組件
   - ComicImageFader 組件

3. 位置設置：
   - 根據你的遊戲設計需求調整 Transform 的 Position
   - 可以設置不同的 Y 值來實現上下排列
   - 可以設置不同的 Z 值來控制深度排序

4. Sorting Layer 設置：
   - 確保 Comic Image 的 Sorting Layer 設置正確，以便正確顯示在其他元素之前或之後

## 範例場景

如果你想要在對話中同時顯示左右兩個漫畫面板：

```
對話條目欄位設置：
- Comic Image Left: "Assets/Arts/Comics/panel_left.png"
- Comic Image Right: "Assets/Arts/Comics/panel_right.png"
```

這樣兩個面板會同時顯示在左右兩側。

