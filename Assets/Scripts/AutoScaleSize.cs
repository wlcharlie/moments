using UnityEngine;

public class AutoScaleSprites : MonoBehaviour
{
    void Start()
    {
        AutoScaleSize();
    }

    [ContextMenu("Auto Scale Size")]
    void AutoScaleSize()
    {
        // 取得螢幕寬高（世界座標，假設正交攝影機）
        float screenHeight = Camera.main.orthographicSize * 2;
        float screenWidth = screenHeight * Camera.main.aspect;

        Transform background = transform.Find("Background");
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

                Debug.Log($"Screen: {screenWidth}x{screenHeight}, Sprite: {spriteSize.x}x{spriteSize.y}, Scale: {bgScaleX}, {bgScaleY}");

                background.localScale = new Vector3(scale, scale, 1);
            }
        }
    }

    // 用 gizmo 畫出 screen 範圍
    void OnDrawGizmos()
    {
        if (Camera.main == null) return;

        float screenHeight = Camera.main.orthographicSize * 2;
        float screenWidth = screenHeight * Camera.main.aspect;

        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(Vector3.zero, new Vector3(screenWidth, screenHeight, 0));
    }
}