using System.Runtime.CompilerServices;
using System.Collections;
using UnityEngine;

public class AutoScaleSprites : MonoBehaviour
{
    [SerializeField] private Sprite backgroundSprite;
    private GameObject backgroundObject;

    private void Start()
    {
        SetBackgroundObject();
        StartCoroutine(WaitForSpriteAndScale());
    }

    private IEnumerator WaitForSpriteAndScale()
    {
        if (backgroundObject == null) yield break;

        SpriteRenderer sr = backgroundObject.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        // 等待 sprite 被設定
        while (sr.sprite == null)
        {
            yield return null;
        }

        AutoScaleSize();
    }

    [ContextMenu("Auto Scale Size")]
    private void AutoScaleSize()
    {
        if (!backgroundObject) return;

        // 取得螢幕寬高（世界座標，假設正交攝影機）
        float screenHeight = Camera.main.orthographicSize * 2;
        float screenWidth = screenHeight * Camera.main.aspect;

        Transform background = backgroundObject.transform;
        if (background != null)
        {
            SpriteRenderer sr = background.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
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

    private void SetBackgroundObject()
    {
        backgroundObject = GameObject.FindGameObjectWithTag("Background");
        if (backgroundObject == null)
        {
            Debug.LogWarning("找不到標籤為 'Background' 的物件");
        }
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
}