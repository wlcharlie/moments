# ComicImageLeft 和 ComicImageRight 快速設置指南

## 🎯 最簡單的方法：複製現有的 ComicImage

### 步驟 1：複製 ComicImage
1. 在 Unity Hierarchy 中選擇 `ComicImage` GameObject
2. 按 `Ctrl+D`（Windows）或 `Cmd+D`（Mac）複製
3. 複製兩次，得到兩個副本

### 步驟 2：重命名
- 第一個副本 → 重命名為 `ComicImageLeft`
- 第二個副本 → 重命名為 `ComicImageRight`

### 步驟 3：調整位置

**ComicImageLeft：**
- 選擇 `ComicImageLeft`
- 在 Inspector 的 Transform 組件中設置 Position：
  - **X**: `-3`（或根據你的需求調整，負值表示左側）
  - **Y**: `2`（保持與原來的相同，或根據需求調整）
  - **Z**: `0`

**ComicImageRight：**
- 選擇 `ComicImageRight`
- 在 Inspector 的 Transform 組件中設置 Position：
  - **X**: `3`（或根據你的需求調整，正值表示右側）
  - **Y**: `2`（保持與原來的相同，或根據需求調整）
  - **Z**: `0`

### 步驟 4：檢查組件

複製後的 GameObject 應該已經有：
- ✅ **Transform** 組件（已調整位置）
- ✅ **Sprite Renderer** 組件
- ✅ **ComicImageFader** 組件

**檢查 ComicImageFader：**
- 點擊 `ComicImageLeft` 或 `ComicImageRight`
- 在 Inspector 中檢查 `ComicImageFader` 組件
- `Sprite Renderer` 欄位應該已經自動連結（或顯示為 "None"，如果顯示為 None，拖拽該 GameObject 的 Sprite Renderer 組件到欄位中）

### 完成！✅

現在你已經有三個 Comic Image 位置：
- `ComicImage` - 原有位置（x: 0, y: 2, z: 0）
- `ComicImageLeft` - 左側（x: -3, y: 2, z: 0）
- `ComicImageRight` - 右側（x: 3, y: 2, z: 0）

## 📝 在對話系統中使用

在 Dialogue Database 的對話條目中，可以添加以下欄位：

- **Comic Image** - 對應到 `ComicImage`（原有）
- **Comic Image Left** - 對應到 `ComicImageLeft`（新增）
- **Comic Image Right** - 對應到 `ComicImageRight`（新增）

欄位值填入 Addressables 路徑，例如：
```
Assets/Arts/Comics/panel_left.png
```

如果欄位值為空，對應的圖片會自動淡出。

## 💡 位置調整建議

根據你的遊戲設計，可以調整 X 值：
- **更左側**：X = -4, -5, -6 等
- **更右側**：X = 4, 5, 6 等
- **更接近中心**：X = -1.5, 1.5 等

Y 值可以：
- 保持相同：`y: 2`（三個位置在同一水平線）
- 錯開高度：例如 Left = `y: 2.5`，Right = `y: 1.5`（視覺效果更豐富）

## ⚠️ 重要提醒

1. **名稱必須完全匹配**（區分大小寫）：
   - ✅ `ComicImageLeft`
   - ✅ `ComicImageRight`
   - ❌ `comicimageleft`（錯誤）
   - ❌ `Comic Image Left`（錯誤，不能有空格）

2. **ComicImageFader 的 Sprite Renderer 連結**：
   - 如果 Inspector 中顯示為 "None (Sprite Renderer)"，需要手動拖拽該 GameObject 的 Sprite Renderer 組件到欄位中
   - 或者留空也可以（腳本會自動獲取）

