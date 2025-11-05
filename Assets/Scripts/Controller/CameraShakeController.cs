using UnityEngine;
using System.Collections;

/// <summary>
/// 攝影機晃動控制器
/// 使用方式：CameraShakeController.Instance.Shake(duration, intensity);
/// </summary>
public class CameraShakeController : MonoBehaviour
{
    public static CameraShakeController Instance { get; private set; }

    private Camera mainCamera;
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    void Awake()
    {
        // Singleton 模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            originalPosition = mainCamera.transform.localPosition;
        }
    }

    void Start()
    {
        // 確保有主攝影機
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                originalPosition = mainCamera.transform.localPosition;
            }
            else
            {
                Debug.LogWarning("[CameraShakeController] 找不到主攝影機");
            }
        }
    }

    /// <summary>
    /// 觸發攝影機晃動
    /// </summary>
    /// <param name="duration">晃動持續時間（秒）</param>
    /// <param name="intensity">晃動強度（0.1 = 輕微, 0.3 = 中等, 0.5+ = 劇烈）</param>
    public void Shake(float duration = 0.3f, float intensity = 0.2f)
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("[CameraShakeController] 無法晃動：主攝影機未設定");
            return;
        }

        // 如果已經在晃動，停止舊的晃動
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, intensity));
    }

    private IEnumerator ShakeCoroutine(float duration, float intensity)
    {
        float elapsed = 0f;
        Vector3 startPosition = mainCamera.transform.localPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 計算衰減（隨時間減弱晃動）
            float progress = elapsed / duration;
            float damper = 1f - Mathf.Pow(progress, 2); // 平方衰減，讓晃動逐漸減弱

            // 隨機偏移
            float x = Random.Range(-1f, 1f) * intensity * damper;
            float y = Random.Range(-1f, 1f) * intensity * damper;

            mainCamera.transform.localPosition = startPosition + new Vector3(x, y, 0);

            yield return null;
        }

        // 恢復原始位置
        mainCamera.transform.localPosition = originalPosition;
        shakeCoroutine = null;
    }

    /// <summary>
    /// 立即停止晃動並恢復位置
    /// </summary>
    public void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        if (mainCamera != null)
        {
            mainCamera.transform.localPosition = originalPosition;
        }
    }

    /// <summary>
    /// 更新原始位置（當攝影機移動時使用）
    /// </summary>
    public void UpdateOriginalPosition()
    {
        if (mainCamera != null && shakeCoroutine == null)
        {
            originalPosition = mainCamera.transform.localPosition;
        }
    }
}

