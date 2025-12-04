using UnityEngine;

[ExecuteInEditMode]
public class CameraFitInside : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float designWidth = 786f;
    [SerializeField] private float designHeight = 1704f;
    [SerializeField] private float pixelsPerUnit = 100f;

    public static event System.Action<float, float> OnCameraAdjusted;
    public static float LastLetterboxWidth { get; private set; }
    public static float LastLetterboxHeight { get; private set; }

    private float targetWidth => designWidth / pixelsPerUnit;
    private float targetHeight => designHeight / pixelsPerUnit;

    private int lastWidth;
    private int lastHeight;

    void OnEnable()
    {
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();
        if (targetCamera == null)
            targetCamera = Camera.main;

        FitCamera();
    }

    void Update()
    {
        // 只在解析度改變時更新
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            FitCamera();
        }
    }

    void FitCamera()
    {
        if (targetCamera == null) return;

        float screenAspect = (float)Screen.width / Screen.height;
        float targetAspect = targetWidth / targetHeight;

        float orthoSize;
        float letterboxWidth = 0f;  // 左右黑邊
        float letterboxHeight = 0f; // 上下黑邊

        if (screenAspect >= targetAspect)
        {
            orthoSize = targetHeight / 2f;
            float cameraWidth = orthoSize * 2f * screenAspect;
            letterboxWidth = (cameraWidth - targetWidth) / 2f;
        }
        else
        {
            orthoSize = targetWidth / screenAspect / 2f;
            float cameraHeight = orthoSize * 2f;
            letterboxHeight = (cameraHeight - targetHeight) / 2f;
        }

        targetCamera.orthographicSize = orthoSize;
        LastLetterboxWidth = letterboxWidth;
        LastLetterboxHeight = letterboxHeight;
        OnCameraAdjusted?.Invoke(letterboxWidth, letterboxHeight);
    }

    void OnDrawGizmos()
    {
        Vector3 center = transform.position;
        float halfWidth = targetWidth / 2f;
        float halfHeight = targetHeight / 2f;

        // Draw design area rectangle
        Gizmos.color = Color.green;
        Vector3 topLeft = center + new Vector3(-halfWidth, halfHeight, 0);
        Vector3 topRight = center + new Vector3(halfWidth, halfHeight, 0);
        Vector3 bottomLeft = center + new Vector3(-halfWidth, -halfHeight, 0);
        Vector3 bottomRight = center + new Vector3(halfWidth, -halfHeight, 0);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}