using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [SerializeField] private BackgroundSettings backgroundSettings;

    public static BackgroundManager Instance { get; private set; }
    void Start()
    {
        // 確保只有一個 GameManager 存在
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 切換場景時不會被銷毀
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Public method to change background sprite by name
    public void ChangeBackgroundSceneSprite(string spriteName)
    {
        if (backgroundSettings != null)
        {
            backgroundSettings.ChangeBackgroundByTag(spriteName);
        }
        else
        {
            Debug.LogError("Background Settings ScriptableObject is not assigned!");
        }
    }
}
