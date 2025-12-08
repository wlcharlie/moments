# Sequencer Commands 指令參考

本文檔記錄了專案中所有可用的 Sequencer Commands 及其參數說明。

---

## 目錄

- [場景管理](#場景管理)
- [視覺效果](#視覺效果)
- [音效](#音效)
- [狀態管理](#狀態管理)
- [資料儲存](#資料儲存)
- [相機效果](#相機效果)

---

## 場景管理

### SwitchScene

切換場景並可選擇轉場效果。

**語法：**

```
SwitchScene(sceneName[, loadSceneMode][, transitionType])
```

**參數：**

| 參數             | 類型   | 必需 | 預設值     | 說明                                                                         |
| ---------------- | ------ | ---- | ---------- | ---------------------------------------------------------------------------- |
| `sceneName`      | string | ✅   | -          | 要載入的場景名稱                                                             |
| `loadSceneMode`  | string | ❌   | `"Single"` | 載入模式：`"Single"`（替換當前場景）或 `"Additive"`（疊加場景）              |
| `transitionType` | string | ❌   | `"Cover"`  | 轉場類型：`"Cover"`、`"Splash"`、`"LoadingScreen"` 或 `"None"`（不使用轉場） |

**轉場類型說明：**

- `"Cover"`：滑動遮罩效果
- `"Splash"`：白色閃光效果
- `"LoadingScreen"`：載入畫面（含進度條）
- `"None"`：不使用轉場，直接載入場景

**範例：**

```
SwitchScene(CH01_SC03_BeigoSneakDoor)
SwitchScene(CH01_SC03_BeigoSneakDoor, Single, LoadingScreen)
SwitchScene(CH01_SC03_BeigoSneakDoor, Single, Splash)
SwitchScene(CH01_SC03_BeigoSneakDoor, Single, Cover)
SwitchScene(CH01_SC03_BeigoSneakDoor, Single, None)
SwitchScene(Playground, Additive)
```

---

## 視覺效果

### ShowSceneDetail

讓現有背景圖放大並移動到指定位置。

**語法：**

```
ShowSceneDetail(zoomScale, duration, reset, [position])
```

**參數：**

| 參數        | 類型   | 必需 | 預設值          | 說明                                                                     |
| ----------- | ------ | ---- | --------------- | ------------------------------------------------------------------------ |
| `zoomScale` | float  | ✅   | -               | 放大倍數（例如：2.5 表示放大 2.5 倍）                                    |
| `duration`  | float  | ✅   | -               | 動畫時間（秒）                                                           |
| `reset`     | bool   | ✅   | -               | 是否自動恢復原狀（`true`/`false`）                                       |
| `position`  | string | ❌   | `"bottom-left"` | 目標位置：`"bottom-left"`（左下角）或 `"left-center-up"`（左邊中間偏上） |

**位置選項說明：**

- `"bottom-left"`：左下角位置（預設）
- `"left-center-up"`：左邊中間偏上（可視範圍落在圖片中上部分）

**範例：**

```
ShowSceneDetail(2.5, 0.8, false)
ShowSceneDetail(2.5, 0.8, false, left-center-up)
ShowSceneDetail(2.5, 0.8, true)
ShowSceneDetail(2, 1.0, false, bottom-left)
```

---

### RestoreSceneDetail

恢復背景圖到原始狀態（正常大小和位置）。

**語法：**

```
RestoreSceneDetail([duration])
```

**參數：**

| 參數       | 類型  | 必需 | 預設值 | 說明           |
| ---------- | ----- | ---- | ------ | -------------- |
| `duration` | float | ❌   | `0.8`  | 動畫時間（秒） |

**範例：**

```
RestoreSceneDetail(0.8)
RestoreSceneDetail()
RestoreSceneDetail(1.0)
```

**注意：**

- 此命令會恢復由 `ShowSceneDetail` 修改的背景圖狀態
- 如果沒有保存的原始狀態，會使用當前狀態作為原始狀態

---

### SetBackgroundColor

改變背景圖的顏色覆蓋層。

**語法：**

```
SetBackgroundColor(#RRGGBB)
SetBackgroundColor(#RRGGBBAA)
```

**參數：**

| 參數       | 類型   | 必需 | 預設值 | 說明                                                       |
| ---------- | ------ | ---- | ------ | ---------------------------------------------------------- |
| `hexColor` | string | ✅   | -      | 十六進位顏色碼，格式：`#RRGGBB` 或 `#RRGGBBAA`（含透明度） |

**範例：**

```
SetBackgroundColor(#00000012)
SetBackgroundColor(#FF0000)
SetBackgroundColor(#000000)
SetBackgroundColor(#FFFFFF80)
```

**注意：**

- 會修改標籤為 `"Background"` 的 GameObject 上的 `SpriteRenderer.color`
- 如果顏色碼沒有 `#` 前綴，會自動添加
- 支援透明度（Alpha 通道）

---

### SetComicImage

設置 Comic Image 並可選擇跳過淡入效果。

**語法：**

```
SetComicImage(position, imagePath[, skipFade])
```

**參數：**

| 參數        | 類型   | 必需 | 預設值 | 說明                                                                                    |
| ----------- | ------ | ---- | ------ | --------------------------------------------------------------------------------------- |
| `position`  | string | ✅   | -      | 位置：`"left"`（左側）、`"right"`（右側）、`"center"`（中央），或直接提供 GameObject 名稱 |
| `imagePath` | string | ✅   | -      | Sprite 名稱（例如 `"CH01_SC04_Dog_Run_0"`）或 Addressables 路徑，空字串表示清空圖片      |
| `skipFade`  | bool   | ❌   | `true` | 是否跳過淡入效果（`true`/`false`）                                                       |

**位置選項說明：**

- `"left"`：對應到 `ComicImageLeft` GameObject
- `"right"`：對應到 `ComicImageRight` GameObject
- `"center"` 或 `"centre"`：對應到 `ComicImage` GameObject
- 也可以直接提供 GameObject 名稱，例如 `"ComicImageLeft"`

**範例：**

```
// 使用 Sprite 名稱（推薦，不需要輸入完整路徑）
SetComicImage(left, CH01_SC04_Dog_Run_0)
SetComicImage(right, MRT_Door_Open_03_0)
SetComicImage(center, panel_center)

// 也可以使用完整 Addressables 路徑
SetComicImage(left, Assets/Arts/Comics/panel_left.png)
SetComicImage(right, Assets/Arts/Comics/panel_right.png)

// 清空圖片
SetComicImage(left, )  // 直接清空，不淡出
SetComicImage(left, , false)  // 清空並使用淡出效果
```

**注意：**

- 此命令預設跳過淡入效果（`skipFade` 預設為 `true`），適合需要連續顯示圖片的情況
- `imagePath` 參數支援兩種格式：
  - **Sprite 名稱**（推薦）：直接使用 Sprite 的名稱，例如 `"CH01_SC04_Dog_Run_0"`，系統會自動查找對應的 Sprite
  - **完整路徑**：使用 Addressables 完整路徑，例如 `"Assets/Arts/Comics/panel_left.png"`
- 如果 `imagePath` 為空字串，會清空對應位置的圖片
- 如果 `skipFade` 設為 `false`，會使用正常的淡入淡出效果

---

### SkipComicFade

跳過 Comic Image 的淡入/淡出效果。當圖片已經在對話欄位中設置好時，可以使用此命令來控制是否跳過淡入或淡出效果。

**語法：**

```
SkipComicFade(position[, fadeType])
```

**參數：**

| 參數       | 類型   | 必需 | 預設值  | 說明                                                                                    |
| ---------- | ------ | ---- | ------- | --------------------------------------------------------------------------------------- |
| `position` | string | ✅   | -       | 位置：`"left"`（左側）、`"right"`（右側）、`"center"`（中央），或直接提供 GameObject 名稱 |
| `fadeType` | string | ❌   | `"both"` | 淡入淡出類型：`"in"`（只跳過淡入）、`"out"`（只跳過淡出）、`"both"`（跳過淡入和淡出）      |

**使用場景：**

當你在對話欄位中已經設置了 Comic Image（例如 "Comic Image Left"），但想要在 sequencer 中控制跳過淡入或淡出效果時使用。

**範例：**

```
// 跳過下一次淡入和淡出（預設）
SkipComicFade(left)

// 只跳過淡入效果（圖片顯示時直接出現，不淡入）
SkipComicFade(left, in)

// 只跳過淡出效果（圖片清空時直接消失，不淡出）
SkipComicFade(left, out)

// 跳過淡入和淡出
SkipComicFade(right, both)
```

**注意：**

- 此命令只會影響**下一次** sprite 變更的淡入淡出效果
- 如果圖片已經在對話欄位中設置好，在對話條目的 Sequence 中使用此命令即可
- 使用 `SkipComicFade(left, in)` 後，當對話條目顯示時，圖片會直接出現而不淡入
- 使用 `SkipComicFade(left, out)` 後，當圖片被清空時，會直接消失而不淡出

---

## 音效

### FMODPlay

播放 FMOD 音效事件。

**語法：**

```
FMODPlay(eventName)
```

**參數：**

| 參數        | 類型   | 必需 | 預設值 | 說明                                     |
| ----------- | ------ | ---- | ------ | ---------------------------------------- |
| `eventName` | string | ✅   | -      | FMOD 事件名稱（不需要 `"event:/"` 前綴） |

**範例：**

```
FMODPlay(obj_take_photo)
FMODPlay(ui_button_click)
FMODPlay(ambient_rain)
```

**注意：**

- 命令會自動在事件名稱前添加 `"event:/"` 前綴
- 使用 `FMODUnity.RuntimeManager.PlayOneShot()` 播放一次性音效

---

## 狀態管理

### UpdateStatus

更新角色狀態值（愛心、金錢、體力）。

**語法：**

```
UpdateStatus(statusType, amount)
```

**參數：**

| 參數         | 類型   | 必需 | 預設值 | 說明                                         |
| ------------ | ------ | ---- | ------ | -------------------------------------------- |
| `statusType` | string | ✅   | -      | 狀態類型：`"Heart"`、`"Money"` 或 `"Energy"` |
| `amount`     | int    | ✅   | -      | 變化數值（可以是正數或負數）                 |

**狀態類型說明：**

- `"Heart"`：愛心值
- `"Money"`：金錢值
- `"Energy"`：體力值

**範例：**

```
UpdateStatus(Heart, 10)
UpdateStatus(Heart, -5)
UpdateStatus(Money, 100)
UpdateStatus(Energy, -20)
```

**注意：**

- 數值會被限制在 0-100 範圍內
- 會觸發對應的狀態變化事件，顯示狀態提示

---

## 資料儲存

### SaveData

儲存資料到 JSON 檔案。

**語法：**

```
SaveData(key, value, [filepath])
```

**參數：**

| 參數       | 類型   | 必需 | 預設值       | 說明                            |
| ---------- | ------ | ---- | ------------ | ------------------------------- |
| `key`      | string | ✅   | -            | 資料鍵值名稱                    |
| `value`    | string | ✅   | -            | 資料值（字串、數字或布林值）    |
| `filepath` | string | ❌   | `"dialogue"` | 檔案路徑（不含 `.json` 副檔名） |

**範例：**

```
SaveData(playerName, 小明)
SaveData(visitedScene1, true)
SaveData(chapter, 3)
SaveData(playerName, 小明, player)
SaveData(score, 100, gameData)
```

**注意：**

- 預設儲存到 `dialogue.json`
- 多次呼叫會累積資料，不會覆蓋整個檔案
- 如果 key 已存在，會更新該值；否則會新增

---

## 相機效果

### CameraShake

觸發相機晃動效果。

**語法：**

```
CameraShake([duration], [intensity])
```

**參數：**

| 參數        | 類型  | 必需 | 預設值 | 說明               |
| ----------- | ----- | ---- | ------ | ------------------ |
| `duration`  | float | ❌   | `0.3`  | 晃動持續時間（秒） |
| `intensity` | float | ❌   | `0.2`  | 晃動強度           |

**強度參考：**

- `0.1`：輕微（輕輕碰撞）
- `0.2`：中等（踩到東西）
- `0.3-0.5`：劇烈（跌倒、爆炸）
- `0.8+`：超劇烈（地震）

**範例：**

```
CameraShake(0.3, 0.2)
CameraShake(0.5, 0.5)
CameraShake(1.0, 0.8)
CameraShake()
```

**注意：**

- 如果場景中沒有 `CameraShakeController`，會自動創建一個
- 命令會等待晃動完成後才結束

---

### CharacterShake

控制角色圖片晃動效果。

**語法：**

```
CharacterShake(mode, [intensity], [frequency], [duration])
```

**參數：**

| 參數        | 類型   | 必需 | 預設值 | 說明                                                         |
| ----------- | ------ | ---- | ------ | ------------------------------------------------------------ |
| `mode`      | string | ✅   | -      | 模式：`"start"`（持續）、`"once"`（單次）或 `"stop"`（停止） |
| `intensity` | float  | ❌   | `0.1`  | 晃動強度                                                     |
| `frequency` | float  | ❌   | `2.0`  | 晃動頻率（Hz）                                               |
| `duration`  | float  | ❌   | `0.5`  | 單次晃動持續時間（秒，僅用於 `once` 模式）                   |

**模式說明：**

- `"start"` 或 `"continuous"`：開始持續晃動
- `"once"` 或 `"single"`：執行單次晃動
- `"stop"`：停止晃動

**範例：**

```
CharacterShake(start, 0.1, 2, 0)
CharacterShake(once, 0.15, 3, 0.5)
CharacterShake(stop)
CharacterShake(continuous, 0.2, 2.5, 0)
```

**注意：**

- 需要場景中有名為 `"CharacterImage"` 的 GameObject，且該物件需要有 `CharacterImageShake` 組件

---

## 使用提示

### 參數格式

- **字串參數**：不需要引號，除非包含空格或特殊字元
- **數值參數**：直接寫數字，例如 `2.5`、`10`、`-5`
- **布林參數**：使用 `true` 或 `false`（不區分大小寫）
- **可選參數**：使用 `[]` 標記，可以省略

### 命令組合

可以在同一個 Sequence 中組合多個命令，使用分號 `;` 分隔：

```
ShowSceneDetail(2.5, 0.8, false, left-center-up); CameraShake(0.3, 0.2); FMODPlay(obj_take_photo)
```

### 時間控制

某些命令支援使用 `@時間` 語法來延遲執行：

```
ShowSceneDetail(2.5, 0.8, false)@1.0
CameraShake(0.5, 0.3)@2.5
```

---

## 更新記錄

- **2024-XX-XX**：新增 `SkipComicFade` 命令，支援跳過 Comic Image 的淡入淡出效果
- **2024-XX-XX**：新增 `SetComicImage` 命令，支援跳過淡入效果
- **2024-XX-XX**：新增 `SwitchScene` 轉場類型參數支援
- **2024-XX-XX**：新增 `ShowSceneDetail` 的 `left-center-up` 位置選項
- **2024-XX-XX**：建立初始文件

---

## 相關資源

- [Pixel Crushers Dialogue System 官方文檔](https://www.pixelcrushers.com/dialogue-system/manual/)
- [Unity Timeline 文檔](https://docs.unity3d.com/Manual/TimelineSection.html)
- [FMOD Unity 整合文檔](https://fmod.com/docs/2.02/unity/)
