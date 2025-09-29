using System.Runtime.CompilerServices;
using UnityEngine;

public class AutoScaleSprites : MonoBehaviour
{
    [SerializeField] private Sprite backgroundSprite;
    private GameObject backgroundObject;

    private void Start()
    {
        CreateBackground();
        AutoScaleSize();
    }

    [ContextMenu("Auto Scale Size")]
    private void AutoScaleSize()
    {
        // 取得螢幕寬高（世界座標，假設正交攝影機）
        float screenHeight = Camera.main.orthographicSize * 2;
        float screenWidth = screenHeight * Camera.main.aspect;

        Transform background = backgroundObject.transform;
        if (background != null)
        {
            SpriteRenderer sr = background.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // 取得 sprite 原始大小（單位：世界座標）
                Vector2 spriteSize = sr.sprite.bounds.size;

                // 計算要填滿螢幕的 scale
                float bgScaleX = screenWidth / spriteSize.x;
                float bgScaleY = screenHeight / spriteSize.y;

                // 維持比例
                float scale = Mathf.Min(bgScaleX, bgScaleY);

                background.localScale = new Vector3(scale, scale, 1);
            }
        }
    }

    private void CreateBackground()
    {
        if (backgroundObject != null) return;

        backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(transform);
        backgroundObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        SpriteRenderer sr = backgroundObject.AddComponent<SpriteRenderer>();
        sr.sprite = backgroundSprite;
        sr.sortingOrder = -100; // 確保在最底層
    }

    // 用 gizmo 畫出 screen 範圍
    private void OnDrawGizmos()
    {
        if (Camera.main == null) return;

        float screenHeight = Camera.main.orthographicSize * 2;
        float screenWidth = screenHeight * Camera.main.aspect;

        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(Vector3.zero, new Vector3(screenWidth, screenHeight, 0));
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Only run in editor, not at runtime
        if (backgroundSprite != null)
        {
            // If backgroundObject doesn't exist, create it
            if (backgroundObject == null)
            {
                CreateBackground();
            }
            else
            {
                var sr = backgroundObject.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = backgroundSprite;
                }
            }
            AutoScaleSize();
        }
    }
#endif
}