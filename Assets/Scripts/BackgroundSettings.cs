using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BackgroundSettings", menuName = "Game/Background Settings")]
public class BackgroundSettings : ScriptableObject
{
    [SerializeField] private List<BackgroundSprite> backgroundSprites;

    private Dictionary<string, Sprite> spriteDictionary;

    private void OnEnable()
    {
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        spriteDictionary = new Dictionary<string, Sprite>();
        if (backgroundSprites != null)
        {
            foreach (var bgSprite in backgroundSprites)
            {
                if (!string.IsNullOrEmpty(bgSprite.spriteName) && bgSprite.sprite != null)
                {
                    spriteDictionary[bgSprite.spriteName] = bgSprite.sprite;
                }
            }
        }
    }

    public void ChangeBackground(GameObject backgroundObject, string spriteName)
    {
        if (backgroundObject == null)
        {
            Debug.LogWarning("Background GameObject is null!");
            return;
        }

        if (backgroundObject.TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRenderer))
        {
            Sprite sprite = GetSprite(spriteName);
            if (sprite != null)
            {
                spriteRenderer.sprite = sprite;
                Debug.Log($"Background changed to: {spriteName}");
            }
            else
            {
                Debug.LogWarning($"Background sprite '{spriteName}' not found!");
            }
        }
        else
        {
            Debug.LogWarning("Background GameObject doesn't have a SpriteRenderer!");
        }
    }

    public void ChangeBackgroundByTag(string spriteName)
    {
        GameObject backgroundObject = GameObject.FindWithTag("Background");
        if (backgroundObject != null)
        {
            ChangeBackground(backgroundObject, spriteName);
        }
        else
        {
            Debug.LogWarning("No GameObject with 'Background' tag found!");
        }
    }

    public Sprite GetSprite(string spriteName)
    {
        if (spriteDictionary == null || spriteDictionary.Count == 0)
        {
            InitializeDictionary();
        }

        if (spriteDictionary.TryGetValue(spriteName, out Sprite sprite))
        {
            return sprite;
        }
        return null;
    }

    public bool HasSprite(string spriteName)
    {
        if (spriteDictionary == null || spriteDictionary.Count == 0)
        {
            InitializeDictionary();
        }

        return spriteDictionary.ContainsKey(spriteName);
    }

    public List<string> GetAllSpriteNames()
    {
        if (spriteDictionary == null || spriteDictionary.Count == 0)
        {
            InitializeDictionary();
        }

        return new List<string>(spriteDictionary.Keys);
    }
}