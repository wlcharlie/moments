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
        Field comicField = subtitle.dialogueEntry.fields.Find(f => f.title == "Comic Image");
        string comicImagePath = comicField?.value;

        GameObject comicObject = GameObject.Find("ComicImage");
        if (comicObject != null)
        {
            SpriteRenderer spriteRenderer = comicObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                if (!string.IsNullOrEmpty(comicImagePath))
                {
                    // 使用 Addressables 載入並設置新的 Comic Image
                    Addressables.LoadAssetAsync<Sprite>(comicImagePath).Completed += (AsyncOperationHandle<Sprite> handle) =>
                    {
                        if (handle.Status == AsyncOperationStatus.Succeeded)
                        {
                            Sprite comicSprite = handle.Result;
                            Debug.Log($"更改漫畫圖片為: {comicImagePath}");
                            if (spriteRenderer != null)
                            {
                                spriteRenderer.sprite = comicSprite;
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"找不到漫畫圖片: {comicImagePath}");
                        }
                    };
                }
                else
                {
                    // 當 Comic Image field 為 null 或 empty 時，清空 sprite
                    Debug.Log("清空漫畫圖片");
                    spriteRenderer.sprite = null;
                }
            }
            else
            {
                Debug.LogWarning("Comic Image 物件沒有 SpriteRenderer 組件");
            }
        }
        else
        {
            Debug.LogWarning("找不到 Comic Image 物件");
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