using System;
using System.Linq;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueEventManager", menuName = "Dialogue/DialogueEventManager")]
public class DialogueEventManager : ScriptableObject
{
    [SerializeField] GameObject ResponseMenuTitle;
    public static DialogueEventManager Instance { get; private set; }

    public void Initialize()
    {
        Instance = this;
    }

    public static void OnConversationLine(Subtitle subtitle)
    {
        // 處理 Background Image
        Field backgroundField = subtitle.dialogueEntry.fields.Find(f => f.title == "Background Image");
        if (backgroundField != null)
        {
            string backgroundImagePath = backgroundField.value;
            if (!string.IsNullOrEmpty(backgroundImagePath))
            {
                // 假設你有一個方法可以根據名稱獲取背景圖片
                Sprite backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(backgroundImagePath);
                if (backgroundSprite != null)
                {
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
                    else
                    {
                        Debug.LogWarning($"找不到背景圖片: {backgroundImagePath}");
                    }
                }
            }
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