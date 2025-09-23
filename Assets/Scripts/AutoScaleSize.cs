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

        float baseWidth = 1704f;
        float baseHeight = 786f;
        float baseRatio = baseWidth / baseHeight;
        float screenRatio = screenWidth / screenHeight;

        // 根據比例決定縮放方式
        float scale;
        if (screenRatio < baseRatio)
        {
            // 螢幕比較寬，依高度縮放（留左右黑邊）
            scale = screenHeight / baseHeight * 100;
        }
        else
        {
            // 螢幕比較高，依寬度縮放（留上下黑邊）
            scale = screenWidth / baseWidth * 100;
        }

        // 設定 container 的 scale
        transform.localScale = new Vector3(scale, scale, 1);
    }
}