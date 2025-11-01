using UnityEngine;

public class TiledBackground : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Transform transform = GetComponent<Transform>();

        // 取得螢幕寬高（世界座標，假設正交攝影機）
        float screenHeight = Camera.main.orthographicSize * 2;
        float screenWidth = screenHeight * Camera.main.aspect;

        Debug.Log($"Screen Width: {screenWidth}, Screen Height: {screenHeight}");

        // 取得 sprite 原始大小（單位：世界座標）
        Vector2 spriteSize = sr.sprite.bounds.size;

        // 計算要填滿螢幕的 scale
        float bgScaleX = screenWidth / spriteSize.x;
        float bgScaleY = screenHeight / spriteSize.y;

        // 維持比例
        float scale = Mathf.Min(bgScaleX, bgScaleY);


        transform.localScale = new Vector3(scale, scale, 1);
        sr.size = new Vector2(screenWidth / scale, screenHeight / scale);
    }
}