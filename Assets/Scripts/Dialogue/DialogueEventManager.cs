using System;
using System.Linq;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[CreateAssetMenu(fileName = "DialogueEventManager", menuName = "Dialogue/DialogueEventManager")]
public class DialogueEventManager : ScriptableObject
{
    public static DialogueEventManager Instance { get; private set; }
    private static float comicFadeDuration = 0.3f;

    public void Initialize()
    {
        Instance = this;
    }

    public static void OnConversationLine(Subtitle subtitle)
    {
        HandleBackgroundImage(subtitle);
        HandleComicImage(subtitle);
        HandleCharacterImage(subtitle);
    }

    private static void HandleBackgroundImage(Subtitle subtitle)
    {
        Field backgroundField = subtitle.dialogueEntry.fields.Find(f => f.title == "Background Image");
        if (backgroundField != null)
        {
            string backgroundImagePath = backgroundField.value;
            if (!string.IsNullOrEmpty(backgroundImagePath))
            {
                // 使用 Addressables 載入背景圖片
                Addressables.LoadAssetAsync<Sprite>(backgroundImagePath).Completed += (AsyncOperationHandle<Sprite> handle) =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        Sprite backgroundSprite = handle.Result;
                        Debug.Log($"更改背景圖片為: {backgroundImagePath}");

                        // get game object with tag Background
                        // change its sprite to backgroundSprite
                        GameObject backgroundObject = GameObject.FindGameObjectWithTag("Background");
                        if (backgroundObject != null)
                        {
                            SpriteRenderer spriteRenderer = backgroundObject.GetComponent<SpriteRenderer>();
                            if (spriteRenderer != null)
                            {
                                spriteRenderer.sprite = backgroundSprite;
                            }
                            else
                            {
                                Debug.LogWarning("Background 物件沒有 SpriteRenderer 組件");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"找不到背景圖片: {backgroundImagePath}");
                    }
                };
            }
        }
    }

    private static void HandleComicImage(Subtitle subtitle)
    {
        // 處理原有的 Comic Image（向後相容）
        Field comicField = subtitle.dialogueEntry.fields.Find(f => f.title == "Comic Image");
        if (comicField != null)
        {
            string comicImagePath = comicField.value;
            SetComicImage("ComicImage", comicImagePath);
        }

        // 處理 Comic Image Left
        Field comicLeftField = subtitle.dialogueEntry.fields.Find(f => f.title == "Comic Image Left");
        if (comicLeftField != null)
        {
            string comicLeftImagePath = comicLeftField.value;
            SetComicImage("ComicImageLeft", comicLeftImagePath);
        }

        // 處理 Comic Image Right
        Field comicRightField = subtitle.dialogueEntry.fields.Find(f => f.title == "Comic Image Right");
        if (comicRightField != null)
        {
            string comicRightImagePath = comicRightField.value;
            SetComicImage("ComicImageRight", comicRightImagePath);
        }
    }

    /// <summary>
    /// 設置指定名稱的 Comic Image GameObject 的 Sprite
    /// </summary>
    /// <param name="gameObjectName">GameObject 的名稱（例如 "ComicImage", "ComicImageLeft", "ComicImageRight"）</param>
    /// <param name="comicImagePath">Comic Image 的路徑（Addressables 路徑），如果為 null 或空字串則清空 sprite</param>
    /// <param name="skipFade">是否跳過淡入效果，直接顯示</param>
    public static void SetComicImage(string gameObjectName, string comicImagePath, bool skipFade = false)
    {
        Debug.Log($"SetComicImage: 處理 {gameObjectName}，路徑={comicImagePath}");

        GameObject comicObject = GameObject.Find(gameObjectName);
        if (comicObject == null)
        {
            Debug.LogWarning($"找不到 {gameObjectName} 物件");
            return;
        }

        SpriteRenderer spriteRenderer = comicObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning($"{gameObjectName} 物件沒有 SpriteRenderer 組件");
            return;
        }

        // 獲取 ComicImageFader 組件
        ComicImageFader fader = comicObject.GetComponent<ComicImageFader>();

        if (!string.IsNullOrEmpty(comicImagePath))
        {
            // 先嘗試作為完整路徑載入
            Addressables.LoadAssetAsync<Sprite>(comicImagePath).Completed += (AsyncOperationHandle<Sprite> handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    Sprite comicSprite = handle.Result;
                    Debug.Log($"更改 {gameObjectName} 為: {comicImagePath} (跳過淡入: {skipFade})");
                    ApplyComicSprite(spriteRenderer, fader, comicSprite, skipFade);
                }
                else
                {
                    // 如果路徑載入失敗，嘗試通過名稱查找
                    Debug.Log($"路徑載入失敗，嘗試通過名稱查找: {comicImagePath}");
                    LoadSpriteByName(comicImagePath, (sprite) =>
                    {
                        if (sprite != null)
                        {
                            Debug.Log($"通過名稱找到 Sprite: {comicImagePath} (跳過淡入: {skipFade})");
                            ApplyComicSprite(spriteRenderer, fader, sprite, skipFade);
                        }
                        else
                        {
                            Debug.LogWarning($"找不到漫畫圖片: {comicImagePath}（已嘗試路徑和名稱查找）");
                        }
                    });
                }
            };
        }
        else
        {
            // 當 Comic Image field 為 null 或 empty 時，清空 sprite
            Debug.Log($"清空 {gameObjectName} (跳過淡出: {skipFade})");
            if (skipFade && fader != null)
            {
                fader.SetSpriteDirectly(null);
            }
            else
            {
                spriteRenderer.sprite = null;
            }
        }
    }

    private static void HandleCharacterImage(Subtitle subtitle)
    {
        Field characterField = subtitle.dialogueEntry.fields.Find(f => f.title == "Character Image");
        if (characterField == null)
        {
            Debug.Log("HandleCharacterImage: 找不到 'Character Image' 欄位");
            return;
        }

        Debug.Log($"HandleCharacterImage: 找到欄位，類型={characterField.type}, 值={characterField.value}, 值類型={characterField.value?.GetType()}");

        GameObject characterObject = GameObject.Find("CharacterImage");
        if (characterObject == null)
        {
            Debug.LogWarning("找不到 Character Image 物件");
            return;
        }

        SpriteRenderer spriteRenderer = characterObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("Character Image 物件沒有 SpriteRenderer 組件");
            return;
        }

        // 嘗試取得 Sprite（支援多種方式）
        Sprite characterSprite = null;
        string characterImagePath = null;

        object fieldValue = characterField.value;

        // 方式 1: 如果欄位值是 Sprite 物件
        if (fieldValue is Sprite spriteValue)
        {
            characterSprite = spriteValue;
        }
        // 方式 2: 如果欄位值是字串路徑
        else if (fieldValue is string stringValue)
        {
            characterImagePath = stringValue;
        }
        // 方式 3: 嘗試轉換為字串
        else if (fieldValue != null)
        {
            characterImagePath = fieldValue.ToString();
        }

        // 如果直接取得 Sprite，直接使用
        if (characterSprite != null)
        {
            Debug.Log($"更改角色圖片為: {characterSprite.name}");
            spriteRenderer.sprite = characterSprite;
            return;
        }

        // 如果有路徑，使用 Addressables 載入
        if (!string.IsNullOrEmpty(characterImagePath))
        {
            Addressables.LoadAssetAsync<Sprite>(characterImagePath).Completed += (AsyncOperationHandle<Sprite> handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    Sprite loadedSprite = handle.Result;
                    Debug.Log($"更改角色圖片為: {characterImagePath}");
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sprite = loadedSprite;
                    }
                }
                else
                {
                    Debug.LogWarning($"找不到角色圖片: {characterImagePath}");
                }
            };
        }
        else
        {
            // 當 Character Image field 為 null 或 empty 時，清空 sprite（角色退場）
            Debug.Log("清空角色圖片");
            spriteRenderer.sprite = null;
        }
    }

    /// <summary>
    /// 應用 Comic Sprite 到 SpriteRenderer
    /// </summary>
    private static void ApplyComicSprite(SpriteRenderer spriteRenderer, ComicImageFader fader, Sprite sprite, bool skipFade)
    {
        if (spriteRenderer != null)
        {
            if (skipFade && fader != null)
            {
                // 使用直接設置方法跳過淡入
                fader.SetSpriteDirectly(sprite);
            }
            else
            {
                // 正常設置，會觸發淡入效果
                spriteRenderer.sprite = sprite;
            }
        }
    }

    /// <summary>
    /// 通過 Sprite 名稱查找並載入 Sprite
    /// </summary>
    private static void LoadSpriteByName(string spriteName, System.Action<Sprite> onComplete)
    {
        // 使用 Addressables 查找所有 Sprite 資源
        var locationsHandle = Addressables.LoadResourceLocationsAsync("t:Sprite");
        locationsHandle.Completed += (AsyncOperationHandle<System.Collections.Generic.IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation>> handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null && handle.Result.Count > 0)
            {
                // 遍歷所有位置，查找名稱匹配的 Sprite
                int totalLocations = handle.Result.Count;
                int completedCount = 0;
                Sprite foundSprite = null;

                foreach (var location in handle.Result)
                {
                    // 載入 Sprite 並檢查名稱
                    Addressables.LoadAssetAsync<Sprite>(location).Completed += (AsyncOperationHandle<Sprite> spriteHandle) =>
                    {
                        completedCount++;
                        
                        if (spriteHandle.Status == AsyncOperationStatus.Succeeded && foundSprite == null)
                        {
                            Sprite sprite = spriteHandle.Result;
                            if (sprite != null && sprite.name == spriteName)
                            {
                                foundSprite = sprite;
                                onComplete?.Invoke(sprite);
                                return;
                            }
                        }
                        
                        // 如果所有資源都檢查完畢且沒找到，嘗試使用 DialogueManager.LoadAsset
                        if (completedCount >= totalLocations && foundSprite == null)
                        {
                            TryLoadSpriteByDialogueManager(spriteName, onComplete);
                        }
                    };
                }
            }
            else
            {
                // 如果 Addressables 查找失敗，嘗試使用 DialogueManager.LoadAsset
                TryLoadSpriteByDialogueManager(spriteName, onComplete);
            }
        };
    }

    /// <summary>
    /// 嘗試使用 DialogueManager 載入 Sprite
    /// </summary>
    private static void TryLoadSpriteByDialogueManager(string spriteName, System.Action<Sprite> onComplete)
    {
        if (DialogueManager.instance != null)
        {
            DialogueManager.LoadAsset(spriteName, typeof(Sprite), (asset) =>
            {
                if (asset is Sprite sprite)
                {
                    onComplete?.Invoke(sprite);
                }
                else
                {
                    onComplete?.Invoke(null);
                }
            });
        }
        else
        {
            onComplete?.Invoke(null);
        }
    }

    public static void OnConversationResponseMenu(Response[] responses)
    {
        // get entry title
        String entryTitle = responses.First<Response>().destinationEntry.Title;
        Debug.Log($"OnConversationResponseMenu: {entryTitle}");
        // find the component "Response Menu Title"
        GameObject responseMenuTitleObject = GameObject.Find("Response Menu Title");
        if (responseMenuTitleObject != null)
        {
            Debug.Log("找到 Response Menu Title 物件");

        }
    }
}